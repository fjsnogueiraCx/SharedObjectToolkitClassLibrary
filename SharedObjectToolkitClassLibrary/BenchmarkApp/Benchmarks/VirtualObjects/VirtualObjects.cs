using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator;
using SharedObjectToolkitClassLibrary.Utilities;
using SharedObjectToolkitClassLibrary.VirtualObject;
using SharedObjectToolkitClassLibrary.VirtualObject.Recorder;

namespace BenchmarkApp.Benchmarks.VirtualObjects {
    public static class VirtualObjects {
        public static void BenchVirtualObjectsParallel(bool multi = false) {
            if (multi) {
                Parallel.Invoke(
                    () => BenchVirtualObjects(), () => BenchVirtualObjects(), () => BenchVirtualObjects(), () => BenchVirtualObjects()
                    );
            } else {
                BenchVirtualObjects();
            }
            Thread.Sleep(1500);
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();

            Console.WriteLine("---------------------------------");
            Console.WriteLine("Block Count : " + MemoryAllocator.BlockCount);
            Console.WriteLine("Efficiency Ratio : " + MemoryAllocator.EfficiencyRatio);
            Console.WriteLine("Segment Count : " + MemoryAllocator.SegmentCount);
            Console.WriteLine("Total Allocated Memory : " + MemoryAllocator.TotalAllocatedMemory);
        }

        public static void BenchVirtualObjects() {
            HighPrecisionTimer timer = new HighPrecisionTimer();
            int n = 0;
            Console.WriteLine("Start...");
            ulong ids = 1;
            timer.Start();
            do {
                MyClasse_B B = new MyClasse_B();
                B.Id = ids++;
                B.Name = "Gabriel is in the floers and the camps !";
                B.Name = "Veronique had a small car !";
                if (B.Name == "Ok")
                    n = n*2/2;
                B.A = 65;
                n++;
                if (timer.Milliseconds > 1000) {
                    Console.WriteLine("Iterations : " + n);
                    break;
                }
            } while (true);
            Console.WriteLine("*********************************** !");
            timer.Stop();

            n = 0;
            timer.Reset(true);
            do {
                MyClasse_B B2 = new MyClasse_B();
                B2.Id = ids++;
                B2.Name = "Le monde est jolie ! Le monde il est beau ! Tout le monde s'aime !";
                B2.A = 65;
                //Repository<ulong>.Add(B2);
                n++;
                if (timer.Milliseconds > 1000) {
                    Console.WriteLine("Iterations : " + n);
                    break;
                }
            } while (true);
            timer.Stop();

            Console.WriteLine("Block Count : " + MemoryAllocator.BlockCount);
            Console.WriteLine("Efficiency Ratio : " + MemoryAllocator.EfficiencyRatio);
            Console.WriteLine("Segment Count : " + MemoryAllocator.SegmentCount);
            Console.WriteLine("Total Allocated Memory : " + MemoryAllocator.TotalAllocatedMemory);

        }

        public static unsafe void TestRebirth() {
            MyClasse_B X = new MyClasse_B();
            X.City = "Paris";

            var cpy = (MyClasse_B) VirtualObjectFactory.Rebirth(X.Data);

            cpy.B = 15;

            Console.WriteLine("X = Field City : " + cpy.City);
            Console.WriteLine("X = Field B : " + cpy.B);

            Console.WriteLine("CPY = Field City : " + cpy.City);
            Console.WriteLine("CPY = Field B : " + cpy.B);
        }

        public static unsafe void TestTreeNode() {
            ByteStringToPtrByteTreeNode _node = new ByteStringToPtrByteTreeNode();

            byte[] _k = new byte[] {0x05, 0x10, 0x11};

            fixed (byte* f = _k) {
                _node.Add(f, 2, (byte*) 1, &_node);
                var v = _node.Get((byte*) f, 2, &_node);
                Console.WriteLine((IntPtr) v);


                _k[0] = 0x05;
                _k[1] = 0x15;
                _k[1] = 0x18;

                _node.Add(f, 2, (byte*) 2, &_node);
                v = _node.Get((byte*) f, 2, &_node);
                Console.WriteLine((IntPtr) v);
            }
        }

        public static unsafe void TestRepository() {
            VirtualObjectRepository<VOId> rep = new VirtualObjectRepository<VOId>();
            HighPrecisionTimer timer = new HighPrecisionTimer();
            int n = 0;
            Console.WriteLine("Start...");
            ulong ids = 0;
            int cnt = 0;
            timer.Start();
            do {
                MyClasse_B B = new MyClasse_B();
                B.City = "Lyon : one of the beutufillest city of France !";
                B.Id = ids++;
                rep.Add(B);
                n++;
                if (timer.Milliseconds > 10000) {
                    Console.WriteLine("Iterations : " + n);
                    cnt = n;
                    break;
                }
            } while (true);

            n = 0;
            Random r = new Random();
            timer.Reset(true);
            do {
                ids = (ulong)r.Next(cnt-1);
                var obj = rep.Get(new VOId(ids));
                n++;
                if (timer.Milliseconds > 10000) {
                    Console.WriteLine("Iterations : " + n);
                    break;
                }
            } while (true);
            
            Console.WriteLine("*********************************** !");
            timer.Stop();
        }

        public static unsafe void TestRepositoryDictionnary() {
            Dictionary<VOId, IntPtr> rep = new Dictionary<VOId, IntPtr>();
            HighPrecisionTimer timer = new HighPrecisionTimer();
            int n = 0;
            Console.WriteLine("Start...");
            ulong ids = 1;
            MyClasse_B B = new MyClasse_B();
            B.Name = "Is it ok ?";
            timer.Start();
            do {
                ids++;
                B.Id = ids;
                rep.Add(B.Id, (IntPtr) B.Data);
                n++;
                if (timer.Milliseconds > 1000) {
                    Console.WriteLine("Iterations : " + n);
                    break;
                }
            } while (true);
            Console.WriteLine("*********************************** !");
            timer.Stop();
        }
    }
}
