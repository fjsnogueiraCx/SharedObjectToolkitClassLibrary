using System;
using System.Collections.Generic;
using SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator;

namespace SharedObjectToolkitClassLibrary.Memory {
    public unsafe class LinkedIndexPool {
        private object _locker = new object();
        private LinkedIndexPoolEntry* _entries;
        private LinkedIndexPoolQueue* _queues;
        private int _capacity;
        private int _queueCount;
        private StackedIndexPoolUnsafe _queueStack;
        private bool checkIt = false;

        public LinkedIndexPool(int capacity, int queueCount, bool check = false) {
            if (capacity < 8)
                capacity = 8;
            if (queueCount < 2)
                queueCount = 2;
            // -------- Initialize entries
            _entries = (LinkedIndexPoolEntry*)HeapAllocator.New(sizeof(LinkedIndexPoolEntry) * capacity);
            _capacity = capacity;
            for (int i = 0; i < capacity; i++) {
                _entries[i].Previous = i - 1;
                _entries[i].Next = i + 1;
                _entries[i].Index = i;
                _entries[i].Queue = 0;
            }
            _entries[0].Previous = -1;
            _entries[capacity - 1].Next = -1;
            // -------- Initialize queues
            _queues = (LinkedIndexPoolQueue*)HeapAllocator.New(sizeof(LinkedIndexPoolQueue) * queueCount);
            _queueCount = queueCount;
            for (int i = 0; i < queueCount; i++) {
                _queues[i].First = _queues[i].Last = -1;
                _queues[i].Index = i;
                _queues[i].Count = 0;
            }
            _queues[0].First = 0;
            _queues[0].Last = capacity - 1;
            _queues[0].Count = capacity;
            // -------- Initialize queues stack
            _queueStack = new StackedIndexPoolUnsafe(queueCount, 2);
            // -------- All is ok...
            checkIt = check;
        }

        public void Dispose() {
            if (_queueStack != null) {
                HeapAllocator.Free((byte*)_entries);
                HeapAllocator.Free((byte*)_queues);
                _queueStack = null;
            }
        }

        ~LinkedIndexPool() {
            Dispose();
        }

        public int Capacity {
            get { return _capacity; }
        }

        public int MaxQueueCount {
            get { return _queueStack.Capacity; }
        }

        public int FreeQueueCount {
            get { return _queueStack.FreeCount; }
        }

        public LinkedIndexPoolEntry* Entries {
            get { return _entries; }
        }

        /********************************************************************************
		* PRIVATE
		********************************************************************************/
        private void Dettach(LinkedIndexPoolEntry* e) {
            if (e->Queue > -1) {
                LinkedIndexPoolQueue* eq = &_queues[e->Queue];
                if (e->Index == eq->First) {
                    if (e->Next != -1) {
                        eq->First = e->Next;
                        _entries[eq->First].Previous = -1;
                    } else {
                        eq->First = eq->Last = -1;
                    }
                } else if (e->Index == eq->Last) {
                    if (e->Previous != -1) {
                        eq->Last = e->Previous;
                        _entries[eq->Last].Next = -1;
                    } else {
                        eq->First = eq->Last = -1;
                    }
                } else {
                    if (e->Next != -1)
                        _entries[e->Next].Previous = e->Previous;
                    if (e->Previous != -1)
                        _entries[e->Previous].Next = e->Next;
                }
                e->Next = e->Previous = e->Queue = -1;
                eq->Count--;
            }
            CheckCoherency();
        }

        private void MoveAtEnd(LinkedIndexPoolEntry* e, LinkedIndexPoolQueue* tq) {
            // -------- Dettach e
            Dettach(e);
            // -------- Attach e
            if (tq->Last == -1) {
                // -------- Empty queue
                tq->First = tq->Last = e->Index;
                tq->Count = 1;
                e->Queue = tq->Index;
            } else {
                _entries[tq->Last].Next = e->Index;
                e->Previous = tq->Last;
                tq->Last = e->Index;
                e->Queue = tq->Index;
                tq->Count++;
            }
            CheckCoherency();
        }

        private void MoveAtStart(LinkedIndexPoolEntry* e, LinkedIndexPoolQueue* tq) {
            // -------- Dettach e
            Dettach(e);
            // -------- Attach e
            if (tq->First == -1) {
                // -------- Empty queue
                tq->First = tq->Last = e->Index;
                tq->Count = 1;
                e->Queue = tq->Index;
            } else {
                _entries[tq->First].Previous = e->Index;
                e->Next = tq->First;
                tq->First = e->Index;
                e->Queue = tq->Index;
                tq->Count++;
            }
            CheckCoherency();
        }

        private void CheckCoherency() {
            if (!checkIt)
                return;
            for (int i = 0; i < Capacity; i++) {
                LinkedIndexPoolEntry e = _entries[i];
                if (e.Next != -1 && _entries[e.Next].Previous != e.Index)
                    throw new Exception("Check : link (next) incoherency.");
                if (e.Next != -1 && _entries[e.Next].Queue != e.Queue)
                    throw new Exception("Check : linqued entries queue are not the same.");
                if (e.Previous != -1 && _entries[e.Previous].Next != e.Index)
                    throw new Exception("Check : link (previous) incoherency.");
                if (e.Previous != -1 && _entries[e.Previous].Queue != e.Queue)
                    throw new Exception("Check : linqued entries queue are not the same.");
            }
            for (int i = 0; i < _queueCount; i++) {
                LinkedIndexPoolQueue q = _queues[i];
                if (q.Count < 0)
                    throw new Exception("Check : Count cannot be less than 0.");
                if ((q.First == -1 || q.Last == -1) && (q.First != q.Last))
                    throw new Exception("Check : when First or List equal -1, both must be -1.");
                if (q.First != -1 && q.Count == 0)
                    throw new Exception("Check : queue cannot have entries and count 0.");
                if (q.First != -1) {
                    if (_entries[q.First].Previous != -1)
                        throw new Exception("Check : first queue entry Previous is not -1.");
                    if (_entries[q.Last].Next != -1)
                        throw new Exception("Check : last queue entry Next is not -1.");
                }
            }
        }

        private void CheckCapacity() {
            if (_queues[0].Count <= 5) {
                // -------- Multiply by 2 the capacity
                LinkedIndexPoolEntry* olds = _entries;
                int newCapacity = _capacity * 2;
                _entries = (LinkedIndexPoolEntry*)HeapAllocator.New(sizeof(LinkedIndexPoolEntry) * newCapacity);
                MemoryHelper.Copy((byte*)olds, (byte*)_entries, sizeof(LinkedIndexPoolEntry) * _capacity);
                for (int i = _capacity; i < newCapacity; i++) {
                    _entries[i].Previous = i - 1;
                    _entries[i].Next = i + 1;
                    _entries[i].Index = i;
                    _entries[i].Queue = 0;
                }
                _entries[newCapacity - 1].Next = -1;
                // -------- Fix 0 queue : 
                _entries[_queues[0].Last].Next = _capacity;
                _entries[_capacity].Previous = _queues[0].Last;
                _queues[0].Last = newCapacity - 1;
                _queues[0].Count += _capacity;
                // -------- 
                _capacity = newCapacity;
                HeapAllocator.Free((byte*)olds);
                CheckCoherency();
            }
        }

        /********************************************************************************
		* PUBLIC
		********************************************************************************/
        public int Pop() {
            lock (_locker) {
                CheckCapacity();
                LinkedIndexPoolEntry* e = &_entries[_queues[0].First];
                MoveAtStart(e, &_queues[1]);
                if (e->Index == 0) {
                    e = &_entries[_queues[0].First];
                    MoveAtStart(e, &_queues[1]);
                    return e->Index;
                } else return e->Index;
            }
        }

        public int Pop(LinkedIndexPoolPopMode mode) {
            lock (_locker) {
                CheckCapacity();
                switch (mode) {
                    case LinkedIndexPoolPopMode.TakeFirstAddAtStart:
                    {
                        LinkedIndexPoolEntry* e = &_entries[_queues[0].First];
                        MoveAtStart(e, &_queues[1]);
                        if (e->Index == 0) {
                            e = &_entries[_queues[0].First];
                            MoveAtStart(e, &_queues[1]);
                            return e->Index;
                        } else return e->Index;
                    }
                    case LinkedIndexPoolPopMode.TakeLastAddAtStart:
                    {
                        LinkedIndexPoolEntry* e = &_entries[_queues[0].Last];
                        MoveAtStart(e, &_queues[1]);
                        if (e->Index == 0) {
                            e = &_entries[_queues[0].Last];
                            MoveAtStart(e, &_queues[1]);
                            return e->Index;
                        } else return e->Index;
                    }
                    case LinkedIndexPoolPopMode.TakeFirstAddAtEnd:
                    {
                        LinkedIndexPoolEntry* e = &_entries[_queues[0].First];
                        MoveAtEnd(e, &_queues[1]);
                        if (e->Index == 0) {
                            e = &_entries[_queues[0].First];
                            MoveAtEnd(e, &_queues[1]);
                            return e->Index;
                        } else return e->Index;
                    }
                    case LinkedIndexPoolPopMode.TakeLastAddAtEnd:
                    {
                        LinkedIndexPoolEntry* e = &_entries[_queues[0].Last];
                        MoveAtEnd(e, &_queues[1]);
                        if (e->Index == 0) {
                            e = &_entries[_queues[0].Last];
                            MoveAtEnd(e, &_queues[1]);
                            return e->Index;
                        } else return e->Index;
                    }
                }
                return 0;
            }
        }

        public void Push(int index) {
            if (index == 0)
                throw new Exception("LinkedIndexPool : index invalide.");
            lock (_locker) {
                LinkedIndexPoolEntry* e = &_entries[index];
                if (e->Queue == 0)
                    throw new Exception("Entry already in zero queue.");
                MoveAtStart(e, &_queues[0]);
            }
        }

        /********************************************************************************/
        // -------- Queues
        public int GetFreeQueue() {
            lock (_locker) {
                int tmp = _queueStack.Pop();
                if (_queueStack.Capacity > _queueCount) {
                    int newQueueCount = _queueStack.Capacity;
                    LinkedIndexPoolQueue* _oldQueues = _queues;
                    _queues = (LinkedIndexPoolQueue*)HeapAllocator.New(sizeof(LinkedIndexPoolQueue) * newQueueCount);
                    for (int i = 0; i < _queueCount; i++) {
                        _queues[i] = _oldQueues[i];
                    }
                    for (int i = _queueCount; i < newQueueCount; i++) {
                        _queues[i].First = _queues[i].Last = -1;
                        _queues[i].Index = i;
                        _queues[i].Count = 0;
                    }
                }
                return tmp;
            }
        }

        public void ReleaseQueue(int queue) {
            if (queue < 2)
                throw new Exception("LinkedIndexPool : invalid queue index : " + queue);
            lock (_locker) {
                if (_queues[queue].Last != -1) {
                    do {
                        LinkedIndexPoolEntry* e = &_entries[_queues[queue].Last];
                        MoveAtEnd(e, &_queues[0]);
                    } while (_queues[queue].Last != -1);
                }
                _queueStack.Push(queue);
            }
        }

        // -------- WARNING : EnqueueNew is BUGGED !
        public int EnqueueNew(int queue) {
            if (queue < 2)
                throw new Exception("LinkedIndexPool : invalid queue index : " + queue);
            lock (_locker) {
                LinkedIndexPoolEntry* e = &_entries[_queues[0].First];
                MoveAtEnd(e, &_queues[queue]);
                return e->Index;
            }
        }

        public void Enqueue(int index, int queue) {
            if (index < 0)
                throw new Exception("LinkedIndexPool : invalid index : " + index);
            if (queue < 2)
                throw new Exception("LinkedIndexPool : invalid queue index : " + queue);
            lock (_locker) {
                LinkedIndexPoolEntry* e = &_entries[index];
                MoveAtEnd(e, &_queues[queue]);
            }
        }

        public int Dequeue(int queue) {
            if (queue < 2)
                throw new Exception("LinkedIndexPool : invalid queue index : " + queue);
            lock (_locker) {
                if (_queues[queue].First != -1) {
                    LinkedIndexPoolEntry* e = &_entries[_queues[queue].First];
                    MoveAtEnd(e, &_queues[1]);
                    return e->Index;
                } else
                    return -1;
            }
        }

        public void AddStructToQueue(int index, int queue) {
            if (GetEntryQueue(index) != queue)
                Enqueue(index, queue);
        }

        public void RemoveFromQueue(int index, int queue) {
            if (GetEntryQueue(index) == queue)
                Dettach(&_entries[index]);
        }

        public int GetQueueLenght(int queue) {
            return _queues[queue].Count;
        }

        public int GetEntryQueue(int index) {
            if (index < 0)
                return 0;
            return _entries[index].Queue;
        }

        public List<int> QueueAsList(int queue) {
            List<int> r = new List<int>();
            lock (_locker) {
                if (_queues[queue].First != -1) {
                    LinkedIndexPoolEntry* e = &_entries[_queues[queue].First];
                    do {
                        r.Add(e->Index);
                        if (e->Next != -1) {
                            e = &_entries[e->Next];
                            if (e->Next == -1)
                                r.Add(e->Index);
                        } else break;
                    } while (e->Next != -1);
                }
            }
            return r;
        }

        public int FirstOfQueue(int queue) {
            return _queues[queue].First;
        }
    }
}