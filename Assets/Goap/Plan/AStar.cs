using ReGoap.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Planner { 
    /// <summary>
    /// A* 用于寻找规划最短路径
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AStar<T> {
        /// <summary>
        /// 低优先队列
        /// </summary>
        private readonly FastPriorityQueue<INode<T>, T> frontier;
        /// <summary>
        /// state节点（把State封装成Node）字典
        /// </summary>
        private readonly Dictionary<T, INode<T>> stateToNode;
        /// <summary>
        /// 探索节点字典
        /// </summary>
        private readonly Dictionary<T, INode<T>> explored;
        /// <summary>
        /// 创建节点字典
        /// </summary>
        private readonly List<INode<T>> createdNodes;
        /// <summary>
        /// 是否开启Debug Plan （配合ParseRaws.bat批处理命令生成本地AI Plan图）
        /// </summary>
        private bool debugPlan = false;

        /// <summary>
        /// Debugger（配合ParseRaws.bat批处理命令生成本地AI Plan图）
        /// </summary>
        private PlanDebugger debugger;


        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="maxNodesExpand"></param>
        public AStar(int maxNodesExpand) {
            frontier = new FastPriorityQueue<INode<T>, T>(maxNodesExpand);
            stateToNode = new Dictionary<T, INode<T>>();
            explored = new Dictionary<T, INode<T>>();
            createdNodes = new List<INode<T>>(maxNodesExpand);
        }
        /// <summary>
        /// 清理所有节点
        /// </summary>
        public void ClearNodes() {
            foreach (var node in createdNodes) {
                node.Recycle();
            }
            createdNodes.Clear();
        }

        /// <summary>
        /// 可视化Debug部分（配合ParseRaws.bat批处理命令生成本地AI Plan图）
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        private void DebugPlan(INode<T> node, INode<T> parent) {
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

            if (parent != null) {
                string connStr = string.Format("{0} -> {1}", parent.GetHashCode(), node.GetHashCode());
                debugger.AddConn(connStr);
            }
        }
        /// <summary>
        /// 停止Debug
        /// </summary>
        /// <param name="node"></param>
        private void EndDebugPlan(INode<T> node) {
            if (debugger != null) {
                while (node != null) {
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

        public INode<T> Run(INode<T> start, T goal,int maxIterations = 100,bool earlExit = true,
            bool clearNodes = true,bool debugPlan = false) {
            this.debugPlan = debugPlan;
            frontier.Clear();
            stateToNode.Clear();
            explored.Clear();
            if (clearNodes) {
                ClearNodes();
                createdNodes.Add(start);
            }
            //先放入start节点
            frontier.Enqueue(start, start.GetCost());

            DebugPlan(start, null);

            var iterations = 0;
            //BFS遍历
            while ((frontier.Count > 0 &&(iterations < maxIterations) && (frontier.Count + 1 < frontier.MaxSize))){
                //取出cost最小的队首
                var node = frontier.Dequeue();
                //如果是Goal就退出规划
                if (node.IsGoal(goal)) {
                    ReGoapLogger.Log("[Astar] Success iterations: " + iterations);
                    EndDebugPlan(node);
                    return node;
                }
                //否则加入已探索节点
                explored[node.GetState()] = node;

                foreach (var child in node.Expand()) {
                    iterations++;//迭代次数+1
                    if (clearNodes) {
                        createdNodes.Add(child);
                    }
                    //如果是Goal就退出规划
                    if (earlExit && child.IsGoal(goal)) {
                        ReGoapLogger.Log("[Astar] (early exit)  Success iterations: " + iterations);
                        EndDebugPlan(child);
                        return node;
                    }

                    var childCost = child.GetCost();
                    var state = child.GetState();
                    //如果已经遍历过，就进行下一个结点的遍历
                    if (explored.ContainsKey(state)) {
                        continue;
                    }

                    INode<T> similiarNode;
                    //如果在stateToNode字典有相同状态的结点
                    stateToNode.TryGetValue(state, out similiarNode);
                    if (similiarNode != null) {
                        //权值大于当前子结点，就把similiarNode从stateToNode字典移除
                        //这是理所应当的，因为要保证权值最小
                        if (similiarNode.GetCost() > childCost)
                            frontier.Remove(similiarNode);
                        else //否则就结束本次子拓扑结点遍历
                            break;
                    }

                    DebugPlan(child, node);
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
    /// A*的节点
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INode<T> {
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
        /// 对比两个节点
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
        /// 获取父节点
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
        /// 回收节点
        /// </summary>
        void Recycle();
    }

    /// <summary>
    /// Node对比接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NodeComparer<T> : IComparer<INode<T>> {
        public int Compare(INode<T> x, INode<T> y) {
            var result = x.CompareTo(y);
            if (result == 0) {
                return 1;
            }
            return result;
        }
    }
}
