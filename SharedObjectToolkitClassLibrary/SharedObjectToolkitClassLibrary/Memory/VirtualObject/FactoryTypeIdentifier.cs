using System.Runtime.InteropServices;

namespace SharedObjectToolkitClassLibrary.BlockBasedAllocator {
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    public unsafe struct FactoryTypeIdentifier {
        public short TypeCode;
        public short Version;
    }
}