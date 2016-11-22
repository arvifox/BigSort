namespace bigsortns
{
    /// <summary>
    /// статичная фабрика создания классов
    /// </summary>
    class Factories
    {
        // главный управляющий класс
        public static IBigSortThread CreateBigSortManager(string[] _args)
        {
            return new Manager(_args);
        }

        // класс, управляющий очередями
        public static IBigSortManagerQueue CreateQueueManager(int _threadscount, int _linecount)
        {
            return new QueueManager(_threadscount, _linecount);
        }

        // пишущий поток
        public static IBigSortThread CreateBigSortWriter(IBigSortManagerQueue ComMan)
        {
            return new Writer(ComMan);
        }

        // рабочие потоки
        public static IBigSortThread CreateBigSortThread(IBigSortManagerQueue _queue)
        {
            return new BigSortThread(_queue);
        }

        // читающий поток
        public static IBigSortThread CreateBigSortReader(IBigSortManagerQueue _queue, string _filename)
        {
            return new BigSortReader(_filename, _queue);
        }

    }
}
