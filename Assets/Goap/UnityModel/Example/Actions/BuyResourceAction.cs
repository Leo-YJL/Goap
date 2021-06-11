using System;
using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions {
    /// <summary>
    /// 收集资源Action
    /// </summary>
    public class BuyResourceAction : ReGoapAction<string, object> {


        /// <summary>
        /// 资源背包
        /// </summary>
        private ResourcesBag resourcesBag;

        protected override void Awake() {
            base.Awake();
            resourcesBag = GetComponent<ResourcesBag>();
        }

        /// <summary>
        /// 从Goal获取所需的资源
        /// </summary>
        /// <param name="goalState"></param>
        /// <returns></returns>
        protected virtual string GetNeededResourceFromGoal(ReGoapState<string, object> goalState) {
            foreach (var pair in goalState.GetValues()) {
                if (pair.Key.StartsWith("hasResource")) {
                    return pair.Key.Substring(11);
                }
            }

            return null;
        }

        /// <summary>
        /// 获取前置条件
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData) {
            preconditions.Clear();
            var newNeededResourceName = GetNeededResourceFromGoal(stackData.goalState);
            if (newNeededResourceName == "Axe") {
                foreach (var banksPair in (Dictionary<Ware, Vector3>)stackData.currentState.Get("wares")) {
                    var resource = banksPair.Key.GetResource(newNeededResourceName);
                    if (resource > 0) {
                        preconditions.Set("isAtPosition", banksPair.Value);
                    }
                }
            }
        
            return preconditions;
        }

        /// <summary>
        /// 获取效果
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData) {
            effects.Clear();

            if (preconditions.Count > 0) {
                effects.Set("hasResourceAxe", true);
            }

            return effects;
        }

        /// <summary>
        /// 获取设置
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData) {
            var newNeededResourceName = GetNeededResourceFromGoal(stackData.goalState);
            settings.Clear();
            if (newNeededResourceName == "Axe") {
                var results = new List<ReGoapState<string, object>>();
                foreach (var banksPair in (Dictionary<Ware, Vector3>)stackData.currentState.Get("wares")) {
                    var resource = banksPair.Key.GetResource(newNeededResourceName);
                    if (resource > 0) {
                        settings.Set("resourcePosition", banksPair.Value);
                        settings.Set("resourceWare", banksPair.Key);
                        results.Add(settings.Clone());
                        break;
                    }
                }
                return results;
            }

            return new List<ReGoapState<string, object>>();
        }

        /// <summary>
        /// 获取Cost
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override float GetCost(GoapActionStackData<string, object> stackData) {
            var extraCost = 0.0f;

            return base.GetCost(stackData) + extraCost;
        }

        /// <summary>
        /// 检查先决条件
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData) {
            return base.CheckProceduralCondition(stackData) && stackData.settings.Get("resourceWare") != null;
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
            ReGoapState<
                string, object> settings, ReGoapState<string, object> goalState,
            Action<IReGoapAction<string, object>> done, Action<
                IReGoapAction<string, object>> fail) {
            base.Run(previous, next, settings, goalState, done, fail);
            var thisSettings = settings;

            var resourceWare = (Ware)thisSettings.Get("resourceWare");
            if (resourceWare == null) {
                failCallback(this);
            } else {
                resourceWare.RemoveResource("Axe", 1);
                resourcesBag.AddResource("Axe", 1);
                doneCallback(this);
            }
            //resource = (IResource)thisSettings.Get("resource");
            //if (resource == null || resource.GetCapacity() < ResourcePerAction)

            //    
            //else {
            //    gatherCooldown = Time.time + TimeToGather;
            //}
        }

    }
}

