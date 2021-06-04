namespace ReGoap.Core
{
    /// <summary>
    /// 记忆接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public interface IReGoapMemory<T, W>
    {
        /// <summary>
        /// 获取世界状态
        /// </summary>
        /// <returns></returns>
        ReGoapState<T, W> GetWorldState();
    }
}