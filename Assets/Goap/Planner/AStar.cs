using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Utilities;

namespace ReGoap.Planner
{
    /// <summary>
    /// A*导航，用于寻找规划最短路径
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AStar<T>
    {
        /// <summary>
        /// 低优先级队列
        /// </summary>
        private readonly FastPriorityQueue<INode<T>, T> frontier;

        /// <summary>
        /// State结点（把State封装成Node）字典
        /// </summary>
        private readonly Dictionary<T, INode<T>> stateToNode;

        /// <summary>
        /// 探索结点字典
        /// </summary>
        private readonly Dictionary<T, INode<T>> explored;

        /// <summary>
        /// 创建结点字典
        /// </summary>
        private readonly List<INode<T>> createdNodes;

        // Debug
        /// <summary>
        /// 是否开启Debug Plan（配合ParseRaws.bat批处理命令生成本地AI Plan图）
        /// </summary>
        private bool debugPlan = false;

        /// <summary>
        /// Debugger（配合ParseRaws.bat批处理命令生成本地AI Plan图）
        /// </summary>
        private PlanDebugger debugger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxNodesToExpand">结点最大数量</param>
        public AStar(int maxNodesToExpand = 1000)
        {
            frontier = new FastPriorityQueue<INode<T>, T>(maxNodesToExpand);
            stateToNode = new Dictionary<T, INode<T>>();
            explored = new Dictionary<T, INode<T>>(); // State -> node
            createdNodes = new List<INode<T>>(maxNodesToExpand);
        }

        /// <summary>
        /// 清理所有结点
        /// </summary>
        void ClearNodes()
        {
            foreach (var node in createdNodes)
            {
                node.Recycle();
            }

            createdNodes.Clear();
        }

        /// <summary>
        /// 可视化Debug部分（配合ParseRaws.bat批处理命令生成本地AI Plan图）
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        private void DebugPlan(INode<T> node, INode<T> parent)
        {
            if (!debugPlan) return;
            if (debugger == null)
                debugger = new PlanDebugger();

            string nodeStr = string.Format(@"{0} [label=<
<table border='0' color='black' fontcolor='#F5F5F5'>
    <tr> <td colspan='2'><b>{4}</b></td> </tr>
    <hr/>
    <tr align='left'> <td border='1' sides='rt'><b>Costs</b></td>           <td border='1' sides='t'><b>g</b>: {1} ; <b>h</b>: {2} ; <b>c</b>: {3}</td> </tr>
    <tr align='left'> <td border='1' sides='rt'><b>Preconditions</b></td>   <td border='1' sides='t'>{5}</td> </tr>
    <tr align='left'> <td border='1' sides='rt'><b>Effects</b></td>         <td border='1' sides='t'>{6}</td> </tr>
    <tr align='left'> <td border='1' sides='rt'><b>Goal</b></td>            <td border='1' sides='t'>{7}</td> </tr>
</table>
>]",
                node.GetHashCode(),
                node.GetPathCost(), node.GetHeuristicCost(), node.GetCost(),
                node.Name, node.Preconditions != null ? node.Preconditions.ToString() : "",
                node.Effects != null ? node.Effects.ToString() : "",
                node.Goal != null ? node.Goal.ToString() : "");
            debugger.AddNode(nodeStr);

            if (parent != null)
            {
                string connStr = string.Format("{0} -> {1}", parent.GetHashCode(), node.GetHashCode());
                debugger.AddConn(connStr);
            }
        }

        /// <summary>
        /// 停止Debug
        /// </summary>
        /// <param name="node"></param>
        private void EndDebugPlan(INode<T> node)
        {
            if (debugger != null)
            {
                while (node != null)
                {
                    //mark success path
                    string nodeStr = string.Format("{0} [style=\"bold\" color=\"darkgreen\"]", node.GetHashCode());
                    debugger.AddNode(nodeStr);
                    node = node.GetParent();
                }

                var txt = debugger.TransformText();
                System.IO.Directory.CreateDirectory("PlanDebugger");
                System.IO.Directory.CreateDirectory("PlanDebugger/Raws");
                System.IO.File.WriteAllText(
                    string.Format("PlanDebugger/Raws/DebugPlan_{0}.dot", System.DateTime.Now.ToString("HHmmss_ffff")),
                    txt);
                debugger.Clear();
            }
        }

        /// <summary>
        /// 以start结点为起始点开始寻找能抵达目标的最短 && 权值最小路径
        /// </summary>
        /// <param name="start">起始结点</param>
        /// <param name="goal">目标</param>
        /// <param name="maxIterations">最大迭代次数</param>
        /// <param name="earlyExit">是否可以提前退出</param>
        /// <param name="clearNodes">是否清理当前已创建结点</param>
        /// <param name="debugPlan">是否对Plan进行Debug</param>
        /// <returns>如果规划成功返回的是Goal结点，否则是null</returns>
        public INode<T> Run(INode<T> start, T goal, int maxIterations = 100, bool earlyExit = true,
            bool clearNodes = true, bool debugPlan = false)
        {
            this.debugPlan = debugPlan;

            frontier.Clear();
            stateToNode.Clear();
            explored.Clear();
            if (clearNodes)
            {
                ClearNodes();
                createdNodes.Add(start);
            }

            //先把Start结点放进去
            frontier.Enqueue(start, start.GetCost());

            DebugPlan(start, null);

            var iterations = 0;
            //BFS遍历
            while ((frontier.Count > 0) && (iterations < maxIterations) && (frontier.Count + 1 < frontier.MaxSize))
            {
                //取出Cost最小的队首
                var node = frontier.Dequeue();
                //Utilities.ReGoapLogger.Log($"这次遍历的是{node.Name}-{node.GetCost()}-{node.GetHashCode()}");
                //如果是Goal就退出规划
                if (node.IsGoal(goal))
                {
                    ReGoapLogger.Log("[Astar] Success iterations: " + iterations);
                    EndDebugPlan(node);
                    return node;
                }

                //否则加入已探索结点
                explored[node.GetState()] = node;

                //对于当前node的每一个拓扑结点（邻接点）
                foreach (var child in node.Expand())
                {
                    iterations++; //迭代次数加一
                    if (clearNodes)
                    {
                        createdNodes.Add(child);
                    }

                    //如果是Goal就退出规划
                    if (earlyExit && child.IsGoal(goal))
                    {
                        ReGoapLogger.Log("[Astar] (early exit) Success iterations: " + iterations);
                        EndDebugPlan(child);
                        return child;
                    }

                    var childCost = child.GetCost();
                    var state = child.GetState();
                    //如果已经遍历过，就进行下一个结点的遍历
                    if (explored.ContainsKey(state))
                        continue;
                    INode<T> similiarNode;
                    //如果在stateToNode字典有相同状态的结点
                    stateToNode.TryGetValue(state, out similiarNode);
                    if (similiarNode != null)
                    {
                        //权值大于当前子结点，就把similiarNode从stateToNode字典移除
                        //这是理所应当的，因为要保证权值最小
                        if (similiarNode.GetCost() > childCost)
                            frontier.Remove(similiarNode);
                        else //否则就结束本次子拓扑结点遍历
                            break;
                    }

                    DebugPlan(child, node);
                    // Utilities.ReGoapLogger.Log(string.Format("    Enqueue frontier: {0}, cost: {1} - {2}", child.Name,
                    //     childCost, child.GetHashCode()));
                    //把当前子结点加入frontier
                    frontier.Enqueue(child, childCost);
                    //把当前子结点加入stateToNode字典
                    stateToNode[state] = child;
                }
            }

            ReGoapLogger.LogWarning("[Astar] failed.");
            EndDebugPlan(null);
            return null;
        }
    }

    /// <summary>
    /// A*中的结点接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INode<T>
    {
        /// <summary>
        /// 获取状态
        /// </summary>
        /// <returns></returns>
        T GetState();

        /// <summary>
        /// 拓扑图
        /// </summary>
        /// <returns></returns>
        List<INode<T>> Expand();

        /// <summary>
        /// 对比两个结点
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        int CompareTo(INode<T> other);

        /// <summary>
        /// 获取结点权重（越小越受青睐）
        /// </summary>
        /// <returns></returns>
        float GetCost();

        /// <summary>
        /// 获取启发式权重（启发式：在某些复杂情况下虽然不能保证结果一定最优，但保证速度更快）
        /// </summary>
        /// <returns></returns>
        float GetHeuristicCost();

        /// <summary>
        /// 获取路径权重（越小越受青睐）
        /// </summary>
        /// <returns></returns>
        float GetPathCost();

        /// <summary>
        /// 获取父结点
        /// </summary>
        /// <returns></returns>
        INode<T> GetParent();

        /// <summary>
        /// 此结点是否就是Goal(即达成了Goal的状态)
        /// </summary>
        /// <param name="goal"></param>
        /// <returns></returns>
        bool IsGoal(T goal);

        /// <summary>
        /// 获取名字
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取目标
        /// </summary>
        T Goal { get; }

        /// <summary>
        /// 获取影响
        /// </summary>
        T Effects { get; }

        /// <summary>
        /// 获取先决条件
        /// </summary>
        T Preconditions { get; }

        /// <summary>
        /// 获取低优先级队列中的值
        /// </summary>
        int QueueIndex { get; set; }

        /// <summary>
        /// 获取优先级（越大越受青睐）
        /// </summary>
        float Priority { get; set; }

        /// <summary>
        /// 回收结点
        /// </summary>
        void Recycle();
    }

    /// <summary>
    /// Node对比接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NodeComparer<T> : IComparer<INode<T>>
    {
        /// <summary>
        /// 对比两个节点，如果x权重大于等于y权重就返回大于0的数，否则就返回小于零的数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public int Compare(INode<T> x, INode<T> y)
        {
            var result = x.CompareTo(y);
            if (result == 0)
                return 1;
            return result;
        }
    }
}