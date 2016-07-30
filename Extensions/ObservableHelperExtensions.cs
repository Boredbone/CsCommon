using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using System.Reactive.Subjects;

namespace Boredbone.Utility.Extensions
{

    public static class ObservableHelperExtensions
    {


        /// <summary>
        /// 辞書にIDisposableを追加し、同じキーが存在している場合は元のアイテムをDispose
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="disposable"></param>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T AddTo<T, TKey>
            (this T disposable, IDictionary<TKey, IDisposable> dictionary, TKey key) where T : IDisposable
        {
            IDisposable result;
            if (dictionary.TryGetValue(key, out result))
            {
                result?.Dispose();
                dictionary.Remove(key);
            }

            dictionary.Add(key, disposable);

            return disposable;
        }

        /// <summary>
        /// 一定時間ごとに一つだけ通過
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IObservable<T> DownSample<T>(this IObservable<T> source, TimeSpan interval)
        {
            return Observable.Create<T>(o =>
            {
                var subscriptions = new CompositeDisposable();
                var acceepted = true;

                var pub = source.Where(x => acceepted);

                pub.Subscribe(o).AddTo(subscriptions);

                pub.Do(x => acceepted = false)
                .Delay(interval).Subscribe(x => acceepted = true)
                .AddTo(subscriptions);

                return subscriptions;
            });
        }

        /// <summary>
        /// Throttleした値と、Throttle後に初めてきた値を流す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IObservable<T> Restrict<T>(this IObservable<T> source, TimeSpan interval)
        {
            var seq = source
                .Throttle(interval)
                .Select(x => new FlaggedItem<T>(x, true))
                .Merge(source.Select(x => new FlaggedItem<T>(x, false)))
                .Publish()
                .RefCount();

            return seq
                .Zip(seq.Skip(1), (Z2, Z1) => new { Z2, Z1 })
                .Zip(seq.Skip(2), (a, b) => new { Value = b.Value, Z0 = b.Flag, Z1 = a.Z1.Flag, Z2 = a.Z2.Flag })
                //.Do(x => Debug.WriteLine($"{x.Value},{x.Z0},{x.Z1},{x.Z2}"))
                .Where(x => x.Z1 || (x.Z0 && !x.Z2))
                .Select(x => x.Value)
                .Merge(source.Take(2));
        }

        private class FlaggedItem<T>
        {
            public bool Flag { get; set; }
            public T Value { get; }

            public FlaggedItem(T value, bool flag)
            {
                this.Value = value;
                this.Flag = flag;
            }
        }

        public static ReactiveCommand<Tvalue> WithSubscribe<Tvalue>
            (this ReactiveCommand<Tvalue> observable, Action<Tvalue> action, ICollection<IDisposable> container)
        {
            observable.Subscribe(action).AddTo(container);
            observable.AddTo(container);
            return observable;
        }
        public static ReactiveCommand WithSubscribe
            (this ReactiveCommand observable, Action<object> action, ICollection<IDisposable> container)
        {
            observable.Subscribe(action).AddTo(container);
            observable.AddTo(container);
            return observable;
        }
        public static Subject<Tvalue> WithSubscribe<Tvalue>
            (this Subject<Tvalue> observable, Action<Tvalue> action, ICollection<IDisposable> container)
        {
            observable.Subscribe(action).AddTo(container);
            observable.AddTo(container);
            return observable;
        }

        public static bool Toggle(this ReactiveProperty<bool> target)
        {
            var newValue = !target.Value;
            target.Value = newValue;
            return newValue;
        }

        public static IObservable<T> SkipAfter<T, V>(this IObservable<T> source, IObservable<V> trigger, int count)
        {
            return Observable.Create<T>(o =>
            {
                var subscriptions = new CompositeDisposable();

                var acceptCount = 0;

                trigger.Subscribe(_ => acceptCount = count).AddTo(subscriptions);

                source.Where(_ => --acceptCount < 0).Subscribe(o).AddTo(subscriptions);

                return subscriptions;
            });
        }

        public static IObservable<IList<T>> BufferUntilThrottle<T>
            (this IObservable<T> source, double timeMilliseconds)
        {
            return source.BufferUntilThrottle(timeMilliseconds, true);
            //return source
            //    .Buffer(source.Throttle(TimeSpan.FromMilliseconds(timeMilliseconds)))
            //    .Where(x => x.Count > 0);
        }
        public static IObservable<IList<T>> BufferUntilThrottle<T>
            (this IObservable<T> source, double timeMilliseconds, bool publish)
        {
            var observable = source;

            if (publish)
            {
                observable = source.Publish().RefCount();

                //return source
                //    .Buffer(source.Throttle(TimeSpan.FromMilliseconds(timeMilliseconds)))
                //    .Where(x => x.Count > 0);
            }

            //var s2 = source.Publish().RefCount();

            return observable
                .Buffer(observable.Throttle(TimeSpan.FromMilliseconds(timeMilliseconds)))
                .Where(x => x.Count > 0);
        }



        /// <summary>Projects old and new element of a sequence into a new form.</summary>
        public static IObservable<OldNewPair<T>> Pairwise<T>(this IObservable<T> source, T initialValue) =>
            Pairwise(source, initialValue, (x, y) => new OldNewPair<T>(x, y));

        /// <summary>Projects old and new element of a sequence into a new form.</summary>
        public static IObservable<TR> Pairwise<T, TR>(this IObservable<T> source, T initialValue, Func<T, T, TR> selector)
        {
            var result = Observable.Create<TR>(observer =>
            {
                T prev = initialValue;

                return source.Subscribe(x =>
                {

                    TR value;
                    try
                    {
                        value = selector(prev, x);
                        prev = x;
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        return;
                    }

                    observer.OnNext(value);
                }, observer.OnError, observer.OnCompleted);
            });

            return result;
        }
    }
}
