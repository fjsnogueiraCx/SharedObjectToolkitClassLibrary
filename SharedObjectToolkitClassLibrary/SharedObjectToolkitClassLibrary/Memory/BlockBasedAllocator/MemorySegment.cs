using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator {
    [Flags]
    public enum MemoryMode {
        ModifyReferenceCounters,
        FreeIfNullCounters
    }

    public unsafe struct MemorySegment {
        private LinkedIndexPool.LinkedIndexPool _blockPool;
        private int _blocksSize;
        private uint _segmentIndex;
        private byte _partitionIndex;
        private int _physicalBlockSize;
        private int _blockCount;
        private int _bufferSize;
        private byte* _data;
        //private GCHandle _memeory;
        private int _freeBlocks;

        public void Build(int segmentSize, int blockSize, uint segmentIndex, byte partitionIndex, out int realBlockedMemory) {
            // -------- Who i am ?
            _blocksSize = blockSize;
            _segmentIndex = segmentIndex;
            _partitionIndex = partitionIndex;
            // -------- Compute
            _blockCount = segmentSize / blockSize;
            _freeBlocks = _blockCount;
            _physicalBlockSize = SegmentHeader.SIZE + blockSize;
            // -------- Allocate
            _blockPool = new LinkedIndexPool.LinkedIndexPool(_blockCount, 2);
            _bufferSize = _physicalBlockSize * (_blockCount + 1);
            //_memory = GCHandle.Alloc(new byte[_bufferSize], GCHandleType.Pinned);
            _data = (byte*)Marshal.AllocHGlobal(_bufferSize).ToPointer();
            realBlockedMemory = _bufferSize;
        }

        public void Dispose(out int realReleasedMemory) {
            realReleasedMemory = 0;
            if (_blockPool != null) {
                _blockPool.Dispose();
                _blockPool = null;
                HeapAllocator.Free(_data);
                realReleasedMemory = _bufferSize;
            }
        }

        public byte* Malloc(int size, bool counterBased = false) {
            int idx = _blockPool.Pop();
            var ptr = &_data[idx * _physicalBlockSize];
            var header = (SegmentHeader*)ptr;
            header->BlockIndex = idx;
            header->PtrSize = size;
            header->BlocksSize = _blocksSize;
            header->SegmentIndex = _segmentIndex;
            header->PartitionIndex = _partitionIndex;
            header->ReferenceCount = counterBased ? 1 : 0;
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
}