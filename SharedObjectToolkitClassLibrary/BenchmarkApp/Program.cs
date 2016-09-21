﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedObjectToolkitClassLibrary.BlockBasedAllocator;
using SharedObjectToolkitClassLibrary.Utilities;

namespace BenchmarkApp {
    public class Program {
        static void Main(string[] args) {
            HighPrecisionTimer timer = new HighPrecisionTimer();
            MyClasse_B B = new MyClasse_B();
            B.Name = "Le monde est jolie !";
            int n = 0;
            Console.WriteLine("Start...");
            timer.Start();
            do {
                if (B.Name == "Ok")
                    n = n * 2 / 2;
                B.A = 65;
                n++;
                if (timer.Milliseconds > 1000) {
                    Console.WriteLine("Iterations : " + n);
                    break;
                }
            } while (true);
            Console.WriteLine("Stopped !");
            timer.Stop();


            n = 0;
            List<MyClasse_B> lst = new List<MyClasse_B>();
            timer.Reset(true);
            do {
                MyClasse_B B2 = new MyClasse_B();
                B2.Name = "Le monde est jolie ! Le monde il est beau ! Tout le monde s'aime !";
                B2.A = 65;
                lst.Add(B2);
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


            Console.ReadKey();
        }
    }
}