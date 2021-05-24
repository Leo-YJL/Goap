using System.Collections;
using System.Collections.Generic;
using System;
namespace ReGoap.Core{

    /// <summary>
    /// Action数据结构体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public struct GoapActionStackData<T, W> {
        /// <summary>
        /// 当前的状态
        /// </summary>
        public ReGoapState<T, W> currentState;
        /// <summary>
        /// 目标状态
        /// </summary>
        public ReGoapState<T, W> goalState;
        /// <summary>
        /// 代理
        /// </summary>
        public IReGoapAgent<T, W> agent;
        /// <summary>
        /// 下一个Action
        /// </summary>
        public IReGoapAction<T, W> next;
        /// <summary>
        /// 设置
        /// </summary>
        public ReGoapState<T, W> settings;
    }


    /// <summary>
    /// Action接口类
    /// </summary>
    public interface IReGoapAction<T,W> {

        /// <summary>
        /// 应该返回当前Action的计算参数，并将其添加到Run方法中
        /// 对动态Action很有用。例如GoTo动作可以保存一些方法（所需位置）
        /// 从Planner中选择时，我们会保存此信息并在运行方法时将其返回
        /// 大多数Action将返回单个元素的List,但是更复杂的操作可能会返回多个元素的List
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        List<ReGoapState<T, W>> GetSettings(GoapActionStackData<T, W> stackData);

        /// <summary>
        /// 运行Action
        /// </summary>
        /// <param name="previousAction">前一个Action</param>
        /// <param name="nextAction">下一个Action</param>
        /// <param name="settings">设置</param>
        /// <param name="goapState">模板</param>
        /// <param name="done">Action完成时调用</param>
        /// <param name="fail">Action失败时调用</param>
        void Run(IReGoapAction<T,W> previousAction,IReGoapAction<T,W> nextAction,ReGoapState<T,W> settings,
            ReGoapState<T,W> goapState,Action<IReGoapAction<T,W>> done,Action<IReGoapAction<T,W>> fail);

        /// <summary>
        /// 会在运行Action列表之前进行统一调用一次
        /// </summary>
        /// <param name="previousAction">前一个Action</param>
        /// <param name="nextAction">下一个Action</param>
        /// <param name="settings">设置</param>
        /// <param name="goapState">目标状态</param>
        void PlanEnter(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goapState);

        /// <summary>
        /// 会在取消运行Action列表时进行统一调用一次
        /// </summary>
        /// <param name="previousAction">前一个Action</param>
        /// <param name="nextAction">下一个Action</param>
        /// <param name="settings">设置</param>
        /// <param name="goapState">目标状态</param>
        void PlanExit(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goapState);

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="nextAction">下一个Action</param>
        void Exit(IReGoapAction<T, W> nextAction);
        /// <summary>
        /// 获取名字
        /// </summary>
        string GetName();
        /// <summary>
        /// 是否处于激活状态
        /// </summary>
        bool IsActive();

        /// <summary>
        /// 是否可以中断
        /// </summary>
        /// <returns></returns>
        bool IsInterruptable();

        /// <summary>
        /// 请求中断
        /// </summary>
        void AskForInterruption();
        /// <summary>
        /// 获取先决条件（注意线程安全）
        /// </summary>
        /// <param name="stackData">Action数据</param>
        /// <returns></returns>
        ReGoapState<T, W> GetPreconditions(GoapActionStackData<T, W> stackData);
        /// <summary>
        /// 获取效果（注意线程安全）
        /// </summary>
        /// <param name="stackData">Action数据</param>
        /// <returns></returns>
        ReGoapState<T, W> GetEffect(GoapActionStackData<T, W> stackData);
        /// <summary>
        /// 检查先决条件是否过关（必须注意线程安全）
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        bool CheckProceduralCondition(GoapActionStackData<T, W> stackData);

        /// <summary>
        /// 获取权重（必须注意线程安全），值越小越受青睐
        /// </summary>
        /// <param name="stackData">Action数据</param>
        /// <returns></returns>
        float GetCost(GoapActionStackData<T, W> stackData);
        /// <summary>
        /// 不要修改运行时的Action变量，即使一个Action正在运行，它的预计数会被执行很多次
        /// </summary>
        /// <param name="stackData"></param>
        void Precalculations(GoapActionStackData<T, W> stackData);
        /// <summary>
        /// 转string
        /// </summary>
        /// <param name="stackData">Action数据</param>
        /// <returns></returns>
        string ToString(GoapActionStackData<T,W> stackData);
    }
}

