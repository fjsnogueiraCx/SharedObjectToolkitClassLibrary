using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedObjectToolkitClassLibrary.Memory;
using SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator;

namespace SharedObjectToolkitClassLibrary.VirtualObject.Recorder {

    public unsafe class ULongToPtrByteTree {
        ByteStringToPtrByteTreeNode _node = new ByteStringToPtrByteTreeNode();

        public void Add(ulong key, byte* ptr) {
            ulong reversed = 0;
            ulong* src = &key;
            ulong* dst = &reversed;
            ((byte*)dst)[0] = ((byte*)src)[7];
            ((byte*)dst)[1] = ((byte*)src)[6];
            ((byte*)dst)[2] = ((byte*)src)[5];
            ((byte*)dst)[3] = ((byte*)src)[4];
            ((byte*)dst)[4] = ((byte*)src)[3];
            ((byte*)dst)[5] = ((byte*)src)[2];
            ((byte*)dst)[6] = ((byte*)src)[1];
            ((byte*)dst)[7] = ((byte*)src)[0];
            fixed (ByteStringToPtrByteTreeNode* f = &_node)
            _node.Add((byte*)dst, 7, ptr, f);
        }

        public byte* Get(ulong key) {
            ulong reversed = 0;
            ulong* src = &key;
            ulong* dst = &reversed;
            ((byte*)dst)[0] = ((byte*)src)[7];
            ((byte*)dst)[1] = ((byte*)src)[6];
            ((byte*)dst)[2] = ((byte*)src)[5];
            ((byte*)dst)[3] = ((byte*)src)[4];
            ((byte*)dst)[4] = ((byte*)src)[3];
            ((byte*)dst)[5] = ((byte*)src)[2];
            ((byte*)dst)[6] = ((byte*)src)[1];
            ((byte*)dst)[7] = ((byte*)src)[0];
            fixed (ByteStringToPtrByteTreeNode* f = &_node)
                return _node.Get((byte*)dst, 7, f);
        }

        public void Remove(ulong key, byte* ptr) {
            ulong reversed = 0;
            ulong* src = &key;
            ulong* dst = &reversed;
            ((byte*)dst)[0] = ((byte*)src)[7];
            ((byte*)dst)[1] = ((byte*)src)[6];
            ((byte*)dst)[2] = ((byte*)src)[5];
            ((byte*)dst)[3] = ((byte*)src)[4];
            ((byte*)dst)[4] = ((byte*)src)[3];
            ((byte*)dst)[5] = ((byte*)src)[2];
            ((byte*)dst)[6] = ((byte*)src)[1];
            ((byte*)dst)[7] = ((byte*)src)[0];
            _node.Remove((byte*)dst, 7);
        }
    }

    public unsafe struct ByteStringToPtrByteTreeNode {
        private ByteStringToPtrByteTreeNode* _next;
        private byte* _value;
        private byte _b;
        private byte _count;

        public ByteStringToPtrByteTreeNode(ByteStringToPtrByteTreeNode* root = null) {
            _next = null;
            _value = null;
            _b = 0;
            _count = 0;
        }

        public bool AddBis(byte* words, int length, byte* value) {
            _b = *words;
            if (length == 0) {
                _value = value;
                return true;
            } else {
                if (_next == null) {
                    _next = (ByteStringToPtrByteTreeNode*)MemoryAllocator.New(sizeof(ByteStringToPtrByteTreeNode) * 256);
                    MemoryHelper.Fill((byte*)_next, 0x00, sizeof(ByteStringToPtrByteTreeNode) * 256);
                }
                if (_next[*words].AddBis(++words, --length, value))
                    _count++;
            }
            return false;
        }

        public bool Remove(byte* words, int length) {
            if (length == 0) {
                _value = null;
                return true;
            } else {
                if (_next[*words]._next != null) {
                    if (_next[*words].Remove(++words, --length))
                        _count--;
                }
            }
            return false;
        }

        public void Add(byte* words, int length, byte* value, ByteStringToPtrByteTreeNode* first) {
            ByteStringToPtrByteTreeNode* cur = first;
            while (length > 0) {
                if (cur->_next == null) {
                    cur->_next = (ByteStringToPtrByteTreeNode*)MemoryAllocator.New(sizeof(ByteStringToPtrByteTreeNode) * 256);
                    MemoryHelper.Fill((byte*)cur->_next, 0x00, sizeof(ByteStringToPtrByteTreeNode) * 256);
                }
                cur = &(cur->_next)[*words];
                words++;
                length--;
            }
            cur->_value = value;
        }

        public Byte* Get(byte* words, int length, ByteStringToPtrByteTreeNode* first) {
            ByteStringToPtrByteTreeNode* cur = first;
            while (length > 0) {
                cur = &(cur->_next)[*words];
                words++;
                length--;
            }
            return cur->_value;
        }

        /*
        public void AddBis(byte* words, int length, byte* value) {
            _b = *words;
            if (length == 0) {
                _value = value;
            } else {
                if (_next == null) {
                    _next = (ULongToPtrByteTree*)MemoryAllocator.New(sizeof(ULongToPtrByteTree));
                    _next[0].AddBis(++words, --length, value);
                } else {
                    // -------- Search
                    var count = MemoryAllocator.SizeOf((byte*)_next) / sizeof(ULongToPtrByteTree);
                    for (int i = 0; i < count; i++) {
                        if (_next[i]._b == *(words + 1)) {
                            _next[i].AddBis(++words, --length, value);
                            return;
                        }
                    }
                    // -------- Expand
                    _next = (ULongToPtrByteTree*)MemoryAllocator.ChangeSize((byte*)_next, (count + 1) * sizeof(ULongToPtrByteTree));
                    _next[count].AddBis(++words, --length, value);
                }

            }
        }*/
    }
}
