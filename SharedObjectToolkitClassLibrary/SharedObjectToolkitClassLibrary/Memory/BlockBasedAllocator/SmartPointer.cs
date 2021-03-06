﻿/*********************************************************************************
*   (c) 2010 - 2016 / Gabriel RABHI
*   SHARED OBJECT TOOLKIT CLASS LIBRARY
*********************************************************************************/
using System;
using System.Threading;

namespace SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator {
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

        ~SmartPointer() {
            Forget();
        }

        protected void Allocate(int size, bool overSized = false) {
            _data = MemoryAllocator.New(size, true, overSized);
        }

        protected void Force(byte* ptr) {
            var header = ((BlockHeader*)(ptr - BlockHeader.SIZE));
            header->CheckCoherency();
            Interlocked.Increment(ref header->ReferenceCount);
            Forget();
            _data = ptr;
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
            GC.SuppressFinalize(this);
            Forget();
        }
    }
}