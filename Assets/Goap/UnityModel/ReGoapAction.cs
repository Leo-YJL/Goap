using System;
using System.Collections.Generic;
using ReGoap.Core;
using UnityEngine;

namespace ReGoap.Unity
{
    /// <summary>
    /// Action基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapAction<T, W> : MonoBehaviour, IReGoapAction<T, W>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name = "GoapAction";

        /// <summary>
        /// 先决条件
        /// </summary>
        protected ReGoapState<T, W> preconditions;

        /// <summary>
        /// 效果
        /// </summary>
        protected ReGoapState<T, W> effects;

        /// <summary>
        /// 权重（越小越受青睐）
        /// </summary>
        public float Cost = 1;

        /// <summary>
        /// 完成时回调
        /// </summary>
        protected Action<IReGoapAction<T, W>> doneCallback;

        /// <summary>
        /// 失败时回调
        /// </summary>
        protected Action<IReGoapAction<T, W>> failCallback;

        /// <summary>
        /// 前一个结点
        /// </summary>
        protected IReGoapAction<T, W> previousAction;

        /// <summary>
        /// 下一个结点
        /// </summary>
        protected IReGoapAction<T, W> nextAction;

        /// <summary>
        /// AI代理
        /// </summary>
        protected IReGoapAgent<T, W> agent;

        /// <summary>
        /// 在可能的时候，是否可以跳出
        /// </summary>
        protected bool interruptWhenPossible;

        /// <summary>
        /// 设置
        /// </summary>
        protected ReGoapState<T, W> settings = null;

        /// <summary>
        /// Unity的生命周期函数
        /// </summary>

        #region UnityFunctions

        protected virtual void Awake()
        {
            enabled = false;

            effects = ReGoapState<T, W>.Instantiate();
            preconditions = ReGoapState<T, W>.Instantiate();

            settings = ReGoapState<T, W>.Instantiate();
        }

        protected virtual void Start()
        {
        }

        #endregion

        /// <summary>
        /// 是否处于激活态
        /// </summary>
        /// <returns></returns>
        public virtual bool IsActive()
        {
            return enabled;
        }

        /// <summary>
        /// 发布计算计划
        /// </summary>
        /// <param name="goapAgent"></param>
        public virtual void PostPlanCalculations(IReGoapAgent<T, W> goapAgent)
        {
            agent = goapAgent;
        }

        /// <summary>
        /// 是否可以跳出
        /// </summary>
        /// <returns></returns>
        public virtual bool IsInterruptable()
        {
            return true;
        }

        /// <summary>
        /// 请求跳出
        /// </summary>
        public virtual void AskForInterruption()
        {
            interruptWhenPossible = true;
        }

        /// <summary>
        /// 预计算
        /// </summary>
        /// <param name="stackData"></param>
        public virtual void Precalculations(GoapActionStackData<T, W> stackData)
        {
            agent = stackData.agent;
        }

        /// <summary>
        /// 获取设置
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public virtual List<ReGoapState<T, W>> GetSettings(GoapActionStackData<T, W> stackData)
        {
            return new List<ReGoapState<T, W>> {settings};
        }

        /// <summary>
        /// 获取先决条件
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public virtual ReGoapState<T, W> GetPreconditions(GoapActionStackData<T, W> stackData)
        {
            return preconditions;
        }

        /// <summary>
        /// 获取效果
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public virtual ReGoapState<T, W> GetEffects(GoapActionStackData<T, W> stackData)
        {
            return effects;
        }

        /// <summary>
        /// 获取权重（越小越受青睐）
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public virtual float GetCost(GoapActionStackData<T, W> stackData)
        {
            return Cost;
        }

        /// <summary>
        /// 检查先决条件
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public virtual bool CheckProceduralCondition(GoapActionStackData<T, W> stackData)
        {
            return true;
        }

        /// <summary>
        /// 开始执行Action
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="next"></param>
        /// <param name="settings"></param>
        /// <param name="goalState"></param>
        /// <param name="done"></param>
        /// <param name="fail"></param>
        public virtual void Run(IReGoapAction<T, W> previous, IReGoapAction<T, W> next, ReGoapState<T, W> settings,
            ReGoapState<T, W> goalState, Action<IReGoapAction<T, W>> done, Action<IReGoapAction<T, W>> fail)
        {
            interruptWhenPossible = false;
            enabled = true;
            doneCallback = done;
            failCallback = fail;
            this.settings = settings;

            previousAction = previous;
            nextAction = next;
        }

        /// <summary>
        /// 当规划到自己的时候会执行
        /// </summary>
        /// <param name="previousAction"></param>
        /// <param name="nextAction"></param>
        /// <param name="settings"></param>
        /// <param name="goalState"></param>
        public virtual void PlanEnter(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction,
            ReGoapState<T, W> settings, ReGoapState<T, W> goalState)
        {
        }

        /// <summary>
        /// 当规划从自己离开的时候会执行
        /// </summary>
        /// <param name="previousAction"></param>
        /// <param name="nextAction"></param>
        /// <param name="settings"></param>
        /// <param name="goalState"></param>
        public virtual void PlanExit(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction,
            ReGoapState<T, W> settings, ReGoapState<T, W> goalState)
        {
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="next"></param>
        public virtual void Exit(IReGoapAction<T, W> next)
        {
            if (gameObject != null)
                enabled = false;
        }

        /// <summary>
        /// 获取名称
        /// </summary>
        /// <returns></returns>
        public virtual string GetName()
        {
            return Name;
        }

        public override string ToString()
        {
            return string.Format("GoapAction('{0}')", Name);
        }

        public virtual string ToString(GoapActionStackData<T, W> stackData)
        {
            string result = string.Format("GoapAction('{0}')", Name);
            if (stackData.settings != null && stackData.settings.Count > 0)
            {
                result += " - ";
                foreach (var pair in stackData.settings.GetValues())
                {
                    result += string.Format("{0}='{1}' ; ", pair.Key, pair.Value);
                }
            }

            return result;
        }
    }
}