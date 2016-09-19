using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using SharedObjectToolkitClassLibrary.BlockBasedAllocator;

namespace SharedObjectToolkitClassLibrary.VirtualObject {
    /// <summary>
    /// A memory reference with control over the real number of references,
    /// and hability to check for MVCC.
    /// A multi referenced block had a ReferenceCount > 1. If == 1 then the smartpointer is alone to reference this bloc.
    /// </summary>
    public unsafe class SmartPointer : IDisposable {
        private byte* _data = null;

        public byte* Data { get { return _data; } }

        public SmartPointer() {
        }

        public SmartPointer(int size) {
            Allocate(size);
        }

        public void StartModification() {
            var header = ((SegmentHeader*) (_data - SegmentHeader.SIZE));
            if (header->ReferenceCount > 1) {
                // Create a copy
            }
        }

        public void Allocate(int size) {
            _data = MemoryAllocator.Malloc(size, true);
        }

        public void Force(byte* ptr) {
            Forget();
            _data = ptr;
            var header = ((SegmentHeader*) (_data - SegmentHeader.SIZE));
            header->CheckCoherency();
            Interlocked.Increment(ref header->ReferenceCount);
        }

        public void ChangeSize(int newSize) {

        }

        public void Forget() {
            if (_data != null) {
                MemoryAllocator.Free(_data, true);
                _data = null;
            }
        }

        public void Dispose() {
            Forget();
        }
    }

    /// <summary>
    /// A repository of virtual objects
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public unsafe class Repository<TKey> {
        private Dictionary<TKey, IntPtr> _map = new Dictionary<TKey, IntPtr>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe class ArrayDescriptor<T> : IArrayDescriptorType where T : struct {
        public Type GetArrayType() {
            return typeof (T);
        }
    }

    public unsafe interface IArrayDescriptorType {
        Type GetArrayType();
    }

    public unsafe struct ArrayPointer {
        public byte* Adress;
        public int Count;
    }

    public unsafe class TypeDescriptor<T> where T : struct {
        private class ArrayTypeDescriptor {
            public int TypeSize;
        }

        private List<ArrayTypeDescriptor> _arrays = new List<ArrayTypeDescriptor>();
        private int _fixedPartLenght;

        public TypeDescriptor(int fixedTSize, params IArrayDescriptorType[] arrays) {
            foreach (var arr in arrays) {
                _arrays.Add(new ArrayTypeDescriptor() {TypeSize = Marshal.SizeOf(arr.GetArrayType())});
            }
            _fixedPartLenght = fixedTSize;
        }

        public SmartPointer Allocate() {
            return new SmartPointer(_fixedPartLenght + (_arrays.Count*4));
        }

        public ArrayPointer GetArray(SmartPointer data, int index) {
            return new ArrayPointer();
        }

        public byte* SetArray(SmartPointer data, int index, byte* source, int lenght) {
            return null;
        }

        public byte* MVCC(byte* data) {
            // -------- If references coutn > 1, create new bloc
            return null;
        }


        // ======== HELPERS : string, int, long, double, DateTime... all primitive and usual struct types !
        public string GetString(SmartPointer data, int index) {
            return null;
        }

        public byte* SetString(SmartPointer data, int index, string newValue) {
            return null;
        }
    }


    public unsafe class TestConstruction {

        public struct FixedPart_A {
            public int _a;
        }

        public class MyClasse_A {
            public static TypeDescriptor<FixedPart_A> descriptor_A = new TypeDescriptor<FixedPart_A>(sizeof (FixedPart_A),
                new ArrayDescriptor<char>(),
                new ArrayDescriptor<int>()
                );

            private SmartPointer _data = descriptor_A.Allocate();

            public int A {
                get { return ((FixedPart_A*) _data.Data)->_a; }
                set {
                    _data.StartModification();
                    ((FixedPart_A*) _data.Data)->_a = value;
                }
            }

            public string Name {
                get {
                    var p = descriptor_A.GetArray(_data, 0);
                    return new string((char*) p.Adress, 0, p.Count);
                }
                set {
                    fixed (char* s = value)
                        descriptor_A.SetArray(_data, 0, (byte*) s, value.Length/2);
                }
            }
        }


        public struct FixedPart_B {
            public int _b;
        }

        public class MyClasse_B : MyClasse_A {
            public static TypeDescriptor<FixedPart_B> descriptor_B = new TypeDescriptor<FixedPart_B>(sizeof (FixedPart_B),
                new ArrayDescriptor<char>(),
                new ArrayDescriptor<int>()
                );

            private SmartPointer _data = descriptor_B.Allocate();

            public int B {
                get { return ((FixedPart_B*) _data.Data)->_b; }
                set {
                    _data.StartModification();
                    ((FixedPart_B*) _data.Data)->_b = value;
                }
            }

            public string Name {
                get {
                    var p = descriptor_B.GetArray(_data, 0);
                    return new string((char*) p.Adress, 0, p.Count);
                }
                set {
                    fixed (char* s = value)
                        descriptor_A.SetArray(_data, 0, (byte*) s, value.Length/2);
                }
            }
        }

        public void Test() {

        }
    }
}