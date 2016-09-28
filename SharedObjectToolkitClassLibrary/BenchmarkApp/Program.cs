using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkApp.Benchmarks;
using BenchmarkApp.Benchmarks.BlockBasedAllocator;
using BenchmarkApp.Benchmarks.VirtualObjects;
using SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator;
using SharedObjectToolkitClassLibrary.Utilities;

namespace BenchmarkApp {
    public unsafe class Program {

        static void Main(string[] args) {

            MemoryAllocator.LogicalSizeToQueue(16900);


            bool shutdown = false;
            char[] seps = new char[] { ' ' };
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("******** Commands ********");
            Console.WriteLine(" exit : quit.");
            Console.WriteLine(" 1 : Bench Virtual Objects Parallel.");
            Console.WriteLine(" 2 : Bench Allocator New-Free.");
            Console.WriteLine(" 3 : Bench Allocator Linear New Change Size.");
            Console.WriteLine(" 4 : Bench Allocator New-Free -> Concurrent.");
            Console.WriteLine(" 5 : Bench Allocator Linear New Change Size -> Concurrent.");
            Console.WriteLine(" 6 : Bench Allocator Linear New.");
            Console.WriteLine(" 7 : Bench Virtual Objects Repository.");
            Console.WriteLine(" 8 : Bench Virtual Objects Dictionnary recording.");
            Console.WriteLine("**************************");
            Console.WriteLine("Enter command :");
            Console.ResetColor();
            while (!shutdown) {
                string userinput = Console.ReadLine();
                if (!string.IsNullOrEmpty(userinput)) {
                    try {
                        string[] parts = userinput.Split(seps);

                        if (!string.IsNullOrEmpty(parts[0])) {
                            string cmd = parts[0];
                            switch (cmd.ToLower()) {
                                case "exit":
                                    shutdown = true;
                                    break;
                                case "1":
                                    VirtualObjects.BenchVirtualObjectsParallel();
                                    break;
                                case "2":
                                    BenchMemoryAllocator.BenchAllocatorNewFree();
                                    break;
                                case "3":
                                    BenchMemoryAllocator.BenchAllocatorLinearNewChangeSize();
                                    break;
                                case "4":
                                    BenchMemoryAllocator.BenchAllocatorNewFreeConcurrent();
                                    break;
                                case "5":
                                    BenchMemoryAllocator.BenchAllocatorLinearNewChangeSizeConcurrent();
                                    break;
                                case "6":
                                    BenchMemoryAllocator.BenchAllocatorLinearNewFixed();
                                    break;
                                case "7":
                                    VirtualObjects.TestRepository();
                                    break;
                                case "8":
                                    VirtualObjects.TestRepositoryDictionnary();
                                    break;
                            }
                            Console.WriteLine("-------- End of command " + cmd.ToLower());
                        }
                    } catch (Exception ex) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("---------------- EXCEPTION ----------------");
                        Console.WriteLine(ex.Message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(ex.StackTrace);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("------------------- END -------------------");
                        Console.ResetColor();
                    }
                }
            }
        }

        
    }
}
