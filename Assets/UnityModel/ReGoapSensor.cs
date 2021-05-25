using ReGoap.Core;
using UnityEngine;

namespace ReGoap.Unity {
    /// <summary>
    /// 感知者
    /// </summary>
    public class ReGoapSensor<T, W> : MonoBehaviour, IReGoapSensor<T, W> {
        /// <summary>
        /// 记忆
        /// </summary>
        protected IReGoapMemory<T, W> memory;
        /// <summary>
        /// 获取记忆
        /// </summary>
        public IReGoapMemory<T, W> GetMemory() {
            return memory;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(IReGoapMemory<T, W> memory) {
            this.memory = memory;
        }

        public void UpdateSensor() {
            
        }
    }
}