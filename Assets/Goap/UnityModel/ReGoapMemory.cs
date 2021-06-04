using ReGoap.Core;
using UnityEngine;

namespace ReGoap.Unity
{
    /// <summary>
    /// 记忆类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapMemory<T, W> : MonoBehaviour, IReGoapMemory<T, W>
    {
        /// <summary>
        /// 状态
        /// </summary>
        protected ReGoapState<T, W> state;

        #region UnityFunctions

        /// <summary>
        /// 初始化状态
        /// </summary>
        protected virtual void Awake()
        {
            state = ReGoapState<T, W>.Instantiate();
        }

        /// <summary>
        /// 当游戏物体销毁时回收状态
        /// </summary>
        protected virtual void OnDestroy()
        {
            state.Recycle();
        }

        protected virtual void Start()
        {
        }

        #endregion

        /// <summary>
        /// 获取世界状态
        /// </summary>
        /// <returns></returns>
        public virtual ReGoapState<T, W> GetWorldState()
        {
            return state;
        }
    }
}