using System.Threading;
using System.IO;
using System;
using System.Collections.Generic;

namespace bigsortns
{
    /// <summary>
    /// класс, пишущий в файл
    /// </summary>
    class Writer : IBigSortThread
    {
        private bool resultOK;
        private Thread writerthread;
        private StreamWriter file_out;
        // интерфейс для доступа к очереди данных
        private IBigSortManagerQueue bigsortwriter;
        private bool isDone = false;

        public Writer(IBigSortManagerQueue _writer)
        {
            writerthread = new Thread(this.run);
            bigsortwriter = _writer;
            writerthread.Name = "Writer";
        }

        /// <summary>
        /// реализация интерфейса
        /// </summary>
        /// <returns></returns>

        public bool ResultOK()
        {
            return resultOK;
        }

        public void StartThread()
        {
            writerthread.Start();
        }

        public void AbortThread()
        {
            writerthread.Abort();
        }

        public void JoinThread()
        {
            writerthread.Join();
        }

        void run()
        {
            SortedSet<bsdata> bytestowrite;
            try
            {
                // пока есть данные
                while ((bytestowrite = bigsortwriter.GetWrite()) != null)
                {
                    using (file_out = new StreamWriter("bsort" + bigsortwriter.GetNextFileNumber()))
                    {
                        foreach (bsdata el in bytestowrite)
                        {
                            file_out.WriteLine(el.ToString());
                        }
                        file_out.Flush();
                        file_out.Close();
                    }
                }
                resultOK = true;
                isDone = true;
                bigsortwriter.Done(true);
            }
            catch (IOException)
            {
                resultOK = false;
                isDone = true;
                bigsortwriter.Done(false);
            }
        }

        public bool IsDone()
        {
            return isDone;
        }
    }
}
