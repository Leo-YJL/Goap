using System;
using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.OtherScripts;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions {

    public class BuyAction : ReGoapAction<string, object> {
        /// <summary>
        /// 资源背包
        /// </summary>
        private ResourcesBag resourcesBag;

        private Ware buyWare;
        private string buyResourceName;
        protected override void Awake() {
            base.Awake();
            resourcesBag = GetComponent<ResourcesBag>();
   
        }

        public void SetBuy(string str) {
            buyResourceName = str;

        }
        /// <summary>
        /// 检查先决条件
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData) {
            return base.CheckProceduralCondition(stackData) && stackData.settings.HasKey("Ware");
        }

        /// <summary>
        /// 获取设置
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData) {
           

            return base.GetSettings(stackData);
        }

        /// <summary>
        /// 获取效果
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData) {
            if (stackData.settings.HasKey("resourceName"))
                effects.Set("collectedResource" + stackData.settings.Get("resourceName") as string, true);
            return effects;
        }

        /// <summary>
        /// 获取前置条件
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData) {
            preconditions.Clear();
            if (true) {

            }
            return preconditions;
        }

        /// <summary>
        /// 运行此动作
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="next"></param>
        /// <param name="settings"></param>
        /// <param name="goalState"></param>
        /// <param name="done"></param>
        /// <param name="fail"></param>
        public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next,
            ReGoapState<string, object> settings, ReGoapState<string, object> goalState,
            Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail) {
            base.Run(previous, next, settings, goalState, done, fail);
            this.settings = settings;


       
            //if () {
            //    done(this);
            //} else {
            //    fail(this);
            //}
        }
    }
}
