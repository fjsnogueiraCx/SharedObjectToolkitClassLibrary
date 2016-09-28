/*********************************************************************************
*   (c) 2010 - 2016 / Gabriel RABHI
*   SHARED OBJECT TOOLKIT CLASS LIBRARY
*********************************************************************************/
using System;
using System.Threading;
using SharedObjectToolkitClassLibrary.Memory.IndexPools;
using SharedObjectToolkitClassLibrary.VirtualObject;

namespace SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator {
    public static unsafe class MemoryAllocator {
        public static readonly int SEGMENT_COUNT = Math.Min(1024 * 32, 0x00FFFFFF); // min 64 Go
        public static object _locker = new object();
        public static object _freeLocker = new object();
        private static MemorySegment[] _segments = new MemorySegment[SEGMENT_COUNT];
        private static LinkedIndexPool _pool = new LinkedIndexPool(SEGMENT_COUNT, 1000);
        private static long _totalMemory = 0;
        private static long _totalBlockCount = 0;
        private static long _totalBufferSpace = 0;
        private static long _totalSegmentCount = 0;

        private static int MinSize { get { return 1024*2048; } }

        // Todo : trouver un algorythme pour déterminer la taille, taille de bloc et catégorie
        // Il devra permettre une forme de prédictibilité sur le taux d'espace perdu
        // Les steps seront de 1/4 de la taille du bloc : Size >> 2
        // si inférieur à 256 -> 256 (les 8 premiers bits sont inutiles)
        // on peut ensuite tester 0x0000FFFF < 0x00FFFFFF < 0xFFFFFFFF
        // cela ne fait plus que 8 bits à tester

        public static int LogicalSizeToQueue(uint size) {
            uint s = size;
            int bit = 0;
            for (int i = 0; i < 32; i++) {
                s = s >> 1;
                if (s == 0) {
                    bit = i-1;
                    break;
                }
            }
            var rangeStart = (1 << bit);
            var d = size - rangeStart;
            var steps = rangeStart / 4;
            return bit * 4 ;
        }

        private static int SizeToQueue(int size) {
            int q = 0;
            if (size < 1024) {
                q = 100 + ((size / 64) * 2);
            } else if (size >= 1024 && size < 8192) {
                q = 200 + (((size - 1024) / 512) * 2);
            } else if (size >= 8192 && size < 65536) {
                q = 300 + (((size - 8192) / 4096) * 2);
            } else if (size >= 65536 && size < 524288) {
                q = 400 + (((size - 65536) / 32768) * 2);
            } else if (size >= 524288 && size < 4194304) {
                q = 500 + (((size - 524288) / 262144) * 2);
            } else if (size >= 4194304 && size < 33554432) {
                q = 600 + (((size - 4194304) / 2097152) * 2);
            }
            return q;
        }

        private static void SizeToSegmentProperties(int size, out int segSize, out int blockSize) {
            int q = 0;
            segSize = 0;
            blockSize = 0;
            if (size < 1024) {
                blockSize = ((size / 64) + 1) * 64;
                segSize = 1024 * 2048; // 2Mo (16 = 32 mo)
            } else if (size >= 1024 && size < 8192) {
                blockSize = ((size / 512) + 1) * 512;
                segSize = 8192 * 1024; // 8Mo (16 = 128 mo)
            } else if (size >= 8192 && size < 65536) {
                blockSize = ((size / 4096) + 1) * 4096;
                segSize = 65536 * 256; // 16Mo (16 = 256 mo)
            } else if (size >= 65536 && size < 524288) {
                blockSize = ((size / 32768) + 1) * 32768;
                segSize = 65536 * 512; // 32Mo  (16 = 512 mo)
            } else if (size >= 524288 && size < 4194304) {
                blockSize = ((size / 262144) + 1) * 262144;
                segSize = 65536 * 512; // 32Mo  (16 = 512 mo)
            } else if (size >= 4194304 && size < 33554432) {
                blockSize = ((size / 2097152) + 1) * 2097152;
                segSize = 65536 * 1024; // 64Mo  (16 = 1 Go)
            }
        }

        // ****************************************************************************************** //
        // ********** STATISTICS
        public static byte* New(int size, bool counterBased = false, bool overSized = false) {
            byte* ptr = null;
            int q = SizeToQueue(size + (overSized ? size * 2 : 0));
            int mem = -1;
            // -------- Reduced contention with minimal instructions count
            lock (_locker) {
                int idx = _pool.FirstOfQueue(q);
                if (idx == -1) {
                    idx = _pool.Pop();
                    _pool.Enqueue(idx, q);
                    int segsSize = 0;
                    int blocksSize = 0;
                    SizeToSegmentProperties(size + (overSized ? size*2 : 0), out segsSize, out blocksSize);
                    _segments[idx].Build(segsSize, blocksSize, idx, out mem);
                    ptr = _segments[idx].Malloc(size, counterBased);
                } else {
                    ptr = _segments[idx].Malloc(size, counterBased);
                    if (_segments[idx].FreeBlocks == 0)
                        _pool.Enqueue(idx, q + 1);
                }
            }
            // -------- Update statistics
            Interlocked.Add(ref _totalMemory, size);
            Interlocked.Increment(ref _totalBlockCount);
            if (mem != -1) {
                Interlocked.Add(ref _totalBufferSpace, mem);
                Interlocked.Increment(ref _totalSegmentCount);
            }
            return ptr;
        }

        public static void Free(byte* ptr, bool counterBased = false) {
            var header = ((BlockHeader*) (ptr - BlockHeader.SIZE));
            header->CheckCoherency();
            if (counterBased)
                // -------- Prevent a simultaneous same test on the same pointer
                lock (_freeLocker)
                    if (header->ReferenceCount > 1) {
                        Interlocked.Decrement(ref header->ReferenceCount);
                        return;
                    }
            int mem = -1;
            int ptrSize = header->PtrSize;
            // -------- Reduced contention with minimal instructions count
            lock (_locker) {
                int idx = (int) header->SegmentIndex;
                _segments[idx].Free(ptr);
                if (_segments[idx].NoAllocatedBlocks) {
                    // -------- Todo : Faire en sorte qu'on garde au moins un segment de chaque taille
                    _segments[idx].Dispose(out mem);
                    _pool.Push(idx);
                } else {
                    var q = _pool.GetEntryQueue(idx);
                    if (q%2 != 0)
                        _pool.Enqueue(idx, q - 1);
                }
            }
            // -------- Update statistics
            Interlocked.Add(ref _totalMemory, -ptrSize);
            Interlocked.Decrement(ref _totalBlockCount);
            if (mem != -1) {
                Interlocked.Add(ref _totalBufferSpace, -mem);
                Interlocked.Decrement(ref _totalSegmentCount);
            }
        }

        public static byte* ChangeSize(byte* ptr, int newSize, bool counterBased = false) {
            var header = ((BlockHeader*) (ptr - BlockHeader.SIZE));
            header->CheckCoherency();
            if (header->BlocksSize >= newSize && newSize >= header->BlocksSize / 4) {// && newSize >= header->BlocksSize/2 && header->BlocksSize > 64) {
                header->PtrSize = newSize;
            } else {
                if (ptr == null)
                    throw new ArgumentException("Null parameter.", "data");
                if (newSize < 0)
                    throw new ArgumentException("Invalid parameter.", "newSize");
                byte* newBuffer = ptr;
                var dataLength = MemoryAllocator.SizeOf(ptr);
                if (dataLength != newSize) {
                    newBuffer = New(newSize, counterBased);
                    MemoryHelper.Copy(ptr, newBuffer, newSize < dataLength ? newSize : dataLength);
                    Free(ptr, counterBased);
                }
                return newBuffer;
            }
            return ptr;
        }

        public static int SizeOf(byte* ptr) {
            var header = ((BlockHeader*)(ptr - BlockHeader.SIZE));
            header->CheckCoherency();
            return header->PtrSize;
        }

        public static int Reserve(byte* ptr) {
            var header = ((BlockHeader*)(ptr - BlockHeader.SIZE));
            header->CheckCoherency();
            return header->BlocksSize - header->PtrSize;
        }

        public static FactoryTypeIdentifier TypeIdentifierOf(byte* ptr) {
            var header = ((BlockHeader*)(ptr - BlockHeader.SIZE));
            header->CheckCoherency();
            return header->TypeIdentifier;
        }

        public static void SetTypeIdentifierOf(byte* ptr, FactoryTypeIdentifier tid) {
            var header = ((BlockHeader*)(ptr - BlockHeader.SIZE));
            header->CheckCoherency();
            header->TypeIdentifier = tid;
        }

        public static int IncrementReference(byte* ptr) {
            var header = ((BlockHeader*) (ptr - BlockHeader.SIZE));
            header->CheckCoherency();
            return Interlocked.Increment(ref header->ReferenceCount);
        }

        // ****************************************************************************************** //
        // ********** STATISTICS
        public static long TotalAllocatedMemory { get { return _totalMemory; } }

        public static long BlockCount { get { return _totalBlockCount; } }

        public static long SegmentCount { get { return _totalSegmentCount; } }

        public static double EfficiencyRatio { get { return ((double)_totalMemory / (double)_totalBufferSpace); } }
    }
}