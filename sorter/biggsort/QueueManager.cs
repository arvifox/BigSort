using System;
using System.Collections.Generic;
using System.Threading;

namespace bigsortns
{
    class QueueManager : IBigSortManagerQueue
    {
        // очередь для прочитанных порций данных
        private Queue<string> queueRead;

        // очередь и мьютекс для готовых к записи данных
        private Queue<SortedSet<bsdata>> queueWrite;

        // событие: queueRead опустела - нужно помещать новые элементы
        private ManualResetEvent queueReadNeed;

        // в queueRead есть элементы - можно брать в потоки на обработку
        private ManualResetEvent queueReadHas;

        // в queueWrite есть элементы - можно брать в поток на запись
        private ManualResetEvent queueWriteHas;

        // массив событий на запись в очередь для потоков
        private ManualResetEvent queueWriteNeed;

        // колво потоков
        private int ThreadsCount = 0;
        private int LineCount = 0;

        // колво элементов в очереди
        private int QueueCount = 0;

        // индексы текущего чтения/записи в очередях
        private int readindex = 0;
        private int writeindex = 0;
        // колво частей которыми читается файл
        private int partCountRead = 0;
        private int partCountWrite = 0;

        // закончено ли чтение исходного файла
        private bool isReadDone = false;

        // все потоки закончили работу или кто-то упал
        private ManualResetEvent allFinished;
        // результат работы потоков
        private bool ThreadsSuccess = false;
        // кол-во потоков, сообщивших о своем завершении
        private int ThreadsCountDone = 0;

        private int filenumber = -1;
        private Object lockobjectfilenumber;

        public QueueManager(int _threadscount, int _linecount)
        {
            ThreadsCount = _threadscount;
            LineCount = _linecount;
            QueueCount = ThreadsCount * 2;
            queueRead = new Queue<string>();
            queueWrite = new Queue<SortedSet<bsdata>>();
            queueReadNeed = new ManualResetEvent(true);
            queueReadHas = new ManualResetEvent(false);
            queueWriteNeed = new ManualResetEvent(true);
            queueWriteHas = new ManualResetEvent(false);
            allFinished = new ManualResetEvent(false);
            lockobjectfilenumber = new Object();
        }

        public int GetNextFileNumber()
        {
            lock (lockobjectfilenumber)
            {
                filenumber++;
                return filenumber;
            }
        }

        public int GetFileCount()
        {
            return filenumber + 1;
        }

        public int GetLineCount()
        {
            return LineCount;
        }

        public bool Wait()
        {
            allFinished.WaitOne();
            return ThreadsSuccess;
        }

        public void Done(bool resultOK)
        {
            lock (allFinished)
            {
                if (!resultOK)
                {
                    ThreadsSuccess = false;
                    allFinished.Set();
                }
                else
                {
                    ThreadsCountDone++;
                    /// +2 - read and write threads
                    if (ThreadsCountDone == ThreadsCount + 2)
                    {
                        ThreadsSuccess = true;
                        allFinished.Set();
                    }
                }
            }
        }

        // положить порцию в очередь готовых для сжатия/расжатия
        public void PutRead(string _data)
        {
            // ждем
            queueReadNeed.WaitOne();
            lock (queueRead)
            {
                queueRead.Enqueue(_data);
                partCountRead++;
                CheckQueueReadEvent();
            }
        }

        // положить порцию в очередь готовых для записи по порядку
        public void PutWrite(SortedSet<bsdata> _data)
        {
            // поток ждет разрешения именно для себя
            // тк порции нужно писать в файл в том же порядке, в котором они считывались
            queueWriteNeed.WaitOne();
            lock (queueWrite)
            {
                queueWrite.Enqueue(_data);
                partCountWrite++;
                CheckQueueWriteEvent();
            }
        }

        // взять порцию и ее номер из очереди прочитанных
        public string GetRead()
        {
            string data = null;
            // потоки ждут появления в очереди данных,
            // сделаем lock, чтобы все потоки не кинулись на одну порцию данных
            lock (queueReadHas)
            {
                queueReadHas.WaitOne();
                lock (queueRead)
                {
                    if (!isReadDone || readindex < partCountRead)
                    {
                        readindex++;
                        data = queueRead.Dequeue();
                        CheckQueueReadEvent();
                    }
                }
            }
            return data;
        }

        // взять порцию из очереди готовых для записи
        public SortedSet<bsdata> GetWrite()
        {
            SortedSet<bsdata> data = null;
            if (!isReadDone || writeindex < partCountWrite)
            {
                // ждет данные в очереди на запись
                queueWriteHas.WaitOne();
                lock (queueWrite)
                {
                    writeindex++;
                    data = queueWrite.Dequeue();
                    CheckQueueWriteEvent();
                }
            }
            return data;
        }

        /// <summary>
        /// установка события для очереди на запись
        /// </summary>
        private void CheckQueueWriteEvent()
        {
            if (queueWrite.Count >= QueueCount)
            {
                queueWriteNeed.Reset();
            }
            else
            {
                queueWriteNeed.Set();
            }
            if (queueWrite.Count > 0)
            {
                queueWriteHas.Set();
            }
            else
            {
                queueWriteHas.Reset();
            }
        }

        /// <summary>
        /// установка событий для очереди чтения
        /// </summary>
        private void CheckQueueReadEvent()
        {
            if (queueRead.Count >= QueueCount)
            {
                queueReadNeed.Reset();
            }
            else
            {
                queueReadNeed.Set();
            }
            if (queueRead.Count > 0)
            {
                queueReadHas.Set();
            }
            else
            {
                queueReadHas.Reset();
            }
        }

        public void ReadDone()
        {
            lock (queueRead)
            {
                isReadDone = true;
                queueReadHas.Set();
            }
        }
    }
}
