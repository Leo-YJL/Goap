namespace ReGoap.Core
{
    /// <summary>
    /// Action的状态
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapActionState<T, W>
    {
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
        /// <param name="action">Action</param>
        /// <param name="settings">设置（状态）</param>
        public ReGoapActionState(IReGoapAction<T, W> action, ReGoapState<T, W> settings)
        {
            Action = action;
            Settings = settings;
        }
    }
}