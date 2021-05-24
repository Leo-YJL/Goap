using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace ReGoap.Core {
    /// <summary>
    /// 状态接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapState<T, W> {
        /// <summary>
        /// 真正的值
        /// </summary>
        private ConcurrentDictionary<T, W> values;
        /// <summary>
        /// 缓冲区A
        /// </summary>
        private readonly ConcurrentDictionary<T, W> bufferA;
        /// <summary>
        /// 缓冲区B
        /// </summary>
        private readonly ConcurrentDictionary<T, W> bufferB;
        /// <summary>
        /// 默认大小
        /// </summary>
        public static int Defaultsize = 20;

        private ReGoapState() {
            bufferA = new ConcurrentDictionary<T, W>(5, Defaultsize);
            bufferB = new ConcurrentDictionary<T, W>(5, Defaultsize);
            values = bufferA;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="old">目标来源</param>
        private void Init(ReGoapState<T,W> old) {
            values.Clear();
            if (old != null) {
                lock (old.values) {
                    foreach (var pair in old.values) {
                        values[pair.Key] = pair.Value;
                    }
                }
            }
        }
        /// <summary>
        /// 重载+运算符，取两个状态集合的并集
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static ReGoapState<T,W> operator +(ReGoapState<T,W> a, ReGoapState<T,W> b) {
            ReGoapState<T, W> result;
            lock (a.values) {
                result = Instantiate(a);
            }
            lock (b.values) {
                foreach (var pair in b.values) {
                    result.values[pair.Key] = pair.Value;
                }
                return result;
            }
        } 
        /// <summary>
        /// 从一个状态集合中加入状态到自身
        /// </summary>
        /// <param name="b"></param>
        public void AddFromState(ReGoapState<T,W> b) {
            lock(values)
            lock (b.values) {
                foreach (var pair in b.values) {
                    values[pair.Key] = pair.Value;
                }
            }
        }
        /// <summary>
        /// 状态的数量
        /// </summary>
        public int Count {
            get { return values.Count; }
        }
        /// <summary>
        /// 当前状态是否与另一状态集有交集
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool HasAny(ReGoapState<T,W> other) {
            lock(values)
            lock (other.values) {
                foreach (var pair in other.values) {
                    W thisValue;
                    values.TryGetValue(pair.Key, out thisValue);
                    if (Equals(thisValue,pair.Value)) {
                        return true;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// 当前状态集是否和另一状态集有差别
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool HasAnyConflict(ReGoapState<T,W> other) {
            lock (values)
            lock (other.values) {
                foreach (var pair in other.values) {
                    W thisValue;
                    values.TryGetValue(pair.Key, out thisValue);
                    if (Equals(thisValue, pair.Value)) {
                        return true;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// 当前状态集是否和另一状态集有差别，还额外受到changes状态集影响
        /// </summary>
        /// <param name="changes"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool HasAnyConflict(ReGoapState<T, W> changes, ReGoapState<T, W> other) {
            lock (values)
                lock (other.values) {
                    foreach (var pair in other.values) {
                        var otherValue = pair.Value;

                        W thisValue;
                        if (!values.TryGetValue(pair.Key, out thisValue)) {
                            continue;
                        }
                        W effectValue;

                        changes.values.TryGetValue(pair.Key, out effectValue);
                        if (!Equals(otherValue, thisValue) && !Equals(effectValue, thisValue)) {
                            return true;
                        }
                    }
                    return false;
                }
        }

        /// <summary>
        /// 对比两个状态集有多少个不同的状态，并返回一个小于stopAt的值（不同状态的个数）
        /// </summary>
        /// <param name="other"></param>
        /// <param name="stopAt"></param>
        /// <returns></returns>
        public int MissingDifference(ReGoapState<T, W> other, int stopAt = int.MaxValue) {
            lock (values) {
                var count = 0;
                foreach (var pair in values) {
                    W otherValue;
                    other.values.TryGetValue(pair.Key, out otherValue);
                    if (!Equals(pair.Value,otherValue)) {
                        count++;
                        if (count >= stopAt) {
                            break;
                        }
                    }
                }
                return count;
            }
        }
        /// <summary>
        /// 对比两个状态集有多少不同的状态，并把不同的状态写入到difference集合中，并返回一个小于stopAt的值（不同状态的个数）
        /// </summary>
        /// <param name="other"></param>
        /// <param name="difference"></param>
        /// <param name="stopAt"></param>
        /// <param name="predicate"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        public int MissingDifference(ReGoapState<T, W> other, ref ReGoapState<T, W> difference, int stopAt = int.MaxValue,
            Func<KeyValuePair<T, W>, W, bool> predicate = null, bool test = false) {

            lock (values) {
                var count = 0;
                foreach (var pair in values) {
                    W otherValue;
                    other.values.TryGetValue(pair.Key, out otherValue);
                    if (!Equals(pair.Value, otherValue) && (predicate == null || predicate(pair,otherValue))) {
                        count++;
                        if (difference != null) {
                            difference.values[pair.Key] = pair.Value;
                        }
                        if (count >= stopAt) {
                            break;
                        }
                    }
                }
                return count;
            }

        }
        /// <summary>
        /// 只在状态集中保留不同的部分
        /// </summary>
        /// <param name="other"></param>
        /// <param name="stopAt"></param>
        /// <param name="predicate"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        public int ReplaceWithMissingDifference(ReGoapState<T,W> other,int stopAt = int.MaxValue,
             Func<KeyValuePair<T, W>, W, bool> predicate = null, bool test = false) {

            lock (values) {
                var count = 0;
                var buffer = values;
                values = values == bufferA ? bufferB : bufferA;
                values.Clear();
                foreach (var pair in buffer) {

                    W otherValue;
                    other.values.TryGetValue(pair.Key, out otherValue);

                    if (!Equals(pair.Value, otherValue) && (predicate == null || predicate(pair, otherValue))) {
                        count++;
                        values[pair.Key] = pair.Value;
                        if (count >= stopAt) {
                            break;
                        }
                    }
                }
                return count;
            }
        }


        /// <summary>
        /// 复制自己
        /// </summary>
        /// <returns></returns>
        public ReGoapState<T, W> Clone() {
            return Instantiate(this);
        }


        #region StateFactory
        /// <summary>
        /// 状态集缓存
        /// </summary>
        private static Stack<ReGoapState<T, W>> cachedStates;

        /// <summary>
        /// 预热
        /// </summary>
        /// <param name="count"></param>
        public static void Warmup(int count) {
            cachedStates = new Stack<ReGoapState<T, W>>(count);
            for (int i = 0; i < count; i++) {
                cachedStates.Push(new ReGoapState<T, W>());
            }
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Recycle() {
            lock (cachedStates) {
                cachedStates.Push(this);
            }
        }
       /// <summary>
       /// 实例化
       /// </summary>
       /// <param name="old"></param>
       /// <returns></returns>
        public static ReGoapState<T, W> Instantiate(ReGoapState<T, W> old = null) {
            ReGoapState<T, W> state;
            if (cachedStates == null) {
                cachedStates = new Stack<ReGoapState<T, W>>();
            }

            lock (cachedStates) {
                state = cachedStates.Count > 0 ? cachedStates.Pop() : new ReGoapState<T, W>();
            }
            state.Init(old);
            return null;
        }
        #endregion

        public override string ToString() {
            lock (values) {
                var result = "";
                foreach (var pair in values) {
                    result = $"{result}{string.Format("'{0}': {1}, ", pair.Key, pair.Value)}";
                }
                return result;
            }
        }

        /// <summary>
        /// 获取对应的状态
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public W Get(T key) {
            lock (values) {
                if (!values.ContainsKey(key)) {
                    return default(W);
                }
                return values[key];
            }
        }
        /// <summary>
        /// 设置状态的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(T key, W value) {
            lock (values) {
                values[key] = value;
            }
        }
        /// <summary>
        /// 移除状态
        /// </summary>
        /// <param name="key"></param>
        public void Remove(T key) {
            values.TryRemove(key, out _);
        }
        /// <summary>
        /// 获取所有状态
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<T,W> GetValues() {
            lock (values) {
                return values;
            }
        }
        /// <summary>
        /// 尝试获取元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(T key,out W value) {
            return values.TryGetValue(key, out value);
        }   
        /// <summary>
        /// 是否拥有某个key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(T key) {
            lock (values) {
                return values.ContainsKey(key);
            }
        }
        /// <summary>
        /// 清理状态集
        /// </summary>
        public void Clear() {
            lock (values) {
                values.Clear();
            }
        }
    }
}
