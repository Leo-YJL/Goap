using System.Collections;
using System.Collections.Generic;
namespace ReGoap.Core {

    /// <summary>
    /// AI代理接口
    /// </summary>
    public interface IReGoapAgent<T, W> {
        /// <summary>
        /// 获取记忆
        /// </summary>
        IReGoapMemory<T, W> GetMemory();
        /// <summary>
        /// 获取当前目标
        /// </summary>
        IReGoapGoal<T, W> GetCurrentGoal();

        /// <summary>
        /// 将会被一个Goal调用（当Goal为可达成的时候）
        /// </summary>
        /// <param name="goal"></param>
        void WarnPossibleGoal(IReGoapGoal<T, W> goal);

        /// <summary>
        /// 是否为激活状态
        /// </summary>
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
        /// <param name="target"></param>
        /// <returns></returns>
        bool HasPlanValue(T target);
        /// <summary>
        /// 获取Goal的集合，注意线程安全
        /// </summary>
        /// <returns></returns>
        List<IReGoapGoal<T, W>> GetGoalsSet();
        /// <summary>
        /// 获取Action的集合，注意线程安全
        /// </summary>
        /// <returns></returns>
        List<IReGoapAction<T, W>> GetActionsSet();
        /// <summary>
        /// 实例化一个新的state
        /// </summary>
        /// <returns></returns>
        ReGoapState<T, W> InstantiateNewState();
    }
}
