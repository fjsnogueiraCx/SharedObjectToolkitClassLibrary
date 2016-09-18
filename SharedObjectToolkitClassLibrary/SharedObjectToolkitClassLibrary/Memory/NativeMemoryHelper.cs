using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharedObjectToolkitClassLibrary
{
    public unsafe static class NativeMemoryHelper
    {
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct Struct1 {
            private byte v;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct Struct2 {
            private byte v;
            private byte v1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct Struct3 {
            private byte v;
            private byte v2;
            private byte v3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct Struct4 {
            private byte v;
            private byte v2;
            private byte v3;
            private byte v4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct Struct5 {
            private byte v;
            private byte v2;
            private byte v3;
            private byte v4;
            private byte v5;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct Struct6 {
            private byte v;
            private byte v2;
            private byte v3;
            private byte v4;
            private byte v5;
            private byte v6;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct Struct7 {
            private byte v;
            private byte v2;
            private byte v3;
            private byte v4;
            private byte v5;
            private byte v6;
            private byte v7;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct Struct8 {
            private byte v;
            private byte v2;
            private byte v3;
            private byte v4;
            private byte v5;
            private byte v6;
            private byte v7;
            private byte v8;
        }

        public static byte* New(int size) {
            return (byte*)Marshal.AllocHGlobal(size);
        }

        public static void Free(byte* ptr) {
            Marshal.FreeHGlobal((IntPtr)ptr);
        }

        public static void Copy(byte* src, byte* dst, int length) {
#if FULL_DEBUG
            CheckWrite(dst, length);
#endif
            if (length < 0)
                throw new ArgumentException("Invalid parameter (" + length + ").", "length");
            if (src == null)
                throw new ArgumentException("Invalid null src parameter.", "src");
            if (dst == null)
                throw new ArgumentException("Invalid null dst parameter.", "dst");
            if (length > 0) {
#if USE_MEMCOPY
                if (length > 256) {
                    CopyMemory((IntPtr)dst, (IntPtr)src, length);
                    return;
                }
#endif
                // -------- Copy 4 * 8 bytes each loop...
                if (length > 96) {
                    long* srcL = (long*)src;
                    long* dstL = (long*)dst;
                    int r = length >> 5;
                    for (int i = 0; i < r; i++) {
                        dstL[0] = srcL[0];
                        dstL[1] = srcL[1];
                        dstL[2] = srcL[2];
                        dstL[3] = srcL[3];
                        dstL += 4;
                        srcL += 4;
                    }
                    r = r << 5;
                    length -= r;
                    if (length > 0) {
                        dst += r;
                        src += r;
                    } else return;
                }
                // -------- Copy 8 bytes each loop...
                if (length > 8) {
                    long* srcL = (long*)src;
                    long* dstL = (long*)dst;
                    int r = length >> 3;
                    for (int i = 0; i < r; i++)
                        *(dstL++) = *(srcL++);
                    r = r << 3;
                    length -= r;
                    if (length > 0) {
                        dst += r;
                        src += r;
                    } else return;
                }
                // -------- Copy remaining bytes...
                if (length > 0) {
                    if (length < 5)
                        if (length < 3)
                            if (length == 1)
                                *((Struct1*)dst) = *((Struct1*)src);
                            else
                                *((Struct2*)dst) = *((Struct2*)src);
                        else
                            if (length == 3)
                            *((Struct3*)dst) = *((Struct3*)src);
                        else
                            *((Struct4*)dst) = *((Struct4*)src);
                    else
                        if (length < 7)
                        if (length == 5)
                            *((Struct5*)dst) = *((Struct5*)src);
                        else
                            *((Struct6*)dst) = *((Struct6*)src);
                    else
                            if (length == 7)
                        *((Struct7*)dst) = *((Struct7*)src);
                    else
                        *((Struct8*)dst) = *((Struct8*)src);
                }
            }
        }
        
        public static void ReverseCopy(byte* src, byte* dst, int length, int delta) {
#if FULL_DEBUG
            CheckWrite(dst, length);
#endif
            if (length < 0)
                throw new ArgumentException("Invalid parameter (" + length + ").", "length");
            if (src == null)
                throw new ArgumentException("Invalid null src parameter.", "src");
            if (dst == null)
                throw new ArgumentException("Invalid null dst parameter.", "dst");
            if (delta >= 8 && length >= 16) {
                // -------- Copier par 8
                long* srcL = (long*)(src + length - 8);
                long* dstL = (long*)(dst + length - 8);
                int r = length >> 3;
                for (int i = 0; i < r; i++) {
                    *dstL = *srcL;
                    dstL--;
                    srcL--;
                }
                r = r << 3;
                length -= r;
                // -------- finaliser
                if (length > 0) {
                    dst = (byte*)dstL;
                    src = (byte*)srcL;
                    dst += 7;
                    src += 7;
                    for (int i = 0; i < length; i++) {
                        *dst = *src;
                        dst--;
                        src--;
                    }
                }
            } else
            if (delta >= 4 && length >= 8) {
                // -------- Copier par 4
                int* srcL = (int*)(src + length - 4);
                int* dstL = (int*)(dst + length - 4);
                int r = length >> 2;
                for (int i = 0; i < r; i++) {
                    *dstL = *srcL;
                    dstL--;
                    srcL--;
                }
                r = r << 2;
                length -= r;
                // -------- finaliser
                if (length > 0) {
                    dst = (byte*)dstL;
                    src = (byte*)srcL;
                    dst += 3;
                    src += 3;
                    for (int i = 0; i < length; i++) {
                        *dst = *src;
                        dst--;
                        src--;
                    }
                }
            } else {
                if (length > 0) {
                    src += length - 1;
                    dst += length - 1;
                    for (int i = 0; i < length; i++) {
                        *dst = *src;
                        dst--;
                        src--;
                    }
                }
            }
        }

        public static void Move(byte* data, int srcOffset, int dstOffset, int length) {
            if (length > 0 && srcOffset != dstOffset) {
                byte* src = &data[srcOffset];
                byte* dst = &data[dstOffset];
                if (srcOffset < dstOffset)
                    ReverseCopy(src, dst, length, dstOffset - srcOffset);
                else
                    Copy(src, dst, length);
            }
        }
    }
}
