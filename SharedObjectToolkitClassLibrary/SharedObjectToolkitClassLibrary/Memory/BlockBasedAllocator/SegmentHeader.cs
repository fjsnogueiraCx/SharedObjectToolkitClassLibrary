using System;
using System.Runtime.InteropServices;

namespace SharedObjectToolkitClassLibrary.BlockBasedAllocator {
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    public unsafe struct SegmentHeader {
        public static readonly int SIZE = sizeof (SegmentHeader); // 32 bytes
        // -------- Block index in the segment
        public int BlockIndex;
        // -------- Current used memory in the block
        public int PtrSize;
        // -------- Size of usable memory in the block
        public int BlocksSize;
        // -------- Sgment index in the whole memory manager
        public int SegmentIndex;
        // -------- Number of SmartPointer (or other datastructure) referencing this block, wite the promess to remove it when not anymore used
        public volatile int ReferenceCount;
        // -------- Virtual object type identifier - or user data...
        public FactoryTypeIdentifier TypeIdentifier;

        public int GetPtrSize(byte* ptr) {
            return ((SegmentHeader*) (ptr - SIZE))->PtrSize;
        }

        public void CheckCoherency() {
            if (BlockIndex < 0 || PtrSize < 0 || BlocksSize < 0 || SegmentIndex < 0 || SegmentIndex > MemoryAllocator.SEGMENT_COUNT)
                throw new Exception("Bad Memory Block Header.");
        }

        public void Invalidate() {
            BlockIndex = PtrSize = BlocksSize = SegmentIndex = -1;
            TypeIdentifier.TypeCode = TypeIdentifier.Version = 0;
        }
    }
}