using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharedObjectToolkitClassLibrary.BlockBasedAllocator;

namespace SharedObjectToolkitClassLibrary.VirtualObject {
    public unsafe class VirtualObject : SmartPointer {
        private TypeDescriptor _descriptor = null;

        protected TypeDescriptor InstanceTypeDescriptor { get { return _descriptor; } }

        protected virtual TypeDescriptor GetDescriptor() {
            return new TypeDescriptor(null, 0);
        }

        public VirtualObject() {
            _descriptor = GetDescriptor();
        }


        protected void NewVersion(bool clearFixed = false, bool clearAll = false) {
            if (_data == null) {
                Allocate(_descriptor.InitialSize, true);
                if (!clearAll && clearFixed)
                    MemoryHelper.Fill(_data, 0x00, _descriptor.FixedPartLenght);
                else if (clearAll) MemoryHelper.Fill(_data, 0x00, MemoryAllocator.SizeOf(_data));
                for (int i = 0; i < _descriptor.VariablePartCount; i++)
                    ArrayRecords[i] = new InBlockArrayRecord() {Lenght = 0, Offset = _descriptor.InitialSize};
            } else {
                var header = ((SegmentHeader*) (_data - SegmentHeader.SIZE));
                if (header->ReferenceCount > 1) {
                    var tmp = MemoryHelper.Clone(_data, false);
                    Force(tmp);
                }
            }
        }

        protected InBlockArrayRecord* ArrayRecords { get { return (InBlockArrayRecord*) (_data + _descriptor.FixedPartLenght); } }

        protected ArrayPointer GetArray(int index) {
            if (index >= _descriptor.VariablePartCount)
                throw new Exception("Index is out of variable parts records.");
            return ArrayRecords[index].ToArrayPointer(_data);
        }

        protected void SetArray(int index, byte* source, int lenght) {
            if (index >= _descriptor.VariablePartCount)
                throw new Exception("Index is out of variable parts records.");
            InBlockArrayRecord* arrayRecords = ArrayRecords;
            int delta = lenght - arrayRecords[index].Lenght;
            if (delta != 0)
                if (ChangeRangeLenght(arrayRecords[index].Offset, arrayRecords[index].Lenght, lenght))
                    arrayRecords = ArrayRecords;
            arrayRecords[index].Lenght = lenght;
            MemoryHelper.Copy(source, GetArray(index).Address, lenght);
            for (int i = index + 1; i < _descriptor.VariablePartCount; i++)
                arrayRecords[i].Offset += delta;
        }

        protected string GetArrayAsString(int index) {
            var p = GetArray(index);
            return new string((char*) p.Address, 0, p.Lenght/2);
        }

        protected void SetArrayAsString(int index, string value) {
            NewVersion();
            fixed (char* s = value)
                SetArray(index, (byte*) s, value.Length*2);
        }
    }
}