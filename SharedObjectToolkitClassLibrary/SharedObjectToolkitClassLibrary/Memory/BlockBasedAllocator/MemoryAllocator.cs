using System;
using System.Linq;
using System.Threading;

namespace SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator {
    public static unsafe class MemoryAllocator {
        static MemoryAllocatorPartition[] _partitions = new MemoryAllocatorPartition[Math.Min(Math.Max(Environment.ProcessorCount/2,2),128)];
        private static int _allocationCircularCounter = 0;
        private static long _lockFailed = 0;

        static MemoryAllocator() {
            byte i = 0;
            foreach (var p in _partitions) {
                _partitions[i] = new MemoryAllocatorPartition();
                _partitions[i].PartitionIndex = i++;
            }
        }

        public static byte* New(int size, bool counterBased = false, bool overSized = false) {
            int temptatives = 0;
            do {
                var p = _partitions[Interlocked.Increment(ref _allocationCircularCounter) % (_partitions.Length - 1)];
                if (Monitor.TryEnter(p._locker)) {
                    try {
                        return p.New(size, counterBased, overSized);
                    } finally {
                        Monitor.Exit(p._locker);
                    }
                }
            } while (temptatives++ < _partitions.Length);
            Interlocked.Increment(ref _lockFailed);
            return _partitions[Interlocked.Increment(ref _allocationCircularCounter) % (_partitions.Length - 1)].New(size, counterBased, overSized);
        }

        public static void Free(byte* ptr, bool counterBased = false) {
            _partitions[PartitionIndex(ptr)].Free(ptr, counterBased);
        }

        public static byte* ChangeSize(byte* ptr, int newSize, bool counterBased = false) {
            return _partitions[PartitionIndex(ptr)].ChangeSize(ptr, newSize, counterBased);
        }

        public static int SizeOf(byte* ptr) {
            var header = ((SegmentHeader*)(ptr - SegmentHeader.SIZE));
            header->CheckCoherency();
            return header->PtrSize;
        }

        public static int Reserve(byte* ptr) {
            var header = ((SegmentHeader*)(ptr - SegmentHeader.SIZE));
            header->CheckCoherency();
            return header->BlocksSize - header->PtrSize;
        }

        public static int PartitionIndex(byte* ptr) {
            var header = ((SegmentHeader*)(ptr - SegmentHeader.SIZE));
            header->CheckCoherency();
            return header->PartitionIndex;
        }

        public static long TotalAllocatedMemory {
            get {
                return _partitions.Sum(partition => partition.TotalAllocatedMemory);
            }
        }

        public static long BlockCount {
            get {
                return _partitions.Sum(partition => partition.BlockCount);
            }
        }

        public static long SegmentCount {
            get {
                return _partitions.Sum(partition => partition.SegmentCount);
            }
        }

        public static double EfficiencyRatio {
            get {
                return _partitions.Average(partition => partition.EfficiencyRatio);
            }
        }

        public static long LockFailed { get { return _lockFailed; } }
    }
}