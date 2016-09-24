using System;
using System.Threading;

namespace SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator {
    public unsafe class MemoryAllocatorPartition {
        public static readonly int SEGMENT_COUNT = Math.Min(1024 * 64, 0x00FFFFFF); // min 120 Go -> max 8 To
        public object _locker = new object();
        private MemorySegment[] _segments = new MemorySegment[SEGMENT_COUNT];
        private LinkedIndexPool.LinkedIndexPool _pool = new LinkedIndexPool.LinkedIndexPool(SEGMENT_COUNT, 1000);
        private byte _partitionIndex = 0;
        private long _totalMemory = 0;
        private long _totalBlockCount = 0;
        private long _totalBufferSpace = 0;
        private long _totalSegmentCount = 0;

        private static int MinSize { get { return 1024*2048; } }

        private static int SizeToQueue(int size) {
            int q = 0;
            if (size < 1024) {
                q = 100 + ((size / 64) * 2);
            } else if (size >= 1024 && size < 8192) {
                q = 200 + (((size - 1024) / 512) * 2);
            } else if (size >= 8192 && size < 65536) {
                q = 300 + (((size - 8192) / 4096) * 2);
            } else if (size >= 65536 && size < 524288) {
                q = 400 + (((size - 65536) / 32768) * 2);
            } else if (size >= 524288 && size < 4194304) {
                q = 500 + (((size - 524288) / 262144) * 2);
            } else if (size >= 4194304 && size < 33554432) {
                q = 600 + (((size - 4194304) / 2097152) * 2);
            }
            return q;
        }

        private static void SizeToSegmentProperties(int size, out int segSize, out int blockSize) {
            int q = 0;
            segSize = 0;
            blockSize = 0;
            if (size < 1024) {
                blockSize = ((size / 64) + 1) * 64;
                segSize = 1024 * 2048; // 2Mo (16 = 32 mo)
            } else if (size >= 1024 && size < 8192) {
                blockSize = ((size / 512) + 1) * 512;
                segSize = 8192 * 1024; // 8Mo (16 = 128 mo)
            } else if (size >= 8192 && size < 65536) {
                blockSize = ((size / 4096) + 1) * 4096;
                segSize = 65536 * 256; // 16Mo (16 = 256 mo)
            } else if (size >= 65536 && size < 524288) {
                blockSize = ((size / 32768) + 1) * 32768;
                segSize = 65536 * 512; // 32Mo  (16 = 512 mo)
            } else if (size >= 524288 && size < 4194304) {
                blockSize = ((size / 262144) + 1) * 262144;
                segSize = 65536 * 512; // 32Mo  (16 = 512 mo)
            } else if (size >= 4194304 && size < 33554432) {
                blockSize = ((size / 2097152) + 1) * 2097152;
                segSize = 65536 * 1024; // 64Mo  (16 = 1 Go)
            }
        }

        // ****************************************************************************************** //
        // ********** STATISTICS
        public byte* New(int size, bool counterBased = false, bool overSized = false) {
            byte* ptr = null;
            int q = SizeToQueue(size + (overSized ? size * 2 : 0));
            lock (_locker) {
                int idx = _pool.FirstOfQueue(q);
                if (idx == -1) {
                    idx = _pool.Pop();
                    _pool.Enqueue(idx, q);
                    int segsSize = 0;
                    int blocksSize = 0;
                    int mem;
                    SizeToSegmentProperties(size + (overSized ? size*2 : 0), out segsSize, out blocksSize);
                    _segments[idx].Build(segsSize, blocksSize, (uint)idx, _partitionIndex, out mem);
                    ptr = _segments[idx].Malloc(size, counterBased);
                    Interlocked.Add(ref _totalBufferSpace, mem);
                    Interlocked.Increment(ref _totalSegmentCount);
                    Interlocked.Add(ref _totalMemory, size);
                    Interlocked.Increment(ref _totalBlockCount);
                } else {
                    ptr = _segments[idx].Malloc(size);
                    if (_segments[idx].FreeBlocks == 0)
                        _pool.Enqueue(idx, q + 1);
                    Interlocked.Add(ref _totalMemory, size);
                    Interlocked.Increment(ref _totalBlockCount);
                }
            }
            return ptr;
        }

        public void Free(byte* ptr, bool counterBased = false) {
            var header = ((SegmentHeader*)(ptr - SegmentHeader.SIZE));
            header->CheckCoherency();
            lock (_locker) {
                if (counterBased && header->ReferenceCount > 1) {
                    Interlocked.Decrement(ref header->ReferenceCount);
                    return;
                }
                int idx = (int) header->SegmentIndex;
                Interlocked.Add(ref _totalMemory, -header->PtrSize);
                Interlocked.Decrement(ref _totalBlockCount);
                _segments[idx].Free(ptr);
                if (_segments[idx].NoAllocatedBlocks) {
                    // -------- Faire en sorte qu'on garde au moins un segment de chaque taille
                    int mem;
                    _segments[idx].Dispose(out mem);
                    Interlocked.Add(ref _totalBufferSpace, -mem);
                    Interlocked.Decrement(ref _totalSegmentCount);
                    _pool.Push(idx);
                } else {
                    var q = _pool.GetEntryQueue(idx);
                    if (q%2 != 0)
                        _pool.Enqueue(idx, q - 1);
                }
            }
        }

        public byte* ChangeSize(byte* ptr, int newSize, bool counterBased = false) {
            var header = ((SegmentHeader*) (ptr - SegmentHeader.SIZE));
            header->CheckCoherency();
            lock (_locker) {
                if (header->BlocksSize >= newSize && newSize >= header->BlocksSize/2 && header->BlocksSize > 64) {
                    header->PtrSize = newSize;
                } else {
                    if (ptr == null)
                        throw new ArgumentException("Null parameter.", "data");
                    if (newSize < 0)
                        throw new ArgumentException("Invalid parameter.", "newSize");
                    byte* newBuffer = ptr;
                    var dataLength = MemoryAllocator.SizeOf(ptr);
                    if (dataLength != newSize) {
                        newBuffer = New(newSize, counterBased);
                        MemoryHelper.Copy(ptr, newBuffer, newSize < dataLength ? newSize : dataLength);
                        if (counterBased)
                            Free(ptr, true);
                    }
                    return newBuffer;
                }
            }
            return ptr;
        }

        // ****************************************************************************************** //
        // ********** STATISTICS
        public long TotalAllocatedMemory { get { return _totalMemory; } }

        public long BlockCount { get { return _totalBlockCount; } }

        public long SegmentCount { get { return _totalSegmentCount; } }

        public double EfficiencyRatio { get { return ((double)_totalMemory / (double)_totalBufferSpace); } }

        public byte PartitionIndex { get { return _partitionIndex; } set { _partitionIndex = value; } }
    }
}