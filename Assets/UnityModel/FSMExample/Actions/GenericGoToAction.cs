using System;
using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.FSM;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions {
    /// <summary>
    /// 走向某地Action
    /// </summary>
    public class GenericGoToAction : ReGoapAction<string,object> {
        /// <summary>
        /// GoTo状态，用于处理移动
        /// </summary>
        protected SmsGoTo smsGoTo;
   
        protected override void Awake() {
            base.Awake();
            smsGoTo = GetComponent<SmsGoTo>();
        }
        /// <summary>
        /// 运行此结点
        /// </summary>

        public override void Run(IReGoapAction<string, object> previousAction, IReGoapAction<string, object> nextAction, ReGoapState<string, object> settings, ReGoapState<string, object> goapState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail) {
            base.Run(previousAction, nextAction, settings, goapState, done, fail);

            if (settings.TryGetValue("objectivePosition",out var v)) {
                smsGoTo.GoTo((Vector3)v, OnDoneMovement, OnFailureMovement);
            }
        }

        /// <summary>
        /// 当移动失败
        /// </summary>
        protected virtual void OnFailureMovement() {
            failCallback(this);
        }

        /// <summary>
        /// 当移动成功
        /// </summary>
        protected virtual void OnDoneMovement() {
            doneCallback(this);
        }
    }
}