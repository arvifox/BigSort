using System;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace bigsortns
{
    /// <summary>
    /// класс, сжимающий порции в потоках
    /// </summary>
    class BigSortThread : IBigSortThread
    {
        private Thread cthr;
        private string indata;
        private SortedSet<bsdata> outdata;
        // интерфейс для доступа к очереди порций данных
        private IBigSortManagerQueue bsqueue;
        private bool isDone = false;
        private bool Ok = true;
        private int fn = 0;

        public BigSortThread(IBigSortManagerQueue _queue)
        {
            cthr = new Thread(this.run);
            bsqueue = _queue;
            cthr.Name = "compressorthread";
            outdata = new SortedSet<bsdata>();
        }

        void run()
        {
            try
            {
                // пока есть что сжимать
                while ((indata = bsqueue.GetRead()) != null)
                {
                    outdata.Add(new bsdata(indata, fn));
                    fn++;
                    if (fn >= bsqueue.GetLineCount())
                    {
                        // отдаем данные в другую очередь
                        fn = 0;
                        bsqueue.PutWrite(outdata);
                        outdata = new SortedSet<bsdata>();
                    }
                }
                if (outdata.Count > 0)
                {
                    bsqueue.PutWrite(outdata);
                }
                Ok = true;
                isDone = true;
                bsqueue.Done(true);
            }
            catch
            {
                Ok = false;
                isDone = true;
                bsqueue.Done(false);
            }
        }

        /// <summary>
        /// реализация интерфейса
        /// </summary>

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
            return Ok;
        }

        public bool IsDone()
        {
            return isDone;
        }
    }
}
