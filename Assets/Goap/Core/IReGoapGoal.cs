using ReGoap.Planner;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ReGoap.Core {
    /// <summary>
    /// 目标接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public interface IReGoapGoal<T,W>  {

        /// <summary>
        /// 运行Goal
        /// </summary>
        /// <param name="callBack">运行目标的回调函数</param>
        void Run(Action<IReGoapGoal<T, W>> callBack);

        /// <summary>
        /// 获取Plan,请务必保证这是线程安全的方法（不能使用任务Unity的库）
        /// </summary>
        /// <returns></returns>
        Queue<ReGoapActionState<T, W>> GetPlan();

        /// <summary>
        /// 获取Goal名字
        /// </summary>
        /// <returns></returns>
        string GetName();

        /// <summary>
        /// 进行预计算
        /// </summary>
        /// <param name="goapPlanner"></param>
        void Precalculations(IGoapPlanner<T, W> goapPlanner);
        /// <summary>
        /// 是否时一个可能的Goal
        /// </summary>
        /// <returns></returns>
        bool IsGoalPossible();

        /// <summary>
        /// 获取Goal的状态
        /// </summary>
        /// <returns></returns>
        ReGoapState<T, W> GetGoalState();
        /// <summary>
        /// 获取优先级，越大越受青睐
        /// </summary>
        /// <returns></returns>
        float GetPriority();
        /// <summary>
        /// 设置Plan
        /// </summary>
        /// <param name="path"></param>
        void SetPlan(Queue<ReGoapActionState<T, W>> path);
        /// <summary>
        /// 获取错误延迟
        /// </summary>
        /// <returns></returns>
        float GetErrorDelay();
    }
}
