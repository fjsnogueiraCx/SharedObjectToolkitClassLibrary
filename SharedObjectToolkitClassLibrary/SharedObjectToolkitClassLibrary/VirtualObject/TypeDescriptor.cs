using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator;

namespace SharedObjectToolkitClassLibrary.VirtualObject {

    public struct sA {
        public long _a;
        public byte _b;
    }

    public struct sB {
        private sA _sA;
        public byte _b;
        public long _a;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    // ******************************************************************************** //
    public unsafe class ArrayDescriptor<T> : IArrayDescriptorType where T : struct {
        public Type GetArrayType() {
            return typeof (T);
        }
        public int EntrySize {
            get {
                return Marshal.SizeOf(typeof(T));
            }
        }
    }

    // ******************************************************************************** //
    public unsafe interface IArrayDescriptorType {
        Type GetArrayType();
    }

    // ******************************************************************************** //
    public unsafe struct ArrayPointer {
        public byte* Address;
        public int Lenght;
    }

    // ******************************************************************************** //
    public unsafe struct InBlockArrayRecord {
        public int Offset;
        public int Lenght;

        public ArrayPointer ToArrayPointer(byte* data) {
            return new ArrayPointer() { Address = data + Offset, Lenght = Lenght };
        }
    }

    // ******************************************************************************** //
    public struct ArrayTypeDescriptor {
        public int TypeSize;
    }

    // ******************************************************************************** //
    public unsafe class TypeDescriptor {
        private List<ArrayTypeDescriptor> _arrays = new List<ArrayTypeDescriptor>();
        private int _fixedPartLenght = 0;
        private TypeDescriptor _base = null;
        private int _classIndex = 0;
        private int _variablePartCount = 0;
        private FactoryTypeIdentifier _tid;

        public TypeDescriptor(TypeDescriptor baseDescriptor, int fixedTSize, FactoryTypeIdentifier tid, Type realType) {
            _base = baseDescriptor;
            if (_base != null)
                _arrays.AddRange(_base.Arrays);
            _fixedPartLenght = fixedTSize;
            _classIndex = ClassNumber - 1;
            _tid = tid;
            VirtualObjectFactory.RecordType(realType, _tid);
        }

        public int InitialSize {
            get {
                return (_fixedPartLenght + (_arrays.Count * sizeof(InBlockArrayRecord)));
            }
        }

        public int AddArray(IArrayDescriptorType array) {
            _arrays.Add(new ArrayTypeDescriptor() { TypeSize = Marshal.SizeOf(array.GetArrayType()) });
            _variablePartCount = _arrays.Count;
            return _arrays.Count - 1;
        }

        public int FixedPartLenght { get { return _fixedPartLenght; } }

        public List<ArrayTypeDescriptor> Arrays { get { return _arrays; } }

        public int ClassNumber {
            get {
                return 1 + (_base == null ? 0 : _base.ClassNumber);
            }
        }

        public int ClassIndex { get { return _classIndex; } }

        public int VariablePartCount { get { return _variablePartCount; } }

        public FactoryTypeIdentifier TypeIdentifier { get { return _tid; } }

        public SmartPointer Allocate() {
            return new SmartPointer(InitialSize);
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
}