using System;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace bigsortns
{
    // класс, выполняющий синхронизацию между потоками чтения, записи и сортировки данных
    class Manager : IBigSortThread
    {
        /// <summary>
        /// работа производится в отдельном потоке
        /// </summary>
        private Thread thr;

        /// <summary>
        /// параметры командной строки
        /// </summary>
        private string[] args;

        /// <summary>
        /// результат работы класса
        /// </summary>
        public bool resultOk { get; set; }

        /// <summary>
        /// флаг, показывающий завершил ли класс свою работу
        /// </summary>
        public bool isDone { get; set; }

        // массив потоков
        private IBigSortThread[] threads;

        // колво потоков
        private int threadscount = 0;

        // поток пишущий в файл
        private IBigSortThread writer = null;

        // класс, читающий из файла
        private IBigSortThread reader;

        // объект, управляющий очередями
        private IBigSortManagerQueue QManager;

        /// <summary>
        /// конструктор
        /// </summary>
        public Manager(string[] _args)
        {
            args = _args;
            thr = new Thread(this.parse);
            //threadscount = Environment.ProcessorCount;
            int mgbt = Int32.Parse(args[2]);
            if (mgbt < 5)
            {
                mgbt = 5;
            }
            int avmem = Convert.ToInt32(mgbt * 0.65); // Mb
            int thr1 = Convert.ToInt32((avmem * 1024) / 2300); // Mb for 1 thread
            int linecount = Convert.ToInt32(avmem * 2500 / 2300); // тыс строк на один поток
            threadscount = 1;
            FileInfo fi = new FileInfo(args[0]);
            int szm = Convert.ToInt32(fi.Length / (1024 * 1024)); // Mb
            int kf = (szm / thr1);
            kf++;
            if (kf > Environment.ProcessorCount)
            {
                kf = Environment.ProcessorCount;
            }
            threadscount = threadscount * kf;
            linecount = linecount * 1000 / kf;
            threads = new IBigSortThread[threadscount];
            QManager = Factories.CreateQueueManager(threadscount, linecount);
        }

        /// <summary>
        /// метод, выполняющийся в потоке
        /// </summary>
        private void parse()
        {
            // если 
            Stopwatch swa = new Stopwatch();
            swa.Start();
            resultOk = BigSorting();
            swa.Stop();
            Console.WriteLine("Time split: {0}", swa.Elapsed);
            if (resultOk)
            {
                int filecount = QManager.GetFileCount();
                if (filecount == 1)
                {
                    File.Move("bsort0", args[1]);
                }
                else
                {
                    // слияние файлов
                    Console.WriteLine("BigSort: merging...");
                    SWriter sw = new SWriter(args[1], new SepReader(filecount));
                    sw.DoIt();
                    for (int i = 0; i < filecount; i++)
                    {
                        File.Delete("bsort" + i.ToString());
                    }
                }
            }
            isDone = true;
        }

        private bool BigSorting()
        {
            try
            {
                bool res;
                // запуск пишущего потока
                writer = Factories.CreateBigSortWriter(QManager);
                writer.StartThread();
                for (int i = 0; i < threadscount; i++)
                {
                    // запуск рабочих потокв
                    threads[i] = Factories.CreateBigSortThread(QManager);
                    threads[i].StartThread();
                }
                // запуск чтения
                reader = Factories.CreateBigSortReader(QManager, args[0]);
                reader.StartThread();
                // ждем потоки
                res = QManager.Wait();
                if (!res)
                {
                    throw new Exception();
                }
                writer.JoinThread();
                return res;
            }
            catch
            {
                /// прерывание всех потоков
                reader.AbortThread();
                writer.AbortThread();
                for (int j = 0; j < threadscount; j++)
                {
                    if (threads[j] != null)
                    {
                        threads[j].AbortThread();
                        threads[j].JoinThread();
                    }
                }
                reader.JoinThread();
                writer.JoinThread();
                return false;
            }
        }

        public void StartThread()
        {
            thr.Start();
        }

        public void AbortThread()
        {
            thr.Abort();
        }

        public void JoinThread()
        {
            thr.Join();
        }

        public bool ResultOK()
        {
            return resultOk;
        }

        public bool IsDone()
        {
            return isDone;
        }
    }
}
