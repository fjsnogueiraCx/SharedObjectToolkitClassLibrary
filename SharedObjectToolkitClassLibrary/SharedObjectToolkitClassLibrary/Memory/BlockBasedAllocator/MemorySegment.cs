/*********************************************************************************
*   (c) 2010 - 2016 / Gabriel RABHI
*   SHARED OBJECT TOOLKIT CLASS LIBRARY
*********************************************************************************/
using System.Runtime.InteropServices;
using System.Threading;
using SharedObjectToolkitClassLibrary.Memory.IndexPools;

namespace SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator {
    public unsafe struct MemorySegment {
        private LinkedIndexPool _blockPool;
        private int _blocksSize;
        private int _segmentIndex;
        private int _physicalBlockSize;
        private int _blockCount;
        private int _bufferSize;
        private byte* _data;
        private int _freeBlocks;

        public void Build(int segmentSize, int blockSize, int segmentIndex, out int realBlockedMemory) {
            // -------- Who i am ?
            _blocksSize = blockSize;
            _segmentIndex = segmentIndex;
            // -------- Compute
            _blockCount = segmentSize / blockSize;
            _freeBlocks = _blockCount;
            _physicalBlockSize = BlockHeader.SIZE + blockSize;
            // -------- Allocate
            _blockPool = new LinkedIndexPool(_blockCount, 2);
            _bufferSize = _physicalBlockSize * (_blockCount + 1);
            _data = (byte*)Marshal.AllocHGlobal(_bufferSize).ToPointer();
            realBlockedMemory = _bufferSize;
        }

        public void Dispose(out int realReleasedMemory) {
            realReleasedMemory = _bufferSize;
            if (_blocksSize > 0) {
                _blockPool.Release();
                HeapAllocator.Free(_data);
                realReleasedMemory = 0;
            }
        }

        public byte* Malloc(int size, bool counterBased) {
            int idx = _blockPool.Pop();
            var ptr = &_data[idx * _physicalBlockSize];
            var header = (BlockHeader*)ptr;
            header->BlockIndex = idx;
            header->PtrSize = size;
            header->BlocksSize = _blocksSize;
            header->SegmentIndex = _segmentIndex;
            header->ReferenceCount = counterBased ? 1 : 0;
            Interlocked.Decrement(ref _freeBlocks);
            return ptr + BlockHeader.SIZE;
        }

        public void Free(byte* ptr) {
            var header = (BlockHeader*)(ptr - BlockHeader.SIZE);
            _blockPool.Push(header->BlockIndex);
            header->Invalidate();
            Interlocked.Increment(ref _freeBlocks);
        }

        public int FreeBlocks { get { return _freeBlocks; } }

        public bool NoAllocatedBlocks { get { return _freeBlocks == _blockCount; } }
    }
}