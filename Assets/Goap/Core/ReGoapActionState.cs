using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace ReGoap.Core {
    /// <summary>
    /// Action状态接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapActionState<T, W> {
        /// <summary>
        /// Action
        /// </summary>
        public IReGoapAction<T, W> Action;
        /// <summary>
        /// 设置（状态）
        /// </summary>
        public ReGoapState<T, W> Settings;
        /// <summary>
        /// 构造函数
        /// </summary>
        public ReGoapActionState(IReGoapAction<T, W> action, ReGoapState<T, W> setting) {
            Action = action;
            Settings = setting;
        }
    }
}


