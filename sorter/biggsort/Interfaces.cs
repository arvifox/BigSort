using System.Collections.Generic;

namespace bigsortns
{
    /// <summary>
    /// интерфейс для работы с очередями
    /// </summary>
    public interface IBigSortManagerQueue
    {
        // читающий поток сообщит, что чтение завершено и новых данных больше не будет
        void ReadDone();
        // положить порцию в очередь прочитанных
        void PutRead(string _data);
        // положить порцию в очередь обработанных с указанием номера потока, который обработал для синхронизации
        void PutWrite(SortedSet<bsdata> _data);
        // взять порцию из прочитанных на обработку с указанием номера потока, который будет обрабатывать
        string GetRead();
        // взять порцию обработанных на запись
        SortedSet<bsdata> GetWrite();
        // потоки сообщают менеджеру очередей о своем завершении
        void Done(bool resultOK);
        // ждать потоки
        bool Wait();
        //
        int GetNextFileNumber();
        int GetFileCount();
        int GetLineCount();
    }

    // интерфейс для потока
    public interface IBigSortThread
    {
        void StartThread();
        void AbortThread();
        void JoinThread();
        bool ResultOK();
        bool IsDone();
    }
}
