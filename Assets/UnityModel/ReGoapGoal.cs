using System;
using System.Collections.Generic;
using System.Linq;
using ReGoap.Core;
using ReGoap.Planner;
using UnityEngine;

// generic goal, should inherit this to do your own goal
namespace ReGoap.Unity {

    /// <summary>
    /// Goal
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapGoal<T, W> : MonoBehaviour, IReGoapGoal<T, W> {
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
        /// 关系可能的Goal
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
        /// Planner
        /// </summary>
        protected IGoapPlanner<T, W> planner;

        #region UnityFunctions
        /// <summary>
        /// 初始化Goal
        /// </summary>
        protected virtual void Awake() {
            goal = ReGoapState<T, W>.Instantiate();
        }
        protected virtual void Start() {
      
        }
        protected virtual void OnDestroy() {
            goal.Recycle();
        }


        #endregion

        #region 实现IReGoapGoal接口
        /// <summary>
        /// 获取名称
        /// </summary>
        public string GetName() {
            return Name;
        }
        /// <summary>
        /// 获取优先级（越大越受青睐）
        /// </summary>
        public float GetPriority() {
            return Priority;
        }
        /// <summary>
        /// 是否关心可能的Goal
        /// </summary>
        public bool IsGoalPossible() {
            return WarnPossibleGoal;
        }
        /// <summary>
        /// 获取Plan队列
        /// </summary>
        public Queue<ReGoapActionState<T, W>> GetPlan() {
            return plan;
        }
        /// <summary>
        /// 设置Plan队列
        /// </summary>
        public void SetPlan(Queue<ReGoapActionState<T, W>> path) {
            plan = path;
        }
        /// <summary>
        /// 获取Goal状态
        /// </summary>
        public ReGoapState<T, W> GetGoalState() {
            return goal;
        }
        /// <summary>
        /// 运行
        /// </summary>
        public void Run(Action<IReGoapGoal<T, W>> callBack) {
           
        }
        /// <summary>
        /// 预计算
        /// </summary>
        public void Precalculations(IGoapPlanner<T, W> goapPlanner) {
            planner = goapPlanner;
        }
        /// <summary>
        /// 获取错误延时
        /// </summary>
        public float GetErrorDelay() {
            return ErrorDelay;
        }
        #endregion
        /// <summary>
        /// Plan转string
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        public static string PlanToString(IEnumerable<IReGoapAction<T, W>> plan) {
            var result = "GoapPlan(";
            var reGoapActions = plan as IReGoapAction<T, W>[] ?? plan.ToArray();
            var end = reGoapActions.Length;
            for (var index = 0; index < end; index++) {
                var action = reGoapActions[index];
                result += string.Format("'{0}'{1}", action, index + 1 < end ? ", " : "");
            }

            result += ")";
            return result;
        }

        public override string ToString() {
            return string.Format("GoapGoal('{0}')", Name);
        }
    }
}