using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace AIHelper.Utils
{
    //materials
    //https://stackoverflow.com/questions/2106877/is-there-a-faster-way-than-this-to-find-all-the-files-in-a-directory-and-all-sub
    //https://www.codeproject.com/Articles/33084/Poor-Man-s-Parallel-ForEach-Iterator
    public static class ParallelPoorMan
    {
        internal static int NumberOfParallelTasks = Environment.ProcessorCount;

        public static void ForEach<T>(IEnumerable<T> enumerable, Action<T> action)
        {
            var syncRoot = new object();

            if (enumerable == null) return;

            var enumerator = enumerable.GetEnumerator();

            InvokeAsync<T> del = InvokeAction;

            var seedItemArray = new T[NumberOfParallelTasks];
            var resultList = new List<IAsyncResult>(NumberOfParallelTasks);

            for (int i = 0; i < NumberOfParallelTasks; i++)
            {
                bool moveNext;

                lock (syncRoot)
                {
                    moveNext = enumerator.MoveNext();
                    seedItemArray[i] = enumerator.Current;
                }

                if (moveNext)
                {
                    var iAsyncResult = del.BeginInvoke
             (enumerator, action, seedItemArray[i], syncRoot, i, null, null);
                    resultList.Add(iAsyncResult);
                }
            }

            foreach (var iAsyncResult in resultList)
            {
                del.EndInvoke(iAsyncResult);
                iAsyncResult.AsyncWaitHandle.Close();
            }
        }

        delegate void InvokeAsync<T>(IEnumerator<T> enumerator,
        Action<T> achtion, T item, object syncRoot, int i);

        static void InvokeAction<T>(IEnumerator<T> enumerator, Action<T> action,
                T item, object syncRoot, int i)
        {
            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name =
            string.Format(CultureInfo.InvariantCulture, "Parallel.ForEach Worker Thread No:{0}", i);

            bool moveNext = true;

            while (moveNext)
            {
                action.Invoke(item);

                lock (syncRoot)
                {
                    moveNext = enumerator.MoveNext();
                    item = enumerator.Current;
                }
            }
        }
    }
}