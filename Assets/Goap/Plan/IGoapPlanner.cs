using System.Collections;
using System.Collections.Generic;
using ReGoap.Core;
using System;
namespace ReGoap.Planner {
    /// <summary>
    /// Planner类接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public interface IGoapPlanner<T,W> {
        /// <summary>
        /// 开始自动根据AI代理的Goal集合而制定一个计划
        /// </summary>
        /// <param name="goapAgent">AI代理</param>
        /// <param name="blackListGoal">Goal黑名单</param>
        /// <param name="currentPlan">当前规划队列</param>
        /// <param name="callback">规划完成回调</param>
        /// <returns></returns>
        IReGoapGoal<T, W> Plan(IReGoapAgent<T, W> goapAgent, IReGoapGoal<T, W> blackListGoal,
            Queue<ReGoapActionState<T, W>> currentPlan, Action<IReGoapGoal<T, W>> callback);
        /// <summary>
        /// 获取当前目标
        /// </summary>
        /// <returns></returns>
        IReGoapGoal<T, W> GetCurrentGoal();
        /// <summary>
        /// 获取当前AI代理
        /// </summary>
        /// <returns></returns>
        IReGoapAgent<T, W> GetCurrentAgent();
        /// <summary>
        /// 是否正在规划
        /// </summary>
        /// <returns></returns>
        bool IsPlanning();
        /// <summary>
        /// 获取设置
        /// </summary>
        /// <returns></returns>
        ReGoapPlannerSettings GetSettings();
    }
}

