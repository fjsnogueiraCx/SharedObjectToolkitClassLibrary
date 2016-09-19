using System;
using System.Runtime.InteropServices;

namespace SharedObjectToolkitClassLibrary.BlockBasedAllocator {
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    public unsafe struct SegmentHeader {
        public static readonly int SIZE = sizeof(SegmentHeader);
        public int BlockIndex;
        public int PtrSize;
        public int BlocksSize;
        public int SegmentIndex;
        public volatile int ReferenceCount;
        public int UserValue;
        // ------- +8 octets de donnée attachées, pour le RTTI par exemple ? Ou pour l'utilisateur ?
        // -------- Référence counter pour les SmartPointer ? 4 octets RTTI + 4 octets de RefCounter
        // -------- Pourquoi pas laisser l'utilisateur choisir l'espace alloué à chaque pointeur ? Comme cela, il se fait ses allocators spécifiques...

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
}