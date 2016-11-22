using System;
using System.Diagnostics;
using System.IO;

namespace bigsortns
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 3 && File.Exists(args[0]))
            {
                // use this property or Console.CancelKeyPress event to handle Ctrl+c
                Console.TreatControlCAsInput = true;

                // Stopwatch
                Stopwatch sw = new Stopwatch();
                sw.Start();

                //GZipStream
                IBigSortThread bigsort = Factories.CreateBigSortManager(args);
                Console.WriteLine("BigSort is working...");
                bigsort.StartThread();

                // waiting for finish or ctrl+c
                Console.WriteLine("Press Ctrl+c to exit");
                ConsoleKeyInfo cki = new ConsoleKeyInfo();
                while (!bigsort.IsDone())
                {
                    if (Console.KeyAvailable)
                    {
                        cki = Console.ReadKey(true);
                        if ((cki.Key == ConsoleKey.C) && (cki.Modifiers & ConsoleModifiers.Control) != 0)
                        {
                            bigsort.AbortThread();
                            bigsort.JoinThread();
                            break;
                        }
                    }
                }
                Console.WriteLine("BigSort has done.");
                sw.Stop();
                Console.WriteLine("Time elapsed: {0}", sw.Elapsed);
                if (!bigsort.ResultOK())
                {
                    Console.WriteLine("Error code = 1");
                    Environment.Exit(1);
                }
            }
            else
            {
                Console.WriteLine("Command line parameters error.");
                Console.WriteLine("The right way:");
                Console.WriteLine("BigSort.exe <source file> <destination file> <free RAM (Mb)>");
            }
        }
    }
}
