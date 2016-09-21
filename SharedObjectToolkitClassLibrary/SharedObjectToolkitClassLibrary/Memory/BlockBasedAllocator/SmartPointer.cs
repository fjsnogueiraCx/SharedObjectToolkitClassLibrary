using System;
using System.Runtime.CompilerServices;
using System.Threading;
using SharedObjectToolkitClassLibrary.BlockBasedAllocator;

namespace SharedObjectToolkitClassLibrary.VirtualObject {
    /// <summary>
    /// A memory reference with control over the real number of references,
    /// and hability to check for MVCC.
    /// A multi referenced block had a ReferenceCount > 1. If == 1 then the smartpointer is alone to reference this bloc.
    /// </summary>
    public unsafe class SmartPointer : IDisposable {
        protected byte* _data = null;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* Data { get { return _data; } }

        public SmartPointer() {
        }

        public SmartPointer(int size) {
            Allocate(size);
        }

        protected void Allocate(int size) {
            _data = MemoryAllocator.New(size, true);
        }

        protected void Force(byte* ptr) {
            Forget();
            _data = ptr;
            var header = ((SegmentHeader*) (_data - SegmentHeader.SIZE));
            header->CheckCoherency();
            Interlocked.Increment(ref header->ReferenceCount);
        }

        protected bool ChangeSize(int newSize) {
            var tmp = MemoryAllocator.ChangeSize(_data, newSize, true);
            bool changed = tmp != _data;
            _data = tmp;
            return changed;
        }

        protected bool ChangeRangeLenght(int offset, int oldLength, int newLength) {
            var tmp = MemoryHelper.ChangeRangeLenght(_data, offset, oldLength, newLength, true);
            bool changed = tmp != _data;
            _data = tmp;
            return changed;
        }

        protected void Forget() {
            if (_data != null) {
                MemoryAllocator.Free(_data, true);
                _data = null;
            }
        }

        public void Dispose() {
            Forget();
        }
    }
}