using ReGoap.Core;
using UnityEngine;

namespace ReGoap.Unity {
    /// <summary>
    /// 记忆类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapMemory<T, W> : MonoBehaviour, IReGoapMemory<T, W> {
        /// <summary>
        /// 状态
        /// </summary>
        protected ReGoapState<T, W> state;

        #region UnityFunctions
        /// <summary>
        /// 初始化状态
        /// </summary>
        protected virtual void Awake() {
            state = ReGoapState<T, W>.Instantiate();
        }

        protected virtual void Start() {

        }

        protected virtual void OnDestroy() {
            state.Recycle();
        }

        #endregion


        public ReGoapState<T, W> GetWorldState() {
            return state;
        }
    }
}