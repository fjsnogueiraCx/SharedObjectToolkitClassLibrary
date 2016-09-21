using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator {
    public unsafe class HeapAllocator {
        public static byte* New(int s ) {
            return (byte*) Marshal.AllocHGlobal(s).ToPointer();
        }

        public static void Free(byte* ptr) {
            Marshal.FreeHGlobal(new IntPtr(ptr));
        }
    }
}
