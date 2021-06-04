using System;
using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Utilities;

namespace ReGoap.Planner
{
    /// <summary>
    /// Node类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapNode<T, W> : INode<ReGoapState<T, W>>
    {
        /// <summary>
        /// 总权重（越小越受青睐）
        /// </summary>
        private float cost;

        /// <summary>
        /// 规划者
        /// </summary>
        private IGoapPlanner<T, W> planner;

        /// <summary>
        /// 父结点
        /// </summary>
        private ReGoapNode<T, W> parent;

        /// <summary>
        /// 对应的Action
        /// </summary>
        private IReGoapAction<T, W> action;

        /// <summary>
        /// 对应的actionSettings
        /// </summary>
        private ReGoapState<T, W> actionSettings;

        /// <summary>
        /// 对应的State
        /// </summary>
        private ReGoapState<T, W> state;

        /// <summary>
        /// 路径点权重（越小越受青睐）
        /// </summary>
        private float g;

        /// <summary>
        /// 启发式权重（越小越受青睐）
        /// </summary>
        private float h;

        /// <summary>
        /// 与World和并在一起的Goal
        /// </summary>
        private ReGoapState<T, W> goalMergedWithWorld;

        /// <summary>
        /// 启发式系数
        /// </summary>
        private float heuristicMultiplier = 1;

        /// <summary>
        /// 拓扑列表
        /// </summary>
        private readonly List<INode<ReGoapState<T, W>>> expandList;

        /// <summary>
        /// 私有构造函数，防止在外部被实例化
        /// </summary>
        private ReGoapNode()
        {
            expandList = new List<INode<ReGoapState<T, W>>>();
        }

        /// <summary>
        /// 初始化结点
        /// </summary>
        /// <param name="planner">规划者</param>
        /// <param name="newGoal">新Goal</param>
        /// <param name="parent">父结点</param>
        /// <param name="action">动作</param>
        /// <param name="settings">设置</param>
        private void Init(IGoapPlanner<T, W> planner, ReGoapState<T, W> newGoal, ReGoapNode<T, W> parent,
            IReGoapAction<T, W> action, ReGoapState<T, W> settings)
        {
            expandList.Clear();

            this.planner = planner;
            this.parent = parent;
            this.action = action;
            if (settings != null)
                this.actionSettings = settings.Clone();

            if (parent != null)
            {
                state = parent.GetState().Clone();
                // g(node)
                g = parent.GetPathCost();
            }
            else
            {
                state = planner.GetCurrentAgent().GetMemory().GetWorldState().Clone();
            }

            var nextAction = parent == null ? null : parent.action;
            if (action != null)
            {
                // create a new instance of the goal based on the paren't goal
                //依据父结点的Goal新建一个Goal
                Goal = ReGoapState<T, W>.Instantiate(newGoal);

                GoapActionStackData<T, W> stackData;
                stackData.currentState = state;
                stackData.goalState = Goal;
                stackData.next = action;
                stackData.agent = planner.GetCurrentAgent();
                stackData.settings = actionSettings;

                Preconditions = action.GetPreconditions(stackData);
                Effects = action.GetEffects(stackData);
                // addding the action's cost to the node's total cost
                //把这个Action结点权重加到g上
                g += action.GetCost(stackData);

                // adding the action's effects to the current node's state
                //把action的Effect State加到当前node上
                state.AddFromState(Effects);

                // removes from goal all the conditions that are now fullfiled in the action's effects
                //只在Goal状态集中保留与Effects中不同的部分
                Goal.ReplaceWithMissingDifference(Effects);
                // add all preconditions of the current action to the goal
                //把preconditions的preconditions加到当前goal上
                Goal.AddFromState(Preconditions);
            }
            else
            {
                Goal = newGoal;
            }

            h = Goal.Count;
            // f(node) = g(node) + h(node)
            //计算总权重
            cost = g + h * heuristicMultiplier;

            // additionally calculate the goal without any world effect to understand if we are done
            //在没有World Effect情况下再计算一次Goal，来看看我们是否完成了目标
            var diff = ReGoapState<T, W>.Instantiate();
            Goal.MissingDifference(planner.GetCurrentAgent().GetMemory().GetWorldState(), ref diff);
            goalMergedWithWorld = diff;
        }

        #region NodeFactory

        private static Stack<ReGoapNode<T, W>> cachedNodes;

        /// <summary>
        /// 预热
        /// </summary>
        /// <param name="count"></param>
        public static void Warmup(int count)
        {
            cachedNodes = new Stack<ReGoapNode<T, W>>(count);
            for (int i = 0; i < count; i++)
            {
                cachedNodes.Push(new ReGoapNode<T, W>());
            }
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Recycle()
        {
            state.Recycle();
            state = null;
            Goal.Recycle();
            Goal = null;
            g = 0;
            lock (cachedNodes)
            {
                cachedNodes.Push(this);
            }
        }

        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="planner">规划者</param>
        /// <param name="newGoal">新的目标</param>
        /// <param name="parent">父结点</param>
        /// <param name="action">Action</param>
        /// <param name="actionSettings">Action Setting</param>
        /// <returns>封装好的结点</returns>
        public static ReGoapNode<T, W> Instantiate(IGoapPlanner<T, W> planner, ReGoapState<T, W> newGoal,
            ReGoapNode<T, W> parent, IReGoapAction<T, W> action, ReGoapState<T, W> actionSettings)
        {
            ReGoapNode<T, W> node;
            if (cachedNodes == null)
            {
                cachedNodes = new Stack<ReGoapNode<T, W>>();
            }

            lock (cachedNodes)
            {
                node = cachedNodes.Count > 0 ? cachedNodes.Pop() : new ReGoapNode<T, W>();
            }

            node.Init(planner, newGoal, parent, action, actionSettings);
            return node;
        }

        #endregion

        /// <summary>
        /// 路径点权重（越小越受青睐）
        /// </summary>
        /// <returns></returns>
        public float GetPathCost()
        {
            return g;
        }

        /// <summary>
        /// 启发式权重（越小越受青睐）
        /// </summary>
        /// <returns></returns>
        public float GetHeuristicCost()
        {
            return h;
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <returns></returns>
        public ReGoapState<T, W> GetState()
        {
            return state;
        }

        /// <summary>
        /// 进行拓扑
        /// </summary>
        /// <returns></returns>
        public List<INode<ReGoapState<T, W>>> Expand()
        {
            expandList.Clear();

            var agent = planner.GetCurrentAgent();
            var actions = agent.GetActionsSet();

            GoapActionStackData<T, W> stackData;
            stackData.currentState = state;
            stackData.goalState = Goal;
            stackData.next = action;
            stackData.agent = agent;
            stackData.settings = null;
            for (var index = actions.Count - 1; index >= 0; index--)
            {
                var possibleAction = actions[index];

                possibleAction.Precalculations(stackData);
                var settingsList = possibleAction.GetSettings(stackData);
                // if (settingsList != null)
                //     ReGoapLogger.Log($"{Name}的拓扑图为{settingsList[0].Count}");
                foreach (var settings in settingsList)
                {
                    stackData.settings = settings;
                    var precond = possibleAction.GetPreconditions(stackData);
                    var effects = possibleAction.GetEffects(stackData);

                    if (effects.HasAny(Goal) && // any effect is the current Goal
                        !Goal.HasAnyConflict(effects,
                            precond) && // no precondition is conflicting with the Goal or has conflict but the effects fulfils the Goal
                        !Goal.HasAnyConflict(effects) && // no effect is conflicting with the Goal
                        possibleAction.CheckProceduralCondition(stackData))
                    {
                        var newGoal = Goal;
                        //ReGoapLogger.Log($"{Name}-{GetCost()}的拓扑图新增{possibleAction.GetName()}");
                        expandList.Add(Instantiate(planner, newGoal, this, possibleAction, settings));
                    }
                }
            }
            //ReGoapLogger.Log($"{Name}-{GetCost()}的拓扑图处理完毕------------------------------");

            return expandList;
        }

        /// <summary>
        /// 获取Action
        /// </summary>
        /// <returns></returns>
        private IReGoapAction<T, W> GetAction()
        {
            return action;
        }

        /// <summary>
        /// 计算路径（从自身向上索引父结点）
        /// </summary>
        /// <returns></returns>
        public Queue<ReGoapActionState<T, W>> CalculatePath()
        {
            var result = new Queue<ReGoapActionState<T, W>>();
            CalculatePath(ref result);
            return result;
        }

        /// <summary>
        /// 计算路径（从自身向上索引父结点）
        /// </summary>
        /// <param name="result"></param>
        public void CalculatePath(ref Queue<ReGoapActionState<T, W>> result)
        {
            var node = this;
            while (node.GetParent() != null)
            {
                result.Enqueue(new ReGoapActionState<T, W>(node.action, node.actionSettings));
                node = (ReGoapNode<T, W>) node.GetParent();
            }
        }

        /// <summary>
        /// 与另一个结点对比权重
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(INode<ReGoapState<T, W>> other)
        {
            return cost.CompareTo(other.GetCost());
        }

        /// <summary>
        /// 获取权重（越小越受青睐）
        /// </summary>
        /// <returns></returns>
        public float GetCost()
        {
            return cost;
        }

        /// <summary>
        /// 获取父结点
        /// </summary>
        /// <returns></returns>
        public INode<ReGoapState<T, W>> GetParent()
        {
            return parent;
        }

        /// <summary>
        /// 是否为Goal
        /// </summary>
        /// <param name="goal"></param>
        /// <returns></returns>
        public bool IsGoal(ReGoapState<T, W> goal)
        {
            return goalMergedWithWorld.Count <= 0;
        }

        /// <summary>
        /// 优先级（越大越受青睐）
        /// </summary>
        public float Priority { get; set; }

        /// <summary>
        /// 插入点的索引
        /// </summary>
        public long InsertionIndex { get; set; }

        /// <summary>
        /// 优先级队列中索引
        /// </summary>
        public int QueueIndex { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return action != null ? action.GetName() : "NoAction"; }
        }

        /// <summary>
        /// Goal
        /// </summary>
        public ReGoapState<T, W> Goal { get; private set; }

        /// <summary>
        /// Effect
        /// </summary>
        public ReGoapState<T, W> Effects { get; private set; }

        /// <summary>
        /// Preconditions
        /// </summary>
        public ReGoapState<T, W> Preconditions { get; private set; }
    }
}