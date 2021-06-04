using System;
using System.Collections.Generic;
using System.Linq;
using ReGoap.Core;
using ReGoap.Planner;
using UnityEngine;

// generic goal, should inherit this to do your own goal
namespace ReGoap.Unity
{
    /// <summary>
    /// Goal类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapGoal<T, W> : MonoBehaviour, IReGoapGoal<T, W>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name = "GenericGoal";

        /// <summary>
        /// 优先级（越大越受青睐）
        /// </summary>
        public float Priority = 1;

        /// <summary>
        /// 错误延时
        /// </summary>
        public float ErrorDelay = 0.5f;

        /// <summary>
        /// 关心可能的Goal
        /// </summary>
        public bool WarnPossibleGoal = true;

        /// <summary>
        /// Goal
        /// </summary>
        protected ReGoapState<T, W> goal;

        /// <summary>
        /// plan队列
        /// </summary>
        protected Queue<ReGoapActionState<T, W>> plan;

        /// <summary>
        /// planner
        /// </summary>
        protected IGoapPlanner<T, W> planner;

        #region UnityFunctions

        /// <summary>
        /// 初始化Goal
        /// </summary>
        protected virtual void Awake()
        {
            goal = ReGoapState<T, W>.Instantiate();
        }

        /// <summary>
        /// 销毁时回收Gaol
        /// </summary>
        protected virtual void OnDestroy()
        {
            goal.Recycle();
        }

        protected virtual void Start()
        {
        }

        #endregion

        #region IReGoapGoal

        /// <summary>
        /// 获取名称
        /// </summary>
        /// <returns></returns>
        public virtual string GetName()
        {
            return Name;
        }

        /// <summary>
        /// 获取优先级（越大越受青睐）
        /// </summary>
        /// <returns></returns>
        public virtual float GetPriority()
        {
            return Priority;
        }

        /// <summary>
        /// 是否关心可能的Goal
        /// </summary>
        /// <returns></returns>
        public virtual bool IsGoalPossible()
        {
            return WarnPossibleGoal;
        }

        /// <summary>
        /// 获取Plan队列
        /// </summary>
        /// <returns></returns>
        public virtual Queue<ReGoapActionState<T, W>> GetPlan()
        {
            return plan;
        }

        /// <summary>
        /// 获取Goal状态
        /// </summary>
        /// <returns></returns>
        public virtual ReGoapState<T, W> GetGoalState()
        {
            return goal;
        }

        /// <summary>
        /// 设置Plan队列
        /// </summary>
        /// <param name="path"></param>
        public virtual void SetPlan(Queue<ReGoapActionState<T, W>> path)
        {
            plan = path;
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="callback"></param>
        public void Run(Action<IReGoapGoal<T, W>> callback)
        {
        }

        /// <summary>
        /// 预计算
        /// </summary>
        /// <param name="goapPlanner"></param>
        public virtual void Precalculations(IGoapPlanner<T, W> goapPlanner)
        {
            planner = goapPlanner;
        }

        /// <summary>
        /// 获取错误延时
        /// </summary>
        /// <returns></returns>
        public virtual float GetErrorDelay()
        {
            return ErrorDelay;
        }

        #endregion

        /// <summary>
        /// Plan转string
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        public static string PlanToString(IEnumerable<IReGoapAction<T, W>> plan)
        {
            var result = "GoapPlan(";
            var reGoapActions = plan as IReGoapAction<T, W>[] ?? plan.ToArray();
            var end = reGoapActions.Length;
            for (var index = 0; index < end; index++)
            {
                var action = reGoapActions[index];
                result += string.Format("'{0}'{1}", action, index + 1 < end ? ", " : "");
            }

            result += ")";
            return result;
        }

        public override string ToString()
        {
            return string.Format("GoapGoal('{0}')", Name);
        }
    }
}