using System.Collections.Generic;

namespace ReGoap.Core
{
    /// <summary>
    /// AI代理接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public interface IReGoapAgent<T, W>
    {
        /// <summary>
        /// 获取记忆
        /// </summary>
        /// <returns></returns>
        IReGoapMemory<T, W> GetMemory();

        /// <summary>
        /// 获取当前目标
        /// </summary>
        /// <returns></returns>
        IReGoapGoal<T, W> GetCurrentGoal();

        // called from a goal when the goal is available
        /// <summary>
        /// 将会被一个Goal调用（当Goal为可达成的时候）
        /// </summary>
        /// <param name="goal"></param>
        void WarnPossibleGoal(IReGoapGoal<T, W> goal);

        /// <summary>
        /// 是否为激活态
        /// </summary>
        /// <returns></returns>
        bool IsActive();

        /// <summary>
        /// 获取正在启用的Plan
        /// </summary>
        /// <returns></returns>
        List<ReGoapActionState<T, W>> GetStartingPlan();

        /// <summary>
        /// 获取Plan的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        W GetPlanValue(T key);

        /// <summary>
        /// 设置Plan的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetPlanValue(T key, W value);

        /// <summary>
        /// 是否拥有目标Plan
        /// </summary>
        /// <param name="target">目标Plan</param>
        /// <returns></returns>
        bool HasPlanValue(T target);

        // THREAD SAFE
        /// <summary>
        /// 获取Goal集合，注意线程安全
        /// </summary>
        /// <returns></returns>
        List<IReGoapGoal<T, W>> GetGoalsSet();

        /// <summary>
        /// 获取Action集合，注意线程安全
        /// </summary>
        /// <returns></returns>
        List<IReGoapAction<T, W>> GetActionsSet();

        /// <summary>
        /// 实例化一个新的State
        /// </summary>
        /// <returns></returns>
        ReGoapState<T, W> InstantiateNewState();
    }
}