using System;
using System.Collections.Generic;
using System.Threading;
using ReGoap.Core;
using ReGoap.Utilities;
using UnityEngine;

namespace ReGoap.Planner {
    /// <summary>
    /// Planner类，负责AI代理的Goal的Plan规划
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapPlanner<T, W> : IGoapPlanner<T, W> {

        /// <summary>
        /// AI代理
        /// </summary>
        private IReGoapAgent<T, W> goapAgent;

        /// <summary>
        /// 当前目标
        /// </summary>
        private IReGoapGoal<T, W> currentGoal;
        /// <summary>
        /// 是否已计算
        /// </summary>
        private bool Calculated;
        /// <summary>
        /// AStar路径规划
        /// </summary>
        private readonly AStar<ReGoapState<T, W>> astar;
        /// <summary>
        /// Planner设置
        /// </summary>
        private readonly ReGoapPlannerSettings settings;
    
        public ReGoapPlanner(ReGoapPlannerSettings settings = null) {
            this.settings = settings ?? new ReGoapPlannerSettings();
            astar = new AStar<ReGoapState<T, W>>(this.settings.MaxNodesToExpand);

        }
        /// <summary>
        /// 开始自动根据AI代理的Goal集合制定的一个计划
        /// </summary>
        /// <param name="agent">AI代理</param>
        /// <param name="blackListGoal">Goal黑名单</param>
        /// <param name="currentPlan">当前规划队列</param>
        /// <param name="callback">规划完成回调</param>
        /// <returns></returns>
        public IReGoapGoal<T, W> Plan(IReGoapAgent<T, W> agent, IReGoapGoal<T, W> blackListGoal  = null,
            Queue<ReGoapActionState<T, W>> currentPlan = null, Action<IReGoapGoal<T, W>> callback = null) {
            if (ReGoapLogger.Level == ReGoapLogger.DebugLevel.Full) {
                ReGoapLogger.Log("[ReGoalPlanner] Starting planning calculation for agent: " + agent);
            }
            goapAgent = agent;
            Calculated = false;
            currentGoal = null;

            var possibleGoals = new List<IReGoapGoal<T, W>>();
            foreach (var goal in goapAgent.GetGoalsSet()) {
                if (goal == blackListGoal) {
                    continue;
                }
                goal.Precalculations(this);
                if (goal.IsGoalPossible()) {
                    possibleGoals.Add(goal);
                }
            }
            //按优先级大小排序
            possibleGoals.Sort((x, y) => x.GetPriority().CompareTo(y.GetPriority()));
            //获取当前世界状态
            var currentState = agent.GetMemory().GetWorldState();

            while (possibleGoals.Count > 0) {
                currentGoal = possibleGoals[possibleGoals.Count - 1];
                possibleGoals.RemoveAt(possibleGoals.Count - 1);
                var goalState = currentGoal.GetGoalState();
                //如果没有选择开启动态Action
                if (!settings.UsingDynamicActions) {
                    var wantedGoalCheck = currentGoal.GetGoalState();
                    GoapActionStackData<T, W> stackData;
                    stackData.currentState = currentState;
                    stackData.goalState = goalState;
                    stackData.next = null;
                    stackData.agent =goapAgent;
                    stackData.settings = null;
                    //先进行预先检查便利，如果不通过就直接排除，如果通过再进行A*遍历
                    //遍历Action集合进行检查
                    foreach (var action in goapAgent.GetActionsSet()) {
                        action.Precalculations(stackData);
                        if (!action.CheckProceduralCondition(stackData)) {
                            continue;
                        }
                        // 对比Goal和Action的Effect，把差异值写入wantedGoalCheck
                        var previous = wantedGoalCheck;
                        wantedGoalCheck = ReGoapState<T, W>.Instantiate();
                        previous.MissingDifference(action.GetEffect(stackData), ref wantedGoalCheck);
                    }
                    //最后进行一次世界状态匹配，如果通过即表示可完成目标
                    var current = wantedGoalCheck;
                    wantedGoalCheck = ReGoapState<T, W>.Instantiate();
                    current.MissingDifference(GetCurrentAgent().GetMemory().GetWorldState(), ref wantedGoalCheck);

                    if (wantedGoalCheck.Count > 0) {
                        currentGoal = null;
                        continue;
                    }
                }

                goalState = goalState.Clone();
                var leaf = (ReGoapNode<T, W>)astar.Run(ReGoapNode<T, W>.Instantiate(this, goalState, null, null, null), goalState, settings.MaxIterations,
                    settings.PlanningEarlyExit, debugPlan: settings.DebugPlan);

                if (leaf == null) {
                    currentGoal = null;
                    continue;
                }

                var reasult = leaf.CalculatePath();
                if (currentPlan != null && currentPlan == reasult) {
                    currentGoal = null;
                    break;
                }

                if (reasult.Count == 0) {
                    currentGoal = null;
                    continue;
                }
                //如果寻得可抵达目标的路径，就设置一下
                currentGoal.SetPlan(reasult);
                break;
            }


            Calculated = true;

            callback?.Invoke(currentGoal);

            if (currentGoal != null) {
                ReGoapLogger.Log(string.Format("[ReGoapPlanner] Calculated plan for goal '{0}', plan length: {1}", currentGoal, currentGoal.GetPlan().Count));
                if (ReGoapLogger.Level == ReGoapLogger.DebugLevel.Full) {
                    int i = 0;
                    GoapActionStackData<T, W> stackData = new GoapActionStackData<T, W>();
                    stackData.agent = agent;
                    stackData.currentState = currentState;
                    stackData.goalState = currentGoal.GetGoalState();
                    stackData.next = null;
                    foreach (var action in currentGoal.GetPlan()) {
                        stackData.settings = action.Settings;
                        //Debug.Log(Thread.CurrentThread.ManagedThreadId);
                        OneThreadSynchronizationContext.Instance.Post(ThreadLog,
                            new TempMessage<T, W>(i++, action, stackData));
                    }
                }
            } else {
                ReGoapLogger.LogWarning("[ReGoapPlanner] Error while calculating plan.");
            }

            return currentGoal;
        }


        #region 其他线程Log辅助器

        public struct TempMessage<T, W> {
            public int i;
            public ReGoapActionState<T, W> action;
            public GoapActionStackData<T, W> stackData;

            public TempMessage(int i, ReGoapActionState<T, W> action, GoapActionStackData<T, W> stackData) {
                this.i = i;
                this.action = action;
                this.stackData = stackData;
            }
        }

        private void ThreadLog(object o) {
            TempMessage<T, W> tempMessage = (TempMessage<T, W>)o;
            ReGoapLogger.Log(string.Format("[ReGoapPlanner] {0}) {1}", tempMessage.i,
                tempMessage.action.Action.ToString(tempMessage.stackData)));
        }

        #endregion

        /// <summary>
        /// 获取当前AI代理
        /// </summary>
        /// <returns></returns>
        public IReGoapAgent<T, W> GetCurrentAgent() {
            return goapAgent;
        }
        /// <summary>
        /// 获取当前Goal
        /// </summary>
        /// <returns></returns>
        public IReGoapGoal<T, W> GetCurrentGoal() {
            return currentGoal;
        }
        /// <summary>
        /// 获取设置
        /// </summary>
        public ReGoapPlannerSettings GetSettings() {
            return settings;
        }
        /// <summary>
        /// 是否正在进行规划
        /// </summary>
        public bool IsPlanning() {
            return !Calculated;
        }


    }
}