using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Core;
using System;

namespace ReGoap.Unity {
    /// <summary>
    /// Action基类
    /// </summary>
    public class ReGoapAction<T, W> : MonoBehaviour, IReGoapAction<T, W> {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name = "GaopAction";
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
        protected Action<IReGoapAction<T, W>> doneCallBack;
        /// <summary>
        /// 失败时回调
        /// </summary>
        protected Action<IReGoapAction<T, W>> failCallBack;
        /// <summary>
        /// 前一个节点
        /// </summary>
        protected IReGoapAction<T, W> previousAction;
        /// <summary>
        /// 下一个节点
        /// </summary>
        protected IReGoapAction<T, W> nextAction;
        /// <summary>
        /// AI 代理
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


        #region  Unity的生命周期函数
        protected virtual void Awake() {

            enabled = false;
            effects = ReGoapState<T, W>.Instantiate();
            preconditions = ReGoapState<T, W>.Instantiate();

            settings = ReGoapState<T, W>.Instantiate();
        }

        protected virtual void Start() {
            
        }

        #endregion
        /// <summary>
        /// 是否处于激活态
        /// </summary>
        public virtual bool IsActive() {
            return enabled;
        }
        /// <summary>
        /// 发布计算计划
        /// </summary>
        /// <param name="goapAgent"></param>
        public virtual void PostPlanCalculations(IReGoapAgent<T,W> goapAgent) {
            agent = goapAgent;
        }
        /// <summary>
        /// 是否可以跳出
        /// </summary>
        public virtual bool IsInterruptable() {
            return true;
        }
        /// <summary>
        /// 请求跳出
        /// </summary>
        public virtual void AskForInterruption() {
            interruptWhenPossible = true;
        }
        /// <summary>
        /// 预计算
        /// </summary>
        public virtual void Precalculations(GoapActionStackData<T, W> stackData) {
            agent = stackData.agent;
        }
        /// <summary>
        /// 获取设置
        /// </summary>
        public List<ReGoapState<T, W>> GetSettings(GoapActionStackData<T, W> stackData) {
            return new List<ReGoapState<T, W>> { settings };
        }
        /// <summary>
        /// 获取先决条件
        /// </summary>
        public ReGoapState<T, W> GetPreconditions(GoapActionStackData<T, W> stackData) {
            return preconditions;
        }
        /// <summary>
        /// 获取效果
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public ReGoapState<T, W> GetEffect(GoapActionStackData<T, W> stackData) {
            return effects;
        }
        /// <summary>
        /// 获取权重
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public float GetCost(GoapActionStackData<T, W> stackData) {
            return Cost;
        }
        /// <summary>
        /// 检查先决条件
        /// </summary>
        public bool CheckProceduralCondition(GoapActionStackData<T, W> stackData) {
            return true;
        }
        /// <summary>
        /// 开始执行Action
        /// </summary>
        public void Run(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goapState,
            Action<IReGoapAction<T, W>> done, Action<IReGoapAction<T, W>> fail) {
            interruptWhenPossible = false;
            enabled = true;
            doneCallBack = done;
            failCallBack = fail;
            this.settings = settings;
            this.previousAction = previousAction;
            this.nextAction = nextAction;
        }
        /// <summary>
        /// 获取名称
        /// </summary>
        /// <returns></returns>
        public string GetName() {
            return Name;
        }
        /// <summary>
        /// 当规划到自己的时候会执行
        /// </summary>
        public void PlanEnter(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goapState) {
            
        }
        /// <summary>
        /// 当规划从自己离开的时候会执行
        /// </summary>
        public void PlanExit(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goapState) {

        }

        /// <summary>
        /// 退出
        /// </summary>
        public void Exit(IReGoapAction<T, W> nextAction) {
            if (gameObject != null)
                enabled = false;
        }



        public override string ToString() {
            return string.Format("GoapAction('{0}')", Name);
        }


        public virtual string ToString(GoapActionStackData<T, W> stackData) {
            string result = string.Format("GoapAction('{0}')", Name);
            if (stackData.settings != null && stackData.settings.Count > 0) {
                result += " - ";
                foreach (var pair in stackData.settings.GetValues()) {
                    result += string.Format("{0}='{1}' ; ", pair.Key, pair.Value);
                }
            }

            return result;
        }
    }
}