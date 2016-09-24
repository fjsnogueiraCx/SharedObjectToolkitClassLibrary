using System;
using System.Runtime.InteropServices;
using SharedObjectToolkitClassLibrary.VirtualObject;

namespace SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator {
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
    public unsafe struct SegmentHeader {
        public static readonly int SIZE = sizeof (SegmentHeader); // 32 bytes
        // -------- Block index in the segment
        [System.Runtime.InteropServices.FieldOffset(0)]
        public int BlockIndex;
        // -------- Current used memory in the block
        [System.Runtime.InteropServices.FieldOffset(4)]
        public int PtrSize;
        // -------- Size of usable memory in the block
        [System.Runtime.InteropServices.FieldOffset(8)]
        public int BlocksSize;
        // -------- Sgment index in the whole memory manager
        [System.Runtime.InteropServices.FieldOffset(12)]
        private uint _segmentIndex;
        // -------- Number of SmartPointer (or other datastructure) referencing this block, wite the promess to remove it when not anymore used
        [System.Runtime.InteropServices.FieldOffset(16)]
        public volatile int ReferenceCount;
        // -------- Virtual object type identifier - or user data...
        [System.Runtime.InteropServices.FieldOffset(24)]
        public FactoryTypeIdentifier TypeIdentifier;
        // -------- Size of usable memory in the block
        [System.Runtime.InteropServices.FieldOffset(28)]
        public int User;

        public int GetPtrSize(byte* ptr) {
            return ((SegmentHeader*) (ptr - SIZE))->PtrSize;
        }

        /*public int SegmentIndex {
            get {
                return _segmentIndex;
            }
            set {
                _segmentIndex = value;
            }
        }*/

        public byte PartitionIndex {
            get {
                return (byte)((_segmentIndex & 0xFF000000) >> 24);
            }
            set {
                _segmentIndex = (uint)((_segmentIndex & 0x00FFFFFF) | (value << 24));
            }
        }

        public uint SegmentIndex {
            get {
                return (_segmentIndex & 0x00FFFFFF);
            }
            set {
                if (value > 0x00FFFFFF)
                    throw new Exception("SegmentIndex cannot be grater than 0x00FFFFFF");
                _segmentIndex = (uint)((_segmentIndex & 0xFF000000) | (value & 0x00FFFFFF));
            }
        }

        public void CheckCoherency() {
            if (BlockIndex < 0 || PtrSize < 0 || BlocksSize < 0 || SegmentIndex == 0x00FFFFFF || SegmentIndex > MemoryAllocatorPartition.SEGMENT_COUNT)
                throw new Exception("Bad Memory Block Header.");
        }

        public void Invalidate() {
            BlockIndex = PtrSize = BlocksSize = -1;
            PartitionIndex = 0;
            SegmentIndex = 0x00FFFFFF;
            TypeIdentifier.TypeCode = TypeIdentifier.Version = 0;
        }
    }
}