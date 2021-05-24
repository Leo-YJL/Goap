using ReGoap.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ReGoap.Unity {
    /// <summary>
    /// AI代理类
    /// </summary>
    public class ReGoapAgent<T, W> : MonoBehaviour, IReGoapAgent<T, W>, IReGoapAgentHelper {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 每次计算间隔
        /// </summary>
        public float CalculationDelay = 0.5f;
        /// <summary>
        /// 是否再Goal失败后加入黑名单（一段时间内不再考虑）
        /// </summary>
        public bool BlackListGoalOnFailure;
        /// <summary>
        /// 在开始时计算新Goal
        /// </summary>
        public bool CalculateNewGoalOnStart = true;
        /// <summary>
        /// 最后一次计算的时间
        /// </summary>
        protected float lastCalCulationTime;
        /// <summary>
        /// Goal列表
        /// </summary>
        protected List<IReGoapGoal<T, W>> goals;
        /// <summary>
        /// Action列表
        /// </summary>
        protected List<IReGoapAction<T, W>> actions;
        /// <summary>
        /// 记忆
        /// </summary>
        protected IReGoapMemory<T, W> memory;
        /// <summary>
        /// 当前目标
        /// </summary>
        protected IReGoapGoal<T, W> currentGoal;
        /// <summary>
        /// 当前的Action状态
        /// </summary>
        protected ReGoapActionState<T, W> currentActionState;
        /// <summary>
        /// Goal黑名单
        /// </summary>
        protected Dictionary<IReGoapGoal<T, W>, float> goalBlackList;
        /// <summary>
        /// 有可能的Goal列表
        /// </summary>
        protected List<IReGoapGoal<T, W>> possibleGoals;
        /// <summary>
        /// 是否给有可能的Goal打上脏标记
        /// </summary>
        protected bool possibleGoalsDirty;
        /// <summary>
        /// 起始Plan
        /// </summary>
        protected List<ReGoapActionState<T, W>> startingPlan;
        /// <summary>
        /// Plan字典
        /// </summary>
        protected Dictionary<T, W> planValues;
        /// <summary>
        /// 是否开始规划
        /// </summary>
        protected bool startedPlanning;
        /// <summary>
        /// 当前规则者的工作
        /// </summary>
        protected ReGoapPlanWork<T, W> currentReGoapPlanWorker;
        /// <summary>
        /// 是否正在规划
        /// </summary>
        public bool IsPlanning {
            get {
                return startedPlanning && currentReGoapPlanWorker.NewGoal == null;
            }
        }

    }
}