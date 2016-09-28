/*********************************************************************************
*   (c) 2010 - 2016 / Gabriel RABHI
*   SHARED OBJECT TOOLKIT CLASS LIBRARY
*********************************************************************************/
using System;
using System.Runtime.InteropServices;

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
