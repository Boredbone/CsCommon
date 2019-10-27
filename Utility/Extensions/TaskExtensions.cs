using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Boredbone.Utility.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// コレクション内のTaskが全て完了するまで待機
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task WhenAll(this IEnumerable<Task> tasks)
        {
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// コレクション内のTaskが全て完了するまで待機
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Taskの完了を待機せず、例外発生時に投げる
        /// </summary>
        /// <param name="task"></param>
        public static void FireAndForget(this Task task,
              [CallerMemberName] string memberName = "",
              [CallerFilePath] string filePath = "",
              [CallerLineNumber] int lineNumber = -1)
        {
            task.ContinueWith(x =>
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"{x.Exception?.InnerException?.Message}" +
                    $":{filePath}, {lineNumber}, {memberName}");
                System.IO.File.AppendAllText("log.txt", $"{DateTimeOffset.Now}\n{x.Exception?.InnerException}\n" +
                    $":{filePath}, {lineNumber}, {memberName}\n\n");
#endif
                throw x.Exception.InnerException;
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Taskの完了を待機せず、例外発生時に指定された処理を行ってから投げる
        /// </summary>
        /// <param name="task"></param>
        /// <param name="onFaulted"></param>
        public static void FireAndForget(this Task task, Action<AggregateException> onFaulted,
              [CallerMemberName] string memberName = "",
              [CallerFilePath] string filePath = "",
              [CallerLineNumber] int lineNumber = -1)
        {
            task.ContinueWith(x =>
            {
                onFaulted?.Invoke(x.Exception);
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"{x.Exception?.InnerException?.Message}" +
                    $":{filePath}, {lineNumber}, {memberName}");
                System.IO.File.AppendAllText("log.txt", $"{DateTimeOffset.Now}\n{x.Exception?.InnerException}\n" +
                    $":{filePath}, {lineNumber}, {memberName}\n\n");
#endif
                throw x.Exception.InnerException;
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// http://neue.cc/2014/03/14_448.html
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <param name="concurrency"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="configureAwait"></param>
        /// <returns></returns>
        public static async Task ForEachAsync<T>
            (this IEnumerable<T> source, Func<T, Task> action, int concurrency,
            CancellationToken cancellationToken = default(CancellationToken), bool configureAwait = false)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");
            if (concurrency <= 0) throw new ArgumentOutOfRangeException("concurrency must be positive1");

            using (var semaphore = new SemaphoreSlim(initialCount: concurrency, maxCount: concurrency))
            {
                var exceptionCount = 0;
                var tasks = new List<Task>();

                foreach (var item in source)
                {
                    if (exceptionCount > 0) break;
                    cancellationToken.ThrowIfCancellationRequested();

                    await semaphore.WaitAsync(cancellationToken).ConfigureAwait(configureAwait);
                    var task = action(item).ContinueWith(t =>
                    {
                        semaphore.Release();

                        if (t.IsFaulted)
                        {
                            Interlocked.Increment(ref exceptionCount);
                            throw t.Exception;
                        }
                    });
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(configureAwait);
            }
        }

        public static async Task<IEnumerable<TResult>> SelectAsync<T, TResult>
            (this IEnumerable<T> source, Func<T, Task<TResult>> func, int concurrency,
            CancellationToken cancellationToken = default(CancellationToken), bool configureAwait = false)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (func == null) throw new ArgumentNullException("func");
            if (concurrency <= 0) throw new ArgumentOutOfRangeException("concurrency must be positive1");

            using (var semaphore = new SemaphoreSlim(initialCount: concurrency, maxCount: concurrency))
            {
                var exceptionCount = 0;
                var tasks = new List<Task<TResult>>();

                foreach (var item in source)
                {
                    if (exceptionCount > 0) break;
                    cancellationToken.ThrowIfCancellationRequested();

                    await semaphore.WaitAsync(cancellationToken).ConfigureAwait(configureAwait);
                    var task = func(item).ContinueWith(t =>
                    {
                        semaphore.Release();

                        if (t.IsFaulted)
                        {
                            Interlocked.Increment(ref exceptionCount);
                            throw t.Exception;
                        }
                        return t.Result;
                    });
                    tasks.Add(task);
                }

                return await Task.WhenAll(tasks.ToArray()).ConfigureAwait(configureAwait);
            }
        }

        public static Task WithCancellation(this Task task, CancellationToken token)
        {
            return task.ContinueWith(t => t.GetAwaiter().GetResult(), token);
        }
        public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken token)
        {
            return task.ContinueWith(t => t.GetAwaiter().GetResult(), token);
        }
    }

}
