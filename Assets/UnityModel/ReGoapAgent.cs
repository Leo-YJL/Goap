using ReGoap.Core;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Utilities;
using System;
using System.Linq;

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
        /// 是否可以去推算另一个Goal
        /// </summary>
        protected bool interruptOnNextTransition;
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
        #region UnityFunctions
        protected virtual void Awake() {
            lastCalCulationTime = -100;
            goalBlackList = new Dictionary<IReGoapGoal<T, W>, float>();

            RefreshGoalsSet();
            RefreshActionsSet();
            RefreshMemory();

        }
        protected virtual void Start() {

        }
        protected virtual void OnEnable() {
            if (CalculateNewGoalOnStart) {
                CalculateNewGoal(true);
            }
        }
        protected virtual void OnDisable() {
            if (currentActionState != null) {
                currentActionState.Action.Exit(null);
                currentActionState = null;
                currentGoal = null;
            }
        }
        #endregion

        /// <summary>
        /// 更新可能的Goal列表
        /// </summary>
        protected virtual void UpdatePossibleGoals() {
            possibleGoalsDirty = false;
            if (goalBlackList.Count > 0) {
                possibleGoals = new List<IReGoapGoal<T, W>>(goals.Count);
                foreach (var goal in goals) {
                    if (!goalBlackList.ContainsKey(goal)) {
                        possibleGoals.Add(goal);
                    } else if (goalBlackList[goal] < Time.time) {
                        goalBlackList.Remove(goal);
                        possibleGoals.Add(goal);
                    }
                }
            } else {
                possibleGoals = goals;
            }
        }
        /// <summary>
        /// 尝试区关系Action是否失败
        /// </summary>
        protected virtual void TryWarnActionFailure(IReGoapAction<T, W> action) {
            if (action.IsInterruptable()) {
                WarnActionFailure(action);
            } else {
                action.AskForInterruption();
            }
        }

        /// <summary>
        /// 计算新的Goal
        /// </summary>
        /// <param name="forceStart"></param>
        /// <returns></returns>
        protected virtual bool CalculateNewGoal(bool forceStart = false) {
            if (IsPlanning) {
                return false;
            }
            if (!forceStart && (Time.time - lastCalCulationTime <= CalculationDelay)) {
                return false;
            }
            lastCalCulationTime = Time.time;

            interruptOnNextTransition = false;
            UpdatePossibleGoals();

            startedPlanning = true;
            currentReGoapPlanWorker = ReGoapPlannerManager<T, W>.Instance.Plan(this,
                BlackListGoalOnFailure ? currentGoal : null,
                currentGoal != null ? currentGoal.GetPlan() : null, OnDonePlanning);

            return true;
        }
        /// <summary>
        /// 在规划完成时调用
        /// </summary>
        /// <param name="newGoal"></param>
        protected virtual void OnDonePlanning(IReGoapGoal<T,W> newGoal) {
            startedPlanning = false;
            currentReGoapPlanWorker = default(ReGoapPlanWork<T, W>);
            if (newGoal == null) {
                if (currentGoal == null) {
                    ReGoapLogger.LogWarning("GoapAgent" + this + " could not find a plan.");
                }
                return;
            }

            if (currentActionState != null) {
                currentActionState.Action.Exit(null);
            }
            currentActionState = null;
            currentGoal = newGoal;
            if (startingPlan != null) {
                //遍历执行每个Action的PlanExit
                for (int i = 0; i < startingPlan.Count; i++) {
                    startingPlan[i].Action.PlanExit(i > 0 ? startingPlan[i - 1].Action : null,
                        i + 1 < startingPlan.Count ? startingPlan[i + 1].Action : null,
                        startingPlan[i].Settings, currentGoal.GetGoalState());
                }
            }

            startingPlan = currentGoal.GetPlan().ToList();
            ClearPlanValues();
            for (int i = 0; i < startingPlan.Count; i++) {
                startingPlan[i].Action.PlanEnter(i > 0 ? startingPlan[i - 1].Action : null,
                       i + 1 < startingPlan.Count ? startingPlan[i + 1].Action : null,
                       startingPlan[i].Settings, currentGoal.GetGoalState());
            }

            currentGoal.Run(WarnGoalEnd);
            PushAction();
        }

        /// <summary>
        /// 入栈Action，准备执行
        /// </summary>
        protected virtual void PushAction() {
            //如果是可以推算另一个Goal，就重新计算Goal
            if (interruptOnNextTransition) {
                CalculateNewGoal();
                return;
            }
            var plan = currentGoal.GetPlan();
            if (plan.Count == 0) {
                if (currentActionState != null) {
                    currentActionState.Action.Exit(currentActionState.Action);
                    currentActionState = null;
                }
                CalculateNewGoal();
            } else {
                var previous = currentActionState;
                currentActionState = plan.Dequeue();
                IReGoapAction<T, W> next = null;
                if (plan.Count > 0) {
                    next = plan.Peek().Action;
                }
                if (previous != null) {
                    previous.Action.Exit(currentActionState.Action);
                }
                currentActionState.Action.Run(previous != null ? previous.Action : null, next,
                    currentActionState.Settings, currentGoal.GetGoalState(), WarnActionEnd, WarnActionFailure);


            }

        }
        /// <summary>
        /// 关心Action是否结束
        /// </summary>
        public virtual void WarnActionEnd(IReGoapAction<T,W> thisAction) {
            if (thisAction != currentActionState.Action) {
                return;
            }
            PushAction();
        }
        /// <summary>
        /// 关心Action是否失败
        /// </summary>
        public virtual void WarnActionFailure(IReGoapAction<T,W> thisAction) {
            if (currentActionState != null && thisAction != currentActionState.Action) {
                ReGoapLogger.LogWarning(string.Format("[GoapAgent] Action {0} warned for failure but is not current action.", thisAction));
                return;
            }
            if (BlackListGoalOnFailure) {
                goalBlackList[currentGoal] = Time.time + currentGoal.GetErrorDelay();
            }
            CalculateNewGoal(true);
        }
        /// <summary>
        /// 关心Goal结束，如果可以的话就去计算新Goal
        /// </summary>
        public virtual void WarnGoalEnd(IReGoapGoal<T,W> goal) {
            //如果目标Goal不是当前Goal，就发出警告
            if (goal != currentGoal) {
                ReGoapLogger.LogWarning(string.Format("[GoapAgent] Goal {0} warned for end but is not current goal.", goal));
                return;
            }

            CalculateNewGoal();
        }

        /// <summary>
        /// Plan转String
        /// </summary>
        public static string PlanToString(IEnumerable<IReGoapAction<T,W>> plan) {
            var result = "GoapPlan(";
            var reGoapActions = plan as IReGoapAction<T, W>[] ?? plan.ToArray();
            for (int i = 0; i < reGoapActions.Length; i++) {
                var action = reGoapActions[i];
                result += string.Format("'{0}'{1}", action, i + 1 < reGoapActions.Length ? ", " : "");
            }

            result += ")";
            return result;
        }
        /// <summary>
        /// 关心可能的Goal，如果可以的话，就重新去计算可能的Goal
        /// </summary>

        public virtual void WarnPossibleGoal(IReGoapGoal<T,W> goal) {
            if ((currentGoal != null) && (goal.GetPriority() <= currentGoal.GetPriority())) {
                return;
            }
            if (currentActionState != null && ! currentActionState.Action.IsInterruptable()) {
                interruptOnNextTransition = true;
                currentActionState.Action.AskForInterruption();
            } else {
                CalculateNewGoal();
            }
        }

        /// <summary>
        /// 是否处于激活
        /// </summary>
        /// <returns></returns>
        public virtual bool IsActive() {
            return enabled;
        }

        /// <summary>
        /// 获取起始规划
        /// </summary>
        /// <returns></returns>
        public virtual List<ReGoapActionState<T,W>> GetStartingPlan() {
            return startingPlan;
        }

        /// <summary>
        /// 清理所有的规划值
        /// </summary>
        protected virtual void ClearPlanValues() {
            if (planValues == null) {
                planValues = new Dictionary<T, W>();
            } else {
                planValues.Clear();
            }
        }

        /// <summary>
        /// 设置规划的值
        /// </summary>
        public virtual void SetPlanValue(T key, W value) {
            planValues[key] = value;
        }
        /// <summary>
        /// 获取规划的值
        /// </summary>
        public virtual W GetPlanValue(T key) {
           return planValues[key];
        }
        /// <summary>
        /// 是否拥有某个规划
        /// </summary>
        public virtual bool HasPlanValue(T key) {
            return planValues.ContainsKey(key);
        }

        /// <summary>
        /// 刷新记忆
        /// </summary>
        public virtual void RefreshMemory() {
            memory = GetComponent<IReGoapMemory<T, W>>();
        }

        /// <summary>
        /// 刷新（初始化）goal集合
        /// </summary>
        public virtual void RefreshGoalsSet() {
            goals = new List<IReGoapGoal<T, W>>(GetComponents<IReGoapGoal<T, W>>());
            possibleGoalsDirty = true;
        }

        /// <summary>
        /// 刷新（初始化）Goal集合
        /// </summary>
        public virtual void RefreshActionsSet() {
            actions = new List<IReGoapAction<T, W>>(GetComponents<IReGoapAction<T, W>>());
        }

        /// <summary>
        /// 获取Goal集合
        /// </summary>
        /// <returns></returns>
        public virtual List<IReGoapGoal<T,W>> GetGoalsSet() {
            if (possibleGoalsDirty) {
                UpdatePossibleGoals();
            }
            return possibleGoals;
        }

        /// <summary>
        /// 获取Action集合
        /// </summary>
        /// <returns></returns>
        public virtual List<IReGoapAction<T, W>> GetActionsSet() {
            return actions;
        }

        /// <summary>
        /// 获取记忆
        /// </summary>
        /// <returns></returns>
        public virtual IReGoapMemory<T,W> GetMemory() {
            return memory;
        }

        /// <summary>
        /// 获取当前的Goal
        /// </summary>
        /// <returns></returns>
        public virtual IReGoapGoal<T,W> GetCurrentGoal() {
            return currentGoal;
        }
        /// <summary>
        /// 实例化新state
        /// </summary>
        /// <returns></returns>
        public virtual ReGoapState<T,W> InstantiateNewState() {
            return ReGoapState<T, W>.Instantiate();
        }

        public override string ToString() {
            return $"GoapAgent('{Name}')";
        }

        /// <summary>
        /// this only works if the ReGoapAgent has been inherited. For "special cases" you have to override this
        /// </summary>
        /// <returns></returns>
        public Type[] GetGenericArguments() {
            return GetType().BaseType.GetGenericArguments();
        }


    }
}