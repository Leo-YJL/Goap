using System;
using System.Collections.Generic;

namespace ReGoap.Planner
{
    /// <summary>
    /// An implementation of a min-Priority Queue using a heap.  Has O(1) .Contains()!
    /// See https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/wiki/Getting-Started for more information
    /// 一个使用小顶堆实现的低优先级队列
    /// </summary>
    public sealed class FastPriorityQueue<T, U>
        where T : class, INode<U>
    {
        /// <summary>
        /// 结点数量
        /// </summary>
        private int numNodes;

        /// <summary>
        /// 所有结点
        /// </summary>
        private T[] nodes;

        /// <summary>
        /// Instantiate a new Priority Queue
        /// 初始化一个低优先级队列
        /// </summary>
        /// <param name="maxNodes">The max nodes ever allowed to be enqueued (going over this will cause undefined behavior)</param>
        public FastPriorityQueue(int maxNodes)
        {
#if DEBUG
            if (maxNodes <= 0)
            {
                throw new InvalidOperationException("New queue size cannot be smaller than 1");
            }
#endif

            numNodes = 0;
            nodes = new T[maxNodes + 1];
        }

        /// <summary>
        /// Returns the number of nodes in the queue.
        /// 返回结点数量
        /// O(1)
        /// </summary>
        public int Count
        {
            get { return numNodes; }
        }

        /// <summary>
        /// Returns the maximum number of items that can be enqueued at once in this queue.  Once you hit this number (ie. once Count == MaxSize),
        /// attempting to enqueue another item will cause undefined behavior.  O(1)
        /// 放回一次性可加入队列的最大数目，一旦触发了这个数目，将要被加入的那些元素将会有不可预料的行为出现
        /// </summary>
        public int MaxSize
        {
            get { return nodes.Length - 1; }
        }

        /// <summary>
        /// Removes every node from the queue.
        /// O(n) (So, don't do this often!)
        /// 从队列移除每一个元素，O(n)复杂度，所以不要经常干这件事
        /// </summary>
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Clear()
        {
            Array.Clear(nodes, 1, numNodes);
            numNodes = 0;
        }

        /// <summary>
        /// Returns (in O(1)!) whether the given node is in the queue.  O(1)
        /// 是否包含某个元素
        /// </summary>
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(T node)
        {
#if DEBUG
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (node.QueueIndex < 0 || node.QueueIndex >= nodes.Length)
            {
                throw new InvalidOperationException(
                    "node.QueueIndex has been corrupted. Did you change it manually? Or add this node to another queue?");
            }
#endif

            return (nodes[node.QueueIndex] == node);
        }

        /// <summary>
        /// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
        /// If the queue is full, the result is undefined.
        /// If the node is already enqueued, the result is undefined.
        /// O(log n)
        /// 向队列加入一个节点，低优先级将会放到前面，然后连接将会重组（重构小顶堆）
        /// </summary>
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Enqueue(T node, float priority)
        {
#if DEBUG
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (numNodes >= nodes.Length - 1)
            {
                throw new InvalidOperationException("Queue is full - node cannot be added: " + node);
            }

            if (Contains(node))
            {
                throw new InvalidOperationException("Node is already enqueued: " + node);
            }
#endif

            node.Priority = priority;
            numNodes++;
            nodes[numNodes] = node;
            node.QueueIndex = numNodes;
            CascadeUp(nodes[numNodes]);
        }

        /// <summary>
        /// 交换两个节点
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void Swap(T node1, T node2)
        {
            //Swap the nodes
            nodes[node1.QueueIndex] = node2;
            nodes[node2.QueueIndex] = node1;

            //Swap their indicies
            int temp = node1.QueueIndex;
            node1.QueueIndex = node2.QueueIndex;
            node2.QueueIndex = temp;
        }

        //Performance appears to be slightly better when this is NOT inlined o_O
        /// <summary>
        /// 重新构建小顶堆（向上）
        /// </summary>
        /// <param name="node"></param>
        private void CascadeUp(T node)
        {
            //aka Heapify-up
            int parent = node.QueueIndex / 2;
            while (parent >= 1)
            {
                T parentNode = nodes[parent];
                if (HasHigherPriority(parentNode, node))
                    break;

                //Node has lower priority value, so move it up the heap
                Swap(node,
                    parentNode); //For some reason, this is faster with Swap() rather than (less..?) individual operations, like in CascadeDown()

                parent = node.QueueIndex / 2;
            }
        }

        /// <summary>
        /// 重新构建小顶堆（向下）
        /// </summary>
        /// <param name="node"></param>
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void CascadeDown(T node)
        {
            //aka Heapify-down
            T newParent;
            int finalQueueIndex = node.QueueIndex;
            while (true)
            {
                newParent = node;
                int childLeftIndex = 2 * finalQueueIndex;
                
                //防止越界
                if (childLeftIndex > numNodes)
                {
                    //This could be placed outside the loop, but then we'd have to check newParent != node twice
                    //这也可以放在循环外进行替换，但是我们就不得不需要进行两次判断了
                    node.QueueIndex = finalQueueIndex;
                    nodes[finalQueueIndex] = node;
                    break;
                }

                //Check if the left-child is higher-priority than the current node
                //如果左孩子比当前结点优先级高
                T childLeft = nodes[childLeftIndex];
                if (HasHigherPriority(childLeft, newParent))
                {
                    newParent = childLeft;
                }

                //Check if the right-child is higher-priority than either the current node or the left child
                //检查右孩子结点优先级是否比当前/左孩子结点高
                int childRightIndex = childLeftIndex + 1;
                if (childRightIndex <= numNodes)
                {
                    T childRight = nodes[childRightIndex];
                    if (HasHigherPriority(childRight, newParent))
                    {
                        newParent = childRight;
                    }
                }

                //If either of the children has higher (smaller) priority, swap and continue cascading
                //如果当中任何一个孩子结点拥有比原结点更高的优先级，就继续重建堆
                if (newParent != node)
                {
                    //Move new parent to its new index.  node will be moved once, at the end
                    //Doing it this way is one less assignment operation than calling Swap()
                    //把高优先级结点往上移动，只移动一次，避免使用了Swap函数（同优化版快排）
                    nodes[finalQueueIndex] = newParent;

                    int temp = newParent.QueueIndex;
                    newParent.QueueIndex = finalQueueIndex;
                    finalQueueIndex = temp;
                }
                else
                {
                    //See note above
                    node.QueueIndex = finalQueueIndex;
                    nodes[finalQueueIndex] = node;
                    break;
                }
            }
        }

        /// <summary>
        /// Returns true if 'higher' has higher priority than 'lower', false otherwise.
        /// Note that calling HasHigherPriority(node, node) (ie. both arguments the same node) will return false
        /// 检查是否higher比lower有更高的优先级（即higher.priority小于lower.Priority），注意，如果两者优先级一致将返回false
        /// </summary>
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool HasHigherPriority(T higher, T lower)
        {
            return (higher.Priority < lower.Priority);
        }

        /// <summary>
        /// Removes the head of the queue and returns it.
        /// If queue is empty, result is undefined
        /// O(log n)
        /// 移除并返回队首
        /// </summary>
        public T Dequeue()
        {
#if DEBUG
            if (numNodes <= 0)
            {
                throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
            }

            if (!IsValidQueue())
            {
                throw new InvalidOperationException(
                    "Queue has been corrupted (Did you update a node priority manually instead of calling UpdatePriority()?" +
                    "Or add the same node to two different queues?)");
            }
#endif

            T returnMe = nodes[1];
            Remove(returnMe);
            return returnMe;
        }

        /// <summary>
        /// Resize the queue so it can accept more nodes.  All currently enqueued nodes are remain.
        /// Attempting to decrease the queue size to a size too small to hold the existing nodes results in undefined behavior
        /// O(n)
        /// 重新扩容队列，当前所有结点都将保留，如果是减容的话，已存在的结点行为时无法预料的
        /// </summary>
        public void Resize(int maxNodes)
        {
#if DEBUG
            if (maxNodes <= 0)
            {
                throw new InvalidOperationException("Queue size cannot be smaller than 1");
            }

            if (maxNodes < numNodes)
            {
                throw new InvalidOperationException("Called Resize(" + maxNodes + "), but current queue contains " +
                                                    numNodes + " nodes");
            }
#endif

            T[] newArray = new T[maxNodes + 1];
            int highestIndexToCopy = Math.Min(maxNodes, numNodes);
            for (int i = 1; i <= highestIndexToCopy; i++)
            {
                newArray[i] = nodes[i];
            }

            nodes = newArray;
        }

        /// <summary>
        /// Returns the head of the queue, without removing it (use Dequeue() for that).
        /// If the queue is empty, behavior is undefined.
        /// O(1)
        /// 返回队首结点，但不移除它
        /// </summary>
        public T First
        {
            get
            {
#if DEBUG
                if (numNodes <= 0)
                {
                    throw new InvalidOperationException("Cannot call .First on an empty queue");
                }
#endif

                return nodes[1];
            }
        }

        /// <summary>
        /// This method must be called on a node every time its priority changes while it is in the queue.  
        /// <b>Forgetting to call this method will result in a corrupted queue!</b>
        /// Calling this method on a node not in the queue results in undefined behavior
        /// O(log n)
        /// 当一个结点优先级变化时必须调用这个函数，否则整个队列都将坏掉
        /// </summary>
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void UpdatePriority(T node, float priority)
        {
#if DEBUG
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (!Contains(node))
            {
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
        private void OnNodeUpdated(T node)
        {
            //Bubble the updated node up or down as appropriate
            int parentIndex = node.QueueIndex / 2;
            T parentNode = nodes[parentIndex];

            if (parentIndex > 0 && HasHigherPriority(node, parentNode))
            {
                //向上重建二叉堆
                CascadeUp(node);
            }
            else
            {
                //Note that CascadeDown will be called if parentNode == node (that is, node is the root)
                //CascadeDown将会在node自身为root时调用
                //向下重建二叉堆
                CascadeDown(node);
            }
        }

        /// <summary>
        /// Removes a node from the queue.  The node does not need to be the head of the queue.  
        /// If the node is not in the queue, the result is undefined.  If unsure, check Contains() first
        /// O(log n)
        /// 从队列移除一个结点，这个节点不一定是队首结点
        /// </summary>
        public void Remove(T node)
        {
#if DEBUG
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (!Contains(node))
            {
                throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + node);
            }
#endif

            //If the node is already the last node, we can remove it immediately
            //如果这个结点已经是最后一个结点，就直接移除
            if (node.QueueIndex == numNodes)
            {
                nodes[numNodes] = null;
                numNodes--;
                return;
            }

            //Swap the node with the last node
            //把当前结点与最后一个结点互换
            T formerLastNode = nodes[numNodes];
            Swap(node, formerLastNode);
            nodes[numNodes] = null;
            numNodes--;

            //Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
            //重新构建堆
            OnNodeUpdated(formerLastNode);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 1; i <= numNodes; i++)
                yield return nodes[i];
        }

        /// <summary>
        /// <b>Should not be called in production code.</b>
        /// Checks to make sure the queue is still in a valid state.  Used for testing/debugging the queue.
        /// 检查这个队列是否处于异常状态
        /// </summary>
        public bool IsValidQueue()
        {
            for (int i = 1; i < nodes.Length; i++)
            {
                if (nodes[i] != null)
                {
                    int childLeftIndex = 2 * i;
                    if (childLeftIndex < nodes.Length && nodes[childLeftIndex] != null &&
                        HasHigherPriority(nodes[childLeftIndex], nodes[i]))
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