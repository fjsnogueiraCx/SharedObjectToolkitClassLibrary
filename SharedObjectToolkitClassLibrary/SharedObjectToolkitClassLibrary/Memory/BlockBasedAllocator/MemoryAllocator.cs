using System;
using System.Runtime.InteropServices;
using System.Threading;
using SharedObjectToolkitClassLibrary.Memory;

namespace SharedObjectToolkitClassLibrary.BlockBasedAllocator {
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    public unsafe struct SegmentHeader {
        public static readonly int SIZE = sizeof(SegmentHeader);
        public int BlockIndex;
        public int PtrSize;
        public int BlocksSize;
        public int SegmentIndex;

        public int GetPtrSize(byte* ptr) {
            return ((SegmentHeader*)(ptr - SIZE))->PtrSize;
        }

        public void CheckCoherency() {
            if (BlockIndex < 0 || PtrSize < 0 || BlocksSize < 0 || SegmentIndex < 0 || SegmentIndex > MemoryAllocator.SEGMENT_COUNT)
                throw new Exception("Bad Memory Block Header.");
        }

        public void Invalidate() {
            BlockIndex = PtrSize = BlocksSize = SegmentIndex = -1;
        }
    }

    public unsafe struct MemorySegment {
        private LinkedIndexPool _blockPool;
        private int _blocksSize;
        private int _segmentIndex;
        private int _physicalBlockSize;
        private int _blockCount;
        private int _bufferSize;
        private byte* _data;
        //private GCHandle _memeory;
        private int _freeBlocks;

        public void Build(int segmentSize, int blockSize, int segmentIndex, out int realBlockedMemory) {
            // -------- Who i am ?
            _blocksSize = blockSize;
            _segmentIndex = segmentIndex;
            // -------- Compute
            _blockCount = segmentSize / blockSize;
            _freeBlocks = _blockCount;
            _physicalBlockSize = SegmentHeader.SIZE + blockSize;
            // -------- Allocate
            _blockPool = new LinkedIndexPool(_blockCount, 2);
            _bufferSize = _physicalBlockSize * (_blockCount + 1);
            //_memory = GCHandle.Alloc(new byte[_bufferSize], GCHandleType.Pinned);
            _data = (byte*)Marshal.AllocHGlobal(_bufferSize).ToPointer();
            realBlockedMemory = _bufferSize;
        }

        public void Dispose(out int realReleasedMemory) {
            realReleasedMemory = 0;
            if (_blockPool != null) {
                _blockPool.Dispose();
                //_memory.Free();
                Marshal.FreeHGlobal(new IntPtr(_data));
                realReleasedMemory = _bufferSize;
            }
        }

        public byte* Malloc(int size) {
            int idx = _blockPool.Pop();
            var ptr = &_data[idx * _physicalBlockSize];
            var header = (SegmentHeader*)ptr;
            header->BlockIndex = idx;
            header->PtrSize = size;
            header->BlocksSize = _blocksSize;
            header->SegmentIndex = _segmentIndex;
            Interlocked.Decrement(ref _freeBlocks);
            return ptr + SegmentHeader.SIZE;
        }

        public void Free(byte* ptr) {
            var header = (SegmentHeader*)(ptr - SegmentHeader.SIZE);
            _blockPool.Push(header->BlockIndex);
            header->Invalidate();
            Interlocked.Increment(ref _freeBlocks);
        }

        public int FreeBlocks { get { return _freeBlocks; } }

        public bool NoAllocatedBlocks { get { return _freeBlocks == _blockCount; } }
    }

    public unsafe class MemoryAllocator {
        public static readonly int SEGMENT_COUNT = 1024 * 128; // 64Go

        private MemorySegment[] _segments = new MemorySegment[SEGMENT_COUNT];
        private LinkedIndexPool _pool = new LinkedIndexPool(SEGMENT_COUNT, 1000);
        private long _totalMemory = 0;
        private long _totalBlocks = 0;
        private long _totalBufferSpace = 0;

        private int SizeToQueue(int size) {
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

        private void SizeToSegmentProperties(int size, out int segSize, out int blockSize) {
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

        public byte* Malloc(int size) {
            byte* ptr = null;
            int q = SizeToQueue(size);
            int idx = _pool.FirstOfQueue(q);
            if (idx == -1) {
                idx = _pool.Pop();
                _pool.Enqueue(idx, q);
                int segsSize = 0;
                int blocksSize = 0;
                int mem;
                SizeToSegmentProperties(size, out segsSize, out blocksSize);
                _segments[idx].Build(segsSize, blocksSize, idx, out mem);
                ptr = _segments[idx].Malloc(size);
                Interlocked.Add(ref _totalBufferSpace, mem);
                Interlocked.Add(ref _totalMemory, size);
                Interlocked.Increment(ref _totalBlocks);
            } else {
                ptr = _segments[idx].Malloc(size);
                if (_segments[idx].FreeBlocks == 0)
                    _pool.Enqueue(idx, q + 1);
                Interlocked.Add(ref _totalMemory, size);
                Interlocked.Increment(ref _totalBlocks);
            }
            return ptr;
        }

        public void Free(byte* ptr) {
            var header = ((SegmentHeader*)(ptr - SegmentHeader.SIZE));
            header->CheckCoherency();
            int idx = header->SegmentIndex;
            Interlocked.Add(ref _totalMemory, -header->PtrSize);
            Interlocked.Decrement(ref _totalBlocks);
            _segments[idx].Free(ptr);
            if (_segments[idx].NoAllocatedBlocks) {
                // -------- Faire en sorte qu'on garde au moins un segment de chaque taille
                int mem;
                _segments[idx].Dispose(out mem);
                Interlocked.Add(ref _totalBufferSpace, -mem);
                _pool.Push(idx);
            } else {
                var q = _pool.GetEntryQueue(idx);
                if (q % 2 != 0)
                    _pool.Enqueue(idx, q - 1);
            }
        }

        public byte* Realloc(byte* ptr, int newSize) {
            return null;
        }

        public long TotalAllocatedMemory { get { return _totalMemory; } }

        public long BlocksCount { get { return _totalBlocks; } }

        public double EfficiencyRatio { get { return ((double)_totalMemory / (double)_totalBufferSpace); } }
    }

}