using System;
using System.IO;
using System.Threading;

namespace bigsortns
{
    /// <summary>
    /// класс, читающий исходный файл порциями и отдающий эти порции в очередь
    /// </summary>
    class BigSortReader : IBigSortThread
    {
        private Thread cthr;

        private string filename_in;

        private bool ok = false;
        private bool done = false;

        /// <summary>
        /// интерфейс для работы с очередью
        /// </summary>
        private IBigSortManagerQueue sortQueue;

        /// <summary>
        /// конструктор
        /// </summary>
        public BigSortReader(string _filename_in, IBigSortManagerQueue _Queue)
        {
            cthr = new Thread(this.DoRead);
            filename_in = _filename_in;
            sortQueue = _Queue;
        }

        public void StartThread()
        {
            cthr.Start();
        }

        public void AbortThread()
        {
            cthr.Abort();
        }

        public void JoinThread()
        {
            cthr.Join();
        }

        public bool ResultOK()
        {
            return ok;
        }

        public bool IsDone()
        {
            return done;
        }

        public void DoRead()
        {
            string bytestoread;
            /// чтение исходного файла кусками; каждый кусок в отдельном потоке сжимается
            try
            {
                using (StreamReader file_in = new StreamReader(filename_in))
                {
                    while ((bytestoread = file_in.ReadLine()) != null)
                    {
                        // отдаем порцию в очередь
                        sortQueue.PutRead(bytestoread);
                    }
                }
                done = true;
                ok = true;
                sortQueue.ReadDone();
                sortQueue.Done(true);
            }
            catch
            {
                done = true;
                ok = false;
                sortQueue.Done(false);
            }
        }
    }
}
