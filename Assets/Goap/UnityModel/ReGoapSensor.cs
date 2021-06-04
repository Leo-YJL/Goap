using ReGoap.Core;
using UnityEngine;

namespace ReGoap.Unity
{
    /// <summary>
    /// 感知器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapSensor<T, W> : MonoBehaviour, IReGoapSensor<T, W>
    {
        /// <summary>
        /// 记忆
        /// </summary>
        protected IReGoapMemory<T, W> memory;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="memory"></param>
        public virtual void Init(IReGoapMemory<T, W> memory)
        {
            this.memory = memory;
        }

        /// <summary>
        /// 获取记忆
        /// </summary>
        /// <returns></returns>
        public virtual IReGoapMemory<T, W> GetMemory()
        {
            return memory;
        }

        /// <summary>
        /// 更新感知
        /// </summary>
        public virtual void UpdateSensor()
        {
        }
    }
}