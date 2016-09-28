/*********************************************************************************
*   (c) 2009 / Gabriel RABHI
*   SHARED OBJECT TOOLKIT CLASS LIBRARY
*********************************************************************************/
using SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator;

namespace SharedObjectToolkitClassLibrary.Memory.IndexPools {
    public unsafe struct StackedIndexPoolUnsafe {
        private int* _entries;
        private int _bottom;
        private int _capacity;
        private int _idOffset;

        public StackedIndexPoolUnsafe(int capacity, int idOffset) {
            _idOffset = idOffset;
            _entries = (int*)HeapAllocator.New(capacity * sizeof(int));
            for (int i = 0; i < capacity; i++)
                _entries[i] = i + idOffset;
            _bottom = 0;
            _capacity = capacity;
        }

        public void Release() {
            if (_entries != null) {
                HeapAllocator.Free((byte*)_entries);
                _entries = null;
            }
        }

        private void EnsureCpacity(int n) {
            if (n >= _capacity) {
                int newCapacity = _capacity * 2;
                int* newEntries = (int*)HeapAllocator.New(newCapacity * sizeof(int));
                MemoryHelper.Copy((byte*)_entries, (byte*)newEntries, _capacity * sizeof(int));
                HeapAllocator.Free((byte*)_entries);
                for (int i = _capacity; i < newCapacity; i++)
                    newEntries[i] = i + _idOffset;
                _entries = newEntries;
                _capacity = newCapacity;
            }
        }

        public int Capacity {
            get { return _capacity; }
        }

        public int FreeCount {
            get { return _capacity - _bottom; }
        }

        public int Pop() {
            EnsureCpacity(_bottom + 1);
            if (_bottom < Capacity)
                return _entries[_bottom++];
            else
                return -1;
        }

        public void Push(int v) {
            if (_bottom > 0)
                _entries[--_bottom] = v;
        }
    }
}
