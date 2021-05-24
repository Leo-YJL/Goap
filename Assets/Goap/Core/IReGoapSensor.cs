using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace ReGoap.Core {
    /// <summary>
    /// 感应接口，这对于GOAP来说并不是必要的，但是如果您的记忆中具有许多状态
    /// 并且您想在不同的AI代理中重复使用不同的感应器
    /// 在Unity游戏中，您可以通过感应器的Update/FixedUpdate来更新记忆
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public interface IReGoapSensor<T, W> {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="memory"></param>
        void Init(IReGoapMemory<T, W> memory);

        /// <summary>
        /// 获取记忆
        /// </summary>
        /// <returns></returns>
        IReGoapMemory<T, W> GetMemory();

        /// <summary>
        /// 更新感应器
        /// </summary>
        void UpdateSensor();
    }
}
          

