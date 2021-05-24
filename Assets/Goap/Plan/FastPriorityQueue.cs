using System.Collections;
using System.Collections.Generic;
using System;
namespace ReGoap.Planner {
    /// <summary>
    /// 一个使用小顶堆实现的低优先级队列
    /// </summary>
    public class FastPriorityQueue<T, U> where T : class,INode<U> {

        private int numNodes;

        private T[] nodes;

        public FastPriorityQueue(int maxNodes) {
#if DEBUG
            if (maxNodes <= 0) {
                throw new InvalidOperationException("new queue size cannot be smaller than 1");
            }
#endif
            numNodes = 0;
            nodes = new T[maxNodes + 1];
        }
        /// <summary>
        /// 返回节点数量 o(1)
        /// </summary>
        public int Count {
            get {
                return numNodes;
            }
        }
        /// <summary>
        /// 放回一次性可加入队列的最大数目，一旦触发了这个数目，将要被加入的那些元素将会有不可预料的行为出现
        /// </summary>
        public int MaxSize {
            get { return nodes.Length - 1; }
        }
        /// <summary>
        ///  从队列移除每一个元素，O(n)复杂度，所以不要经常干这件事
        /// </summary>
        public void Clear() {
            Array.Clear(nodes, 1, numNodes);
            numNodes = 0;
        }
        /// <summary>
        /// 是否包含某个元素
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool Contains(T node) {
#if DEBUG

            if (node == null) {
                throw new ArgumentNullException("node");
            }

            if (node.QueueIndex < 0 || node.QueueIndex >= nodes.Length) {
                throw new InvalidOperationException(
                    "node.QueueIndex has been corrupted. Did you change it manually? Or add this node to another queue?");
            }
#endif
            return (nodes[node.QueueIndex] == node);
        }

        public void Enqueue(T node, float priority) {
#if DEBUG
            if (node == null) {
                throw new ArgumentNullException("node");
            }

            if (numNodes >= nodes.Length - 1) {
                throw new InvalidOperationException("Queue is full - node cannot be added: " + node);
            }

            if (Contains(node)) {
                throw new InvalidOperationException("Node is already enqueued: " + node);
            }
#endif
            node.Priority = priority;
            numNodes++;
            node.QueueIndex = numNodes;
            CascadeUp(nodes[numNodes]);
        }
        /// <summary>
        /// 交换节点
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        public void Swap(T node1,T node2) {
            nodes[node1.QueueIndex] = node2;
            nodes[node2.QueueIndex] = node1;

            int temp = node1.QueueIndex;
            node1.QueueIndex = node2.QueueIndex;
            node2.QueueIndex = temp;
        }
        /// <summary>
        /// 重构小顶堆 （向上）
        /// </summary>
        /// <param name="node"></param>
        private void CascadeUp(T node) {
            int parent = node.QueueIndex / 2;
            while (parent >= 1) {
                T parentNode = nodes[parent];
                if (HasHigherPriority(parentNode,node)) {
                    break;
                }

                Swap(node, parentNode);
                parent = node.QueueIndex / 2;
            }
        }

        private void CascadeDown(T node) {
            T newParent;
            int finialQueueIndex = node.QueueIndex;
            while (true) {
                newParent = node;
                int childLeftIndex = 2 * finialQueueIndex;
                //防止越界
                if (childLeftIndex > numNodes) {
                    node.QueueIndex = finialQueueIndex;
                    nodes[finialQueueIndex] = node;
                    break;
                }
                //如果左孩子比当前结点优先级高
                T childLeft = nodes[childLeftIndex];
                if (HasHigherPriority(childLeft,newParent)) {
                    newParent = childLeft;
                }
                //检查右孩子结点优先级是否比当前/左孩子结点高
                int childRightIndex = childLeftIndex + 1;
                if (childRightIndex <= numNodes) {
                    T childRight = nodes[childRightIndex];
                    if (HasHigherPriority(childRight,newParent)) {
                        newParent = childRight;
                    }
                }
                //如果当中任何一个孩子结点拥有比原结点更高的优先级，就继续重建堆
                if (newParent != node) {
                    //把高优先级结点往上移动，只移动一次，避免使用了Swap函数（同优化版快排）
                    nodes[finialQueueIndex] = newParent;
                    int temp = newParent.QueueIndex;
                    newParent.QueueIndex = finialQueueIndex;
                    finialQueueIndex = temp;
                } else {
                    node.QueueIndex = finialQueueIndex;
                    nodes[finialQueueIndex] = node;
                    break;
                }

            }
        }

        /// <summary>
        /// 检查是否higher比Lower有更高的优先级（即higher.priority小于lower.Priority），注意，如果两者优先级一致将返回false
        /// </summary>
        /// <param name="higher"></param>
        /// <param name="lower"></param>
        /// <returns></returns>
        private bool HasHigherPriority(T higher,T lower) {
            return higher.Priority < lower.Priority;
        }

        /// <summary>
        /// 移除并返回队首
        /// </summary>
        /// <returns></returns>
        public T Dequeue() {
#if DEBUG
            if (numNodes <= 0) {
                throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
            }

            if (!IsValidQueue()) {
                throw new InvalidOperationException(
                    "Queue has been corrupted (Did you update a node priority manually instead of calling UpdatePriority()?" +
                    "Or add the same node to two different queues?)");
            }
#endif
            T returnMe = nodes[1];
            Remove(returnMe);
            return returnMe;
        }


        public T First {
            get {
#if DEBUG
                if (numNodes <= 0) {
                    throw new InvalidOperationException("Cannot call .First on an empty queue");
                }
#endif
                return nodes[1];
            }
        }


        /// <summary>
        /// 重新扩容队列，当前所有结点都将保留，如果是减容的话，已存在的结点行为时无法预料的
        /// </summary>
        /// <param name="maxNodes"></param>
        public void Resize(int maxNodes) {
#if DEBUG
            if (maxNodes <= 0) {
                throw new InvalidOperationException("Queue size cannot be smaller than 1");
            }

            if (maxNodes < numNodes) {
                throw new InvalidOperationException("Called Resize(" + maxNodes + "), but current queue contains " +
                                                    numNodes + " nodes");
            }
#endif

            T[] newArray = new T[maxNodes + 1];
            int highestIndexToCopy = Math.Min(maxNodes, numNodes);
            for (int i = 0; i <= highestIndexToCopy; i++) {
                newArray[i] = nodes[i];
            }
            nodes = newArray;
        }
        /// <summary>
        /// 当一个结点优先级变化时必须调用这个函数，否则整个队列都将坏掉
        /// </summary>
        /// <param name="node"></param>
        /// <param name="priority"></param>
        public void UpdatePriority(T node,float priority) {
#if DEBUG
            if (node == null) {
                throw new ArgumentNullException("node");
            }

            if (!Contains(node)) {
                throw new InvalidOperationException("Cannot call UpdatePriority() on a node which is not enqueued: " +
                                                    node);
            }
#endif

            node.Priority = priority;
            OnNodeUpdated(node);
        }
        /// <summary>
        /// 更新一个节点
        /// </summary>
        /// <param name="node"></param>
        private void OnNodeUpdated(T node) {
            int parentIdx = node.QueueIndex / 2;
            T parentNode = nodes[parentIdx];
            if (parentIdx > 0 && HasHigherPriority(node,parentNode)) {
                CascadeUp(node);
            } else {
                CascadeDown(node);
            }
        }
        /// <summary>
        /// 从队列移除一个结点，这个节点不一定是队首结点
        /// </summary>
        /// <param name="node"></param>
        public void Remove(T node) {
#if DEBUG
            if (node == null) {
                throw new ArgumentNullException("node");
            }

            if (!Contains(node)) {
                throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + node);
            }
#endif
            //如果这个结点已经是最后一个结点，就直接移除
            if (node.QueueIndex == numNodes) {
                nodes[numNodes] = null;
                numNodes--;
                return;
            }
            //把当前结点与最后一个结点互换
            T formerLastNode = nodes[numNodes];
            Swap(node, formerLastNode);
            nodes[numNodes] = null;
            numNodes--;
            //重新构建堆
            OnNodeUpdated(formerLastNode);
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < numNodes; i++) {
                yield return nodes[i];
            }
        }
        /// <summary>
        /// 检查这个队列是否处于异常状态
        /// </summary>
        /// <returns></returns>
        public bool IsValidQueue() {
            for (int i = 0; i < nodes.Length; i++) {
                if (nodes[i] != null) {
                    int childLeftIndex = 2 * i;
                    if (childLeftIndex < nodes.Length && nodes[childLeftIndex] != null && 
                        HasHigherPriority(nodes[childLeftIndex],nodes[i]))
                            return false;
                    int childRightIndex = childLeftIndex + 1;
                    if (childRightIndex < nodes.Length && nodes[childRightIndex] != null &&
                        HasHigherPriority(nodes[childRightIndex], nodes[i]))
                        return false;
                }
            }
            return true;
        }
    }
}