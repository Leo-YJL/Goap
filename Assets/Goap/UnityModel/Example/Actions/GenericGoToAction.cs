using System;
using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.FSM;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions
{   
    // you could also create a generic ExternalGoToAction : GenericGoToAction
    // which let you add effects / preconditions from some source (Unity, external file, etc.)
    // and then add multiple ExternalGoToAction to your agent's gameobject's behaviours
    // you can use this without any helper class by having the actions that need to move to a position
    // or transform to have a precondition isAtPosition
    /// <summary>
    /// 走向某地Action
    /// </summary>
    [RequireComponent(typeof(SmsGoTo))]
    public class GenericGoToAction : ReGoapAction<string, object>
    {
        // sometimes a Transform is better (moving target), sometimes you do not have one (last target position)
        //  but if you're using multi-thread approach you can't use a transform or any unity's API
        //GoTo状态，用于处理移动
        protected SmsGoTo smsGoto;

        protected override void Awake()
        {
            base.Awake();

            smsGoto = GetComponent<SmsGoTo>();
        }

        /// <summary>
        /// 运行此结点
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="next"></param>
        /// <param name="settings"></param>
        /// <param name="goalState"></param>
        /// <param name="done"></param>
        /// <param name="fail"></param>
        public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next,
            ReGoapState<string, object> settings, ReGoapState<string, object> goalState,
            Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, settings, goalState, done, fail);

            if (settings.TryGetValue("objectivePosition", out var v))
                smsGoto.GoTo((Vector3) v, OnDoneMovement, OnFailureMovement);
            else
                failCallback(this);
        }

        /// <summary>
        /// 检查先决条件
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            return base.CheckProceduralCondition(stackData) && stackData.settings.HasKey("objectivePosition");
        }

        /// <summary>
        /// 获取效果
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            if (stackData.settings.TryGetValue("objectivePosition", out var objectivePosition))
            {
                effects.Set("isAtPosition", objectivePosition);
                if (stackData.settings.HasKey("reconcilePosition"))
                    effects.Set("reconcilePosition", true);
            }
            else
            {
                effects.Clear();
            }

            return base.GetEffects(stackData);
        }

        /// <summary>
        /// 获取设置
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
        {
            if (stackData.goalState.TryGetValue("isAtPosition", out var isAtPosition))
            {
                settings.Set("objectivePosition", isAtPosition);
                return base.GetSettings(stackData);
            }
            else if (stackData.goalState.HasKey("reconcilePosition") && stackData.goalState.Count == 1)
            {
                settings.Set("objectivePosition", stackData.agent.GetMemory().GetWorldState().Get("startPosition"));
                settings.Set("reconcilePosition", true);
                return base.GetSettings(stackData);
            }

            return new List<ReGoapState<string, object>>();
        }

        // if you want to calculate costs use a non-dynamic/generic goto action
        /// <summary>
        /// 获取Cost
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override float GetCost(GoapActionStackData<string, object> stackData)
        {
            var distance = 0.0f;
            if (stackData.settings.TryGetValue("objectivePosition", out object objectivePosition)
                && stackData.currentState.TryGetValue("isAtPosition", out object isAtPosition))
            {
                if (objectivePosition is Vector3 p && isAtPosition is Vector3 r)
                    distance = (p - r).magnitude;
            }

            return base.GetCost(stackData) + Cost + distance;
        }

        /// <summary>
        /// 当移动失败
        /// </summary>
        protected virtual void OnFailureMovement()
        {
            failCallback(this);
        }

        /// <summary>
        /// 当移动成功
        /// </summary>
        protected virtual void OnDoneMovement()
        {
            doneCallback(this);
        }
    }
}