using System;
using System.Collections.Generic;
using System.Linq;
using ReGoap.Core;
using ReGoap.Utilities;
using UnityEngine;

namespace ReGoap.Unity
{
    /// <summary>
    /// AI代理类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapAgent<T, W> : MonoBehaviour, IReGoapAgent<T, W>, IReGoapAgentHelper
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 每次计算间隔
        /// </summary>
        public float CalculationDelay = 0.5f;

        /// <summary>
        /// 是否在Goal失败后加入黑名单（一段时间内不再考虑）
        /// </summary>
        public bool BlackListGoalOnFailure;

        /// <summary>
        /// 在开始是计算新Goal
        /// </summary>
        public bool CalculateNewGoalOnStart = true;

        /// <summary>
        /// 最后一次计算的时间
        /// </summary>
        protected float lastCalculationTime;

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
        /// 当前Action的状态
        /// </summary>
        protected ReGoapActionState<T, W> currentActionState;

        /// <summary>
        /// Goal黑名单
        /// </summary>
        protected Dictionary<IReGoapGoal<T, W>, float> goalBlacklist;

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
        /// 是否可以去推算另一个Goal
        /// </summary>
        protected bool interruptOnNextTransition;

        /// <summary>
        /// 是否开始规划
        /// </summary>
        protected bool startedPlanning;

        /// <summary>
        /// 当前规划者的工作
        /// </summary>
        protected ReGoapPlanWork<T, W> currentReGoapPlanWorker;

        /// <summary>
        /// 是否正在规划
        /// </summary>
        public bool IsPlanning
        {
            get { return startedPlanning && currentReGoapPlanWorker.NewGoal == null; }
        }

        #region UnityFunctions

        protected virtual void Awake()
        {
            lastCalculationTime = -100;
            goalBlacklist = new Dictionary<IReGoapGoal<T, W>, float>();

            RefreshGoalsSet();
            RefreshActionsSet();
            RefreshMemory();
        }

        protected virtual void Start()
        {
            if (CalculateNewGoalOnStart)
            {
                CalculateNewGoal(true);
            }
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
            if (currentActionState != null)
            {
                currentActionState.Action.Exit(null);
                currentActionState = null;
                currentGoal = null;
            }
        }

        #endregion

        /// <summary>
        /// 更新可能的Goal列表
        /// </summary>
        protected virtual void UpdatePossibleGoals()
        {
            possibleGoalsDirty = false;
            if (goalBlacklist.Count > 0)
            {
                possibleGoals = new List<IReGoapGoal<T, W>>(goals.Count);
                foreach (var goal in goals)
                    if (!goalBlacklist.ContainsKey(goal))
                    {
                        possibleGoals.Add(goal);
                    }
                    else if (goalBlacklist[goal] < Time.time)
                    {
                        goalBlacklist.Remove(goal);
                        possibleGoals.Add(goal);
                    }
            }
            else
            {
                possibleGoals = goals;
            }
        }

        /// <summary>
        /// 尝试去关心Action是否失败
        /// </summary>
        /// <param name="action"></param>
        protected virtual void TryWarnActionFailure(IReGoapAction<T, W> action)
        {
            if (action.IsInterruptable())
                WarnActionFailure(action);
            else
                action.AskForInterruption();
        }

        /// <summary>
        /// 计算新Goal
        /// </summary>
        /// <param name="forceStart"></param>
        /// <returns></returns>
        protected virtual bool CalculateNewGoal(bool forceStart = false)
        {
            if (IsPlanning)
                return false;
            if (!forceStart && (Time.time - lastCalculationTime <= CalculationDelay))
                return false;
            lastCalculationTime = Time.time;

            interruptOnNextTransition = false;
            UpdatePossibleGoals();
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            startedPlanning = true;
            currentReGoapPlanWorker = ReGoapPlannerManager<T, W>.Instance.Plan(this,
                BlackListGoalOnFailure ? currentGoal : null,
                currentGoal != null ? currentGoal.GetPlan() : null, OnDonePlanning);

            return true;
        }

        /// <summary>
        /// 在规划完成时调用
        /// </summary>
        /// <param name="newGoal">新的目标</param>
        protected virtual void OnDonePlanning(IReGoapGoal<T, W> newGoal)
        {
            startedPlanning = false;
            currentReGoapPlanWorker = default(ReGoapPlanWork<T, W>);
            if (newGoal == null)
            {
                if (currentGoal == null)
                {
                    ReGoapLogger.LogWarning("GoapAgent " + this + " could not find a plan.");
                }

                return;
            }

            if (currentActionState != null)
                currentActionState.Action.Exit(null);
            currentActionState = null;
            currentGoal = newGoal;
            if (startingPlan != null)
            {
                //遍历执行每个Action的PlanExit
                for (int i = 0; i < startingPlan.Count; i++)
                {
                    startingPlan[i].Action.PlanExit(i > 0 ? startingPlan[i - 1].Action : null,
                        i + 1 < startingPlan.Count ? startingPlan[i + 1].Action : null, startingPlan[i].Settings,
                        currentGoal.GetGoalState());
                }
            }

            startingPlan = currentGoal.GetPlan().ToList();
            ClearPlanValues();
            for (int i = 0; i < startingPlan.Count; i++)
            {
                //开始进行新Goal
                startingPlan[i].Action.PlanEnter(i > 0 ? startingPlan[i - 1].Action : null,
                    i + 1 < startingPlan.Count ? startingPlan[i + 1].Action : null, startingPlan[i].Settings,
                    currentGoal.GetGoalState());
            }

            currentGoal.Run(WarnGoalEnd);
            PushAction();
        }

        /// <summary>
        /// Plan转String
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        public static string PlanToString(IEnumerable<IReGoapAction<T, W>> plan)
        {
            var result = "GoapPlan(";
            var reGoapActions = plan as IReGoapAction<T, W>[] ?? plan.ToArray();
            for (var index = 0; index < reGoapActions.Length; index++)
            {
                var action = reGoapActions[index];
                result += string.Format("'{0}'{1}", action, index + 1 < reGoapActions.Length ? ", " : "");
            }

            result += ")";
            return result;
        }
        
        /// <summary>
        /// 入栈Action，准备执行他
        /// </summary>
        protected virtual void PushAction()
        {
            //如果是可以推算另一个Goal，就重新计算Goal
            if (interruptOnNextTransition)
            {
                CalculateNewGoal();
                return;
            }

            var plan = currentGoal.GetPlan();
            if (plan.Count == 0)
            {
                if (currentActionState != null)
                {
                    currentActionState.Action.Exit(currentActionState.Action);
                    currentActionState = null;
                }

                CalculateNewGoal();
            }
            else
            {
                var previous = currentActionState;
                currentActionState = plan.Dequeue();
                IReGoapAction<T, W> next = null;
                if (plan.Count > 0)
                    next = plan.Peek().Action;
                if (previous != null)
                    previous.Action.Exit(currentActionState.Action);
                currentActionState.Action.Run(previous != null ? previous.Action : null, next,
                    currentActionState.Settings, currentGoal.GetGoalState(), WarnActionEnd, WarnActionFailure);
            }
        }
        
        /// <summary>
        /// 关心Action是否结束
        /// </summary>
        /// <param name="thisAction"></param>
        public virtual void WarnActionEnd(IReGoapAction<T, W> thisAction)
        {
            if (thisAction != currentActionState.Action)
                return;
            PushAction();
        }
        
        /// <summary>
        /// 关心Action是否失败
        /// </summary>
        /// <param name="thisAction"></param>
        public virtual void WarnActionFailure(IReGoapAction<T, W> thisAction)
        {
            if (currentActionState != null && thisAction != currentActionState.Action)
            {
                ReGoapLogger.LogWarning(
                    string.Format("[GoapAgent] Action {0} warned for failure but is not current action.", thisAction));
                return;
            }

            if (BlackListGoalOnFailure)
                goalBlacklist[currentGoal] = Time.time + currentGoal.GetErrorDelay();
            CalculateNewGoal(true);
        }

        /// <summary>
        /// 关心Goal结束，如果可以的话就去计算新Goal
        /// </summary>
        /// <param name="goal"></param>
        public virtual void WarnGoalEnd(IReGoapGoal<T, W> goal)
        {
            //如果目标Goal不是当前Goal，就发出警告
            if (goal != currentGoal)
            {
                ReGoapLogger.LogWarning(string.Format("[GoapAgent] Goal {0} warned for end but is not current goal.",
                    goal));
                return;
            }

            CalculateNewGoal();
        }

        /// <summary>
        /// 关心可能的Goal，如果可以的话，就重新去计算可能的Goal
        /// </summary>
        /// <param name="goal"></param>
        public virtual void WarnPossibleGoal(IReGoapGoal<T, W> goal)
        {
            if ((currentGoal != null) && (goal.GetPriority() <= currentGoal.GetPriority()))
                return;
            if (currentActionState != null && !currentActionState.Action.IsInterruptable())
            {
                interruptOnNextTransition = true;
                currentActionState.Action.AskForInterruption();
            }
            else
                CalculateNewGoal();
        }

        /// <summary>
        /// 是否处于激活态
        /// </summary>
        /// <returns></returns>
        public virtual bool IsActive()
        {
            return enabled;
        }

        /// <summary>
        /// 获得起始规划
        /// </summary>
        /// <returns></returns>
        public virtual List<ReGoapActionState<T, W>> GetStartingPlan()
        {
            return startingPlan;
        }

        /// <summary>
        /// 清理所有规划值
        /// </summary>
        protected virtual void ClearPlanValues()
        {
            if (planValues == null)
                planValues = new Dictionary<T, W>();
            else
            {
                planValues.Clear();
            }
        }

        /// <summary>
        /// 获取规划
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual W GetPlanValue(T key)
        {
            return planValues[key];
        }

        /// <summary>
        /// 是否拥有某个规划
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool HasPlanValue(T key)
        {
            return planValues.ContainsKey(key);
        }

        /// <summary>
        /// 设置规划的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual void SetPlanValue(T key, W value)
        {
            planValues[key] = value;
        }

        /// <summary>
        /// 刷新记忆
        /// </summary>
        public virtual void RefreshMemory()
        {
            memory = GetComponent<IReGoapMemory<T, W>>();
        }

        /// <summary>
        /// 刷新（重新初始化）Goal集合
        /// </summary>
        public virtual void RefreshGoalsSet()
        {
            goals = new List<IReGoapGoal<T, W>>(GetComponents<IReGoapGoal<T, W>>());
            possibleGoalsDirty = true;
        }

        /// <summary>
        /// 刷新（重新初始化）Goal集合
        /// </summary>
        public virtual void RefreshActionsSet()
        {
            actions = new List<IReGoapAction<T, W>>(GetComponents<IReGoapAction<T, W>>());
        }

        /// <summary>
        /// 获取Goal集合
        /// </summary>
        /// <returns></returns>
        public virtual List<IReGoapGoal<T, W>> GetGoalsSet()
        {
            if (possibleGoalsDirty)
                UpdatePossibleGoals();
            return possibleGoals;
        }

        /// <summary>
        /// 获取Action集合
        /// </summary>
        /// <returns></returns>
        public virtual List<IReGoapAction<T, W>> GetActionsSet()
        {
            return actions;
        }

        /// <summary>
        /// 获取记忆
        /// </summary>
        /// <returns></returns>
        public virtual IReGoapMemory<T, W> GetMemory()
        {
            return memory;
        }

        /// <summary>
        /// 获取当前Goal
        /// </summary>
        /// <returns></returns>
        public virtual IReGoapGoal<T, W> GetCurrentGoal()
        {
            return currentGoal;
        }

        /// <summary>
        /// 实例化新State
        /// </summary>
        /// <returns></returns>
        public virtual ReGoapState<T, W> InstantiateNewState()
        {
            return ReGoapState<T, W>.Instantiate();
        }

        public override string ToString()
        {
            return string.Format("GoapAgent('{0}')", Name);
        }

        // this only works if the ReGoapAgent has been inherited. For "special cases" you have to override this
        public virtual Type[] GetGenericArguments()
        {
            return GetType().BaseType.GetGenericArguments();
        }
    }
}