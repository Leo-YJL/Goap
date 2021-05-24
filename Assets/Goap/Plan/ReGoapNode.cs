using System.Collections;
using System.Collections.Generic;
using ReGoap.Core;
namespace ReGoap.Planner {
    /// <summary>
    /// 节点类
    /// </summary>
    public class ReGoapNode<T, W> : INode<ReGoapState<T, W>> {

        /// <summary>
        /// 总权重(越小越受青睐)
        /// </summary>
        private float cost;
        /// <summary>
        /// 规划者
        /// </summary>
        private IGoapPlanner<T, W> planner;
        /// <summary>
        /// 父节点
        /// </summary>
        private ReGoapNode<T, W> parent;
        /// <summary>
        /// 对应的Action
        /// </summary>
        private IReGoapAction<T, W> action;
        /// <summary>
        /// 对应的ActionSettings
        /// </summary>
        private ReGoapState<T, W> actionSetting;
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
        /// 与world和并在一起的Goal
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
        private ReGoapNode() {
            expandList = new List<INode<ReGoapState<T, W>>>();
        }

        private void Init(IGoapPlanner<T,W> planner,ReGoapState<T,W> newGoal,ReGoapNode<T,W> parent,
            IReGoapAction<T,W> action,ReGoapState<T,W> setting) {
            expandList.Clear();
            this.planner = planner;
            this.parent = parent;
            this.action = action;

            if (setting != null) {
                this.actionSetting = setting.Clone();
            }

            if (parent != null) {
                state = parent.GetState().Clone();
                g = parent.GetPathCost();
            } else {
                state = planner.GetCurrentAgent().GetMemory().GetWorldState().Clone();
            }

            var nextAction = parent == null ? null : parent.action;
            if (action != null) {
                Goal = ReGoapState<T, W>.Instantiate(newGoal);

                GoapActionStackData<T, W> stackData;
                stackData.currentState = state;
                stackData.goalState = Goal;
                stackData.next = action;
                stackData.agent = planner.GetCurrentAgent();
                stackData.settings = actionSetting;

                PreConditions = action.GetPreconditions(stackData);
                Effects = action.GetEffect(stackData);
                //把这个action的权重加到g上
                g += action.GetCost(stackData);
                //把action的Effect state 加到当前的node上
                state.AddFromState(Effects);
                //只在Goal状态集中保留与Effects中不同的部分
                Goal.ReplaceWithMissingDifference(Effects);
                //把preconditions 的precondition加到当前的goal上
                Goal.AddFromState(PreConditions);
            } else {
                Goal = newGoal;
            }
            h = Goal.Count;
            //计算总权重
            cost = g + h * heuristicMultiplier;

            //在没有World Effect情况下再计算一次Goal,来看看我们是否完成了目标
            var diff = ReGoapState<T, W>.Instantiate();
            Goal.MissingDifference(planner.GetCurrentAgent().GetMemory().GetWorldState(), ref diff);
            goalMergedWithWorld = diff; 
        }

        #region NodeFactory
        private static Stack<ReGoapNode<T, W>> cachedNodes;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="count"></param>
        public static void Warmup(int count) {
            cachedNodes = new Stack<ReGoapNode<T, W>>(count);
            for (int i = 0; i < count; i++) {
                cachedNodes.Push(new ReGoapNode<T, W>());
            }
        }
        /// <summary>
        /// 回收
        /// </summary>
        public void Recycle() {
            state.Recycle();
            state = null;
            Goal.Recycle();
            Goal = null;
            g = 0;
            lock (cachedNodes) {
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
        /// <returns>封装好的节点</returns>
        public static ReGoapNode<T,W> Instantiate(IGoapPlanner<T, W> planner, ReGoapState<T, W> newGoal,
            ReGoapNode<T, W> parent, IReGoapAction<T, W> action, ReGoapState<T, W> actionSettings{

            ReGoapNode<T, W> node;
            if (cachedNodes == null) {
                cachedNodes = new Stack<ReGoapNode<T, W>>();
            }
            lock (cachedNodes) {
                node = cachedNodes.Count > 0 ? cachedNodes.Pop() : new ReGoapNode<T, W>();
            }
            node.Init(planner, newGoal, parent, action, actionSettings);
            return node;
        }
        #endregion

        public string Name {
            get { return action != null ? action.GetName() : "NoAction"; } 
        }

        public ReGoapState<T, W> Goal { get; private set; }

        public ReGoapState<T, W> Effects { get; private set; }

        public ReGoapState<T, W> PreConditions { get; private set; }

        /// <summary>
        /// 优先级队列中索引
        /// </summary>
        public int QueueIndex { get; set; }
        /// <summary>
        /// 优先级（越大越受青睐）
        /// </summary>
        public float Priority { get; set; }
        /// <summary>
        /// 插入点的索引
        /// </summary>
        public long InsertionIndex { get; set; }

        /// <summary>
        /// 与另一个结点对比权重
        /// </summary>
        public int CompareTo(INode<ReGoapState<T, W>> other) {
            return cost.CompareTo(other.GetCost());
        }
        /// <summary>
        /// 进行拓扑
        /// </summary>
        public List<INode<ReGoapState<T, W>>> Expand() {
            expandList.Clear();
            var agent = planner.GetCurrentAgent();
            var actions = agent.GetActionSet();
            GoapActionStackData<T, W> stackData;
            stackData.currentState = state;
            stackData.goalState = Goal;
            stackData.next = action;
            stackData.agent = agent;
            stackData.settings = null;
            for (int index = actions.Count - 1; index >= 0; index--) {
                var possibleAction = actions[index];
                possibleAction.Precalculations(stackData);
                var settingList = possibleAction.GetSettings(stackData);

                foreach (var settings in settingList) {
                    stackData.settings = settings;
                    var precond = possibleAction.GetPreconditions(stackData);
                    var effects = possibleAction.GetEffect(stackData);

                    if (effects.HasAny(Goal) && !Goal.HasAnyConflict(effects,precond) && 
                        !Goal.HasAnyConflict(effects) && possibleAction.CheckProceduralCondition(stackData)){
                        var newGoal = Goal;
                        expandList.Add(Instantiate(planner, newGoal, this, possibleAction, settings));
                    }
                }
            }
            return expandList;
        }
        /// <summary>
        /// 获取权重（越小越受青睐）
        /// </summary>
        public float GetCost() {
            return cost;
        }
        /// <summary>
        /// 启发式权重（越小越受青睐）
        /// </summary>
        /// <returns></returns>
        public float GetHeuristicCost() {
            return h;
        }
        /// <summary>
        /// 获取父结点
        /// </summary>
        public INode<ReGoapState<T, W>> GetParent() {
            return parent;
        }
        /// <summary>
        /// 路径点权重（越小越受青睐）
        /// </summary>
        /// <returns></returns>
        public float GetPathCost() {
            return g;
        }
        /// <summary>
        /// 获取状态
        /// </summary>
        public ReGoapState<T, W> GetState() {
            return state;
        }
        /// <summary>
        /// 是否为Goal
        /// </summary>
        public bool IsGoal(ReGoapState<T, W> goal) {
            return goalMergedWithWorld.Count <= 0;
        }
        /// <summary>
        /// 计算路径（从自身向上索引父结点）
        /// </summary>
        public Queue<ReGoapActionState<T,W>> CalculatePath() {
            var result = new Queue<ReGoapActionState<T, W>>();
            CalculatePath(ref result);
            return result;
        }
        /// <summary>
        /// 计算路径（从自身向上索引父结点）
        /// </summary>
        public void CalculatePath(ref Queue<ReGoapActionState<T, W>> result) {
            var node = this;
            while (node.GetParent() != null) {
                result.Enqueue(new ReGoapActionState<T, W>(node.action, node.actionSetting));
                node = (ReGoapNode<T, W>)node.GetParent();
            }
        }
    }
}