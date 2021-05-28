using System;
using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.OtherScripts;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions {
    /// <summary>
    /// 把自身资源放入银行Action
    /// </summary>
    [RequireComponent(typeof(ResourcesBag))]
    public class AddResourceToBankAction : ReGoapAction<string, object> {
        /// <summary>
        /// 资源背包
        /// </summary>
        private ResourcesBag resourcesBag;
        /// <summary>
        /// 每个资源的设置
        /// </summary>
        private Dictionary<string, List<ReGoapState<string, object>>> settingsPerResource;

        protected override void Awake() {
            base.Awake();
            resourcesBag = GetComponent<ResourcesBag>();
            settingsPerResource = new Dictionary<string, List<ReGoapState<string, object>>>();
        }
        /// <summary>
        /// 检查先决条件
        /// </summary>
        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData) {
            return base.CheckProceduralCondition(stackData) && stackData.settings.HasKey("bank");
        }
        /// <summary>
        /// 获取设置
        /// </summary>
        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData) {
            foreach (var pair in stackData.goalState.GetValues()) {
                if (pair.Key.StartsWith("collectedResource")) {
                    var resourceName = pair.Key.Substring(17);
                    if (settingsPerResource.ContainsKey(resourceName))
                        return settingsPerResource[resourceName];
                    var results = new List<ReGoapState<string, object>>();
                    settings.Set("resourceName", resourceName);
                    // push all available banks
                    //获取当前记忆所有bank
                    foreach (var banksPair in (Dictionary<Bank, Vector3>)stackData.currentState.Get("banks")) {
                        settings.Set("bank", banksPair.Key);
                        settings.Set("bankPosition", banksPair.Value);
                        results.Add(settings.Clone());
                    }

                    settingsPerResource[resourceName] = results;
                    return results;
                }
            }


            return base.GetSettings(stackData);
        }
    }
}