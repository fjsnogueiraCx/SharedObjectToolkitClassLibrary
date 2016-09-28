using System.Runtime.InteropServices;

namespace SharedObjectToolkitClassLibrary.VirtualObject {
    // -------- FLags ? Compressed hybernation + Compressed persistance
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    public unsafe struct FactoryTypeIdentifier {
        public ushort TypeCode;
        public short Version;

        public FactoryTypeIdentifier(ushort t, short v) {
            TypeCode = t;
            Version = v;
        }
    }
}