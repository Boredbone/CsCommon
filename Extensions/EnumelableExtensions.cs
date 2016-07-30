using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boredbone.Utility.Extensions
{
    public static class EnumelableExtensions
    {
        /// <summary>
        /// 二つのDictionaryを結合
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>
            (this Dictionary<TKey, TValue> first, IEnumerable<KeyValuePair<TKey, TValue>> second)
        {
            if (first == null && second == null)
            {
                return null;
            }
            else if (first == null)
            {
                return second.ToDictionary(x => x.Key, x => x.Value);
            }
            else if (second == null)
            {
                return first;
            }

            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

            foreach (var x in first.Concat(second).Where(x => x.Value != null))
            {
                dictionary[x.Key] = x.Value;//重複していたら上書き
            }

            return dictionary;

            //return first.Concat(second).Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// 二つのDictionaryを結合
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static ConcurrentDictionary<TKey, TValue> Merge<TKey, TValue>
            (this ConcurrentDictionary<TKey, TValue> first, IEnumerable<KeyValuePair<TKey, TValue>> second)
        {
            if (first == null && second == null)
            {
                return null;
            }
            else if (first == null)
            {
                return new ConcurrentDictionary<TKey, TValue>(second);//.ToDictionary(x => x.Key, x => x.Value));
            }
            else if (second == null)
            {
                return first;
            }

            ConcurrentDictionary<TKey, TValue> dictionary = new ConcurrentDictionary<TKey, TValue>();

            foreach (var x in first.Concat(second).Where(x => x.Value != null))
            {
                dictionary[x.Key] = x.Value;//重複していたら上書き
            }

            return dictionary;

            //return new ConcurrentDictionary<TKey, TValue>(first.Concat(second).Where(x => x.Value != null));
            //.ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// 最初の要素を返却，シーケンスが空の場合はnull
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T? FirstOrNull<T>(this IEnumerable<T> source) where T : struct
        {
            foreach (var item in source)
            {
                return item;
            }
            return null;
        }

        /// <summary>
        /// 条件に一致する最初の要素を返却，シーケンスが空の場合はnull
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T? FirstOrNull<T>
            (this IEnumerable<T> source, Func<T, bool> predicate) where T : struct
        {
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return item;
                }
            }
            return null;
        }



        ///// <summary>
        ///// 2つのシーケンスを連結
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="first"></param>
        ///// <param name="second"></param>
        ///// <returns></returns>
        //public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, params T[] second)
        //{
        //    return Enumerable.Concat(first, second);
        //}


        /// <summary>
        /// シーケンス全体を指定回数繰り返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="souce"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> souce, int count)
        {
            return Enumerable.Range(0, count).SelectMany(_ => souce);
        }

        /// <summary>
        /// シーケンスの各要素ごとに指定回数ずつ繰り返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="souce"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<T> Stretch<T>(this IEnumerable<T> souce, int count)
        {
            return souce.SelectMany(value => Enumerable.Range(0, count).Select(_ => value));
        }


        /// <summary>
        /// 指定のシーケンスと同じ内容になるようAddとRemoveを行う
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static IList<T> Imitate<T>
            (this IList<T> source, IEnumerable<T> reference) where T : class
        {
            return source.Imitate(reference, (x, y) => object.ReferenceEquals(x, y), x => x);
        }

        /// <summary>
        /// 指定のシーケンスと同じ内容になるようAddとRemoveを行う
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="reference"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static IList<T> Imitate<T>
            (this IList<T> source, IEnumerable<T> reference, Func<T, T, bool> match)
        {
            return source.Imitate(reference, match, x => x);
        }

        /// <summary>
        /// 指定のシーケンスと同じ内容になるようAddとRemoveを行う
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="source"></param>
        /// <param name="reference"></param>
        /// <param name="match"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static IList<T1> Imitate<T1, T2>
            (this IList<T1> source, IEnumerable<T2> reference, Func<T1, T2, bool> match, Func<T2, T1> converter)
        {
            source.Absorb(reference, match, converter);
            return source.FilterStrictlyBy(reference, match);

            //source.FilterBy(reference, match);
            //source.AbsorbStrictly(reference, match, converter);
        }

        /// <summary>
        /// 二つのシーケンスを順番に比較し，referenceにしかない要素があれば追加
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="source"></param>
        /// <param name="reference"></param>
        /// <param name="match"></param>
        /// <param name="converter"></param>
        private static void AbsorbStrictly<T1, T2>
            (this IList<T1> source, IEnumerable<T2> reference, Func<T1, T2, bool> match, Func<T2, T1> converter)
        {

            int startIndex = 0;

            //新しいアイテムを追加

            foreach (var item in reference)
            {
                var length = source.Count;

                if (startIndex >= length)
                {
                    source.Add(converter(item));
                    startIndex = source.Count;
                    continue;
                }

                if (!match(source[startIndex], item))
                {
                    source.Insert(startIndex, converter(item));
                }
                startIndex++;
            }
        }

        /// <summary>
        /// 二つのシーケンスを順番に比較し，referenceにしかない要素があれば追加
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="source"></param>
        /// <param name="reference"></param>
        /// <param name="match"></param>
        /// <param name="converter"></param>
        public static void Absorb<T1, T2>
            (this IList<T1> source, IEnumerable<T2> reference, Func<T1, T2, bool> match, Func<T2, T1> converter)
        {

            int startIndex = 0;

            //新しいアイテムを追加

            foreach (var item in reference)
            {

                var length = source.Count;
                var existance = false;

                for (int i = startIndex; i < length; i++)
                {
                    var checkItem = source[i];

                    if (match(checkItem, item))
                    {
                        existance = true;
                        startIndex = i + 1;
                        break;
                    }
                }

                if (!existance)
                {
                    if (startIndex >= length)
                    {
                        source.Add(converter(item));
                        startIndex = source.Count;
                    }
                    else
                    {
                        source.Insert(startIndex, converter(item));
                        startIndex++;
                    }
                }
            }
        }

        /// <summary>
        /// 二つのシーケンスを順番に比較し，referenceに存在しない要素があれば削除
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="source"></param>
        /// <param name="reference"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        private static List<T1> FilterStrictlyBy<T1, T2>
            (this IList<T1> source, IEnumerable<T2> reference, Func<T1, T2, bool> match)
        {
            //消えたアイテムの削除

            var removedItems = new List<T1>();

            using (var e = reference.GetEnumerator())
            {
                var usable = e.MoveNext();
                var currentReference = e.Current;

                foreach (var item in source)
                {
                    if (!usable || !match(item, currentReference))
                    {
                        removedItems.Add(item);
                    }
                    else
                    {
                        usable = e.MoveNext();
                        currentReference = e.Current;
                    }
                }
            }


            foreach (var di in removedItems)
            {
                source.Remove(di);
            }
            return removedItems;
        }

        /// <summary>
        /// 二つのシーケンスを順番に比較し，referenceに存在しない要素があれば削除
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="source"></param>
        /// <param name="reference"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static List<T1> FilterBy<T1, T2>
            (this IList<T1> source, IEnumerable<T2> reference, Func<T1, T2, bool> match)
        {

            //消えたアイテムの削除

            int referenceIndex = 0;

            var removedItems = new List<T1>();

            foreach (var item in source)
            {
                var existingIndex = reference.FindIndex(referenceIndex, x => match(item, x));

                if (existingIndex < 0)
                {
                    removedItems.Add(item);
                }
                else
                {
                    referenceIndex = existingIndex + 1;
                }

            }


            foreach (var di in removedItems)
            {
                source.Remove(di);
            }
            return removedItems;
        }

        /// <summary>
        /// 条件に一致する最初の要素のインデックスを探す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            return source.FindIndex(0, match);
        }

        /// <summary>
        /// 指定されたインデックス以降で条件に一致する最初の要素のインデックスを探す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="match"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int FindIndex<T>(this IEnumerable<T> source, int startIndex, Predicate<T> match)
        {
            int index = 0;
            foreach (var x in source)
            {
                if (index >= startIndex && match(x))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }


        public static int FindLastIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            var index = 0;
            var result = -1;
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    result = index;
                }
                index++;
            }
            return result;
        }


        public static int BinarySearch<T>(this IList<T> source, Predicate<T> match)
        {
            return source.BinarySearch(match, 0, source.Count - 1, false);
        }
        public static int BinarySearchReversed<T>(this IList<T> source, Predicate<T> match)
        {
            return source.BinarySearch(match, 0, source.Count - 1, true);
        }

        public static int BinarySearch<T>(this IList<T> source, Predicate<T> match,
            int startIndex, int lastIndex, bool reversed)
        {
            if (!reversed && !match(source[lastIndex]))
            {
                return -1;
            }
            if (reversed && !match(source[startIndex]))
            {
                return -1;
            }

            var left = startIndex;
            var right = lastIndex;


            while (left < right)
            {
                var mid = (left + right) / 2;

                if (match(source[mid]) ^ (!reversed))
                {
                    left = mid;
                }
                else
                {
                    right = mid;
                }

                if (left == right)
                {
                    return left;
                }
                else if (left + 1 == right)
                {
                    if (!reversed)
                    {
                        return match(source[left]) ? left : right;
                    }
                    else
                    {
                        return match(source[right]) ? right : left;
                    }
                }
            }
            return -1;
        }


        /// <summary>
        /// シーケンスの数値を積分
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<double> Integral(this IEnumerable<double> source)
        {
            double sum = 0.0;

            foreach (var item in source)
            {
                sum += item;
                yield return sum;
            }
        }

        /// <summary>
        /// シーケンスの数値を積分
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<int> Integral(this IEnumerable<int> source)
        {
            int sum = 0;

            foreach (var item in source)
            {
                sum += item;
                yield return sum;
            }
        }

        /// <summary>
        /// シーケンスの数値を積分
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<long> Integral(this IEnumerable<long> source)
        {
            long sum = 0;

            foreach (var item in source)
            {
                sum += item;
                yield return sum;
            }
        }

        /// <summary>
        /// 二つのシーケンスの順番と長さが同じかどうか調べる
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static bool SequenceEqual<T1, T2>
            (this IEnumerable<T1> first, IEnumerable<T2> second, Func<T1, T2, bool> match)
        {
            if (first == null && second == null)
            {
                return true;
            }

            if (first == null || second == null)
            {
                return false;
            }

            using (var e1 = first.GetEnumerator())
            using (var e2 = second.GetEnumerator())
            {
                while (e1.MoveNext())
                {
                    if (!(e2.MoveNext() && match(e1.Current, e2.Current))) return false;
                }
                if (e2.MoveNext()) return false;
            }
            return true;
        }

        public static bool SequenceEqualParallel<T1, T2>
            (this IList<T1> first, IList<T2> second, Func<T1, T2, bool> match)
        {
            if (first == null && second == null)
            {
                return true;
            }

            if (first == null || second == null)
            {
                return false;
            }

            if (first.Count != second.Count)
            {
                return false;
            }

            return first.Zip(second, (a, b) => new Tuple<T1, T2>(a, b))
                .AsParallel()
                .All(x => match(x.Item1, x.Item2));

        }

        //private struct ValueTuple<T1, T2>
        //{
        //    public T1 Item1 { get; }
        //    public T2 Item2 { get; }
        //
        //    public ValueTuple(T1 item1,T2 item2)
        //    {
        //        this.Item1 = item1;
        //        this.Item2 = item2;
        //    }
        //
        //}

        /// <summary>
        /// インデックスが配列の範囲内かどうか調べる
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool ContainsIndex<T>(this IList<T> list, int index)
        {
            if (list == null)
            {
                return false;
            }
            return (index >= 0 && index < list.Count);
        }

        /// <summary>
        /// インデックスが配列の範囲内かどうか調べる
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool ContainsIndex<T>(this T[] array, int index)
        {
            if (array == null)
            {
                return false;
            }
            return (index >= 0 && index < array.Length);
        }

        /// <summary>
        /// 指定されたインデックスの要素を取得，配列の範囲外の場合はdefault
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T FromIndexOrDefault<T>(this IList<T> list, int index)
            => (list.ContainsIndex(index)) ? list[index] : default(T);

        /// <summary>
        /// 指定されたインデックスの要素を取得，配列の範囲外の場合はdefault
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T FromIndexOrDefault<T>(this T[] array, int index)
        {
            if (array.ContainsIndex(index))
            {
                return array[index];
            }
            return default(T);
        }



        /// <summary>
        /// 一つ前の値と合わせて処理
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="source"></param>
        /// <param name="func">前の値，現在の値，結果</param>
        /// <returns></returns>
        public static IEnumerable<Tout> TakeOver<Tin, Tout>
            (this IEnumerable<Tin> source, Func<Tin, Tin, Tout> func)
        {
            return source.TakeOver(func, source.First());
        }

        /// <summary>
        /// 一つ前の値と合わせて処理
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="source"></param>
        /// <param name="func">&lt;前の値，現在の値，結果&gt;</param>
        /// <param name="initialValue">初期値</param>
        /// <returns></returns>
        public static IEnumerable<Tout> TakeOver<Tin, Tout>
            (this IEnumerable<Tin> source, Func<Tin, Tin, Tout> func, Tin initialValue)
        {
            //yield return source.First() - initialValue;
            var prev = initialValue;
            foreach (var value in source)
            {
                yield return func(prev, value);
                prev = value;
            }
        }


        /// <summary>
        /// IEnumerable&lt;object&gt;に変換
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<object> AsEnumerable(this IEnumerable array)
        {
            foreach (var item in array)
            {
                yield return item;
            }
        }


        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return (list == null || list.Count <= 0);
        }

        public static bool IsNullOrEmpty<T>(this T[] list)
        {
            return (list == null || list.Length <= 0);
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> sequence, params T[] toAppend)
        {
            if (sequence == null) throw new ArgumentNullException("sequence");

            return Enumerable.Concat(sequence, toAppend);
            /*
            foreach (T item in sequence)
            {
                yield return item;
            }
            foreach (T item in toAppend)
            {
                yield return item;
            }*/
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> sequence, params T[] toPrepend)
        {
            if (sequence == null) throw new ArgumentNullException("sequence");

            return Enumerable.Concat(toPrepend, sequence);
            /*
            foreach (T item in toPrepend)
            {
                yield return item;
            }
            foreach (T item in sequence)
            {
                yield return item;
            }*/
        }


    }
}
