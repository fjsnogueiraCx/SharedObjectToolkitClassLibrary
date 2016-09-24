using System.Runtime.InteropServices;

namespace SharedObjectToolkitClassLibrary.VirtualObject {
    // -------- FLags ? Compressed hybernation + Compressed persistance
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    public unsafe struct FactoryTypeIdentifier {
        public short TypeCode;
        public short Version;
    }
}