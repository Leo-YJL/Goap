using System;
using System.Collections.Generic;

namespace ReGoap.Unity.FSM
{
    /// <summary>
    /// FSM状态接口类
    /// </summary>
    public interface ISmState
    {
        /// <summary>
        /// 持有的切换类列表
        /// </summary>
        List<ISmTransition> Transitions { get; set; }

        /// <summary>
        /// 进入
        /// </summary>
        void Enter();

        /// <summary>
        /// 退出
        /// </summary>
        void Exit();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="stateMachine">归属的状态机，需要把这个状态加入状态机中</param>
        void Init(StateMachine stateMachine);

        /// <summary>
        /// 是否处于激活态
        /// </summary>
        /// <returns></returns>
        bool IsActive();

        /// <summary>
        /// 获取优先级（越大越受青睐）
        /// </summary>
        /// <returns></returns>
        int GetPriority();
    }

    /// <summary>
    /// FSM切换接口类
    /// </summary>
    public interface ISmTransition
    {
        /// <summary>
        /// 切换检查
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        Type TransitionCheck(ISmState state);

        /// <summary>
        /// 获取切换优先级（越大越受青睐）
        /// </summary>
        /// <returns></returns>
        int GetPriority();
    }

    // you can inherit your FSM's transition from this, but feel free to implement your own (note: must implement ISmTransition and IComparable<ISmTransition>)
    /// <summary>
    /// 可以继承这个类来实现自定义的FSM切换类，但是也可以通过继承ISmTransition和IComparable<ISmTransition/> 来实现
    /// </summary>
    public class SmTransition : ISmTransition, IComparable<ISmTransition>
    {
        /// <summary>
        /// 优先级（越大越受青睐）
        /// </summary>
        private readonly int priority;

        /// <summary>
        /// 检查是否能够切换
        /// </summary>
        private readonly Func<ISmState, Type> checkFunc;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="priority">优先级</param>
        /// <param name="checkFunc">检查是否能够切换</param>
        public SmTransition(int priority, Func<ISmState, Type> checkFunc)
        {
            this.priority = priority;
            this.checkFunc = checkFunc;
        }

        /// <summary>
        /// 切换检查
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public Type TransitionCheck(ISmState state)
        {
            return checkFunc(state);
        }

        /// <summary>
        /// 获取优先级（越大越受青睐）
        /// </summary>
        /// <returns></returns>
        public int GetPriority()
        {
            return priority;
        }

        /// <summary>
        /// 与另一个切换对比优先级，如果自身优先级小于目标优先级就返回大于0的整数，反之
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ISmTransition other)
        {
            return -GetPriority().CompareTo(other.GetPriority());
        }
    }
}