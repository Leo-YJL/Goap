using System;
using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.OtherScripts;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions
{
    /// <summary>
    /// 把自身资源放入银行Action
    /// </summary>
    [RequireComponent(typeof(ResourcesBag))]
    public class AddResourceToBankAction : ReGoapAction<string, object>
    {
        /// <summary>
        /// 资源背包
        /// </summary>
        private ResourcesBag resourcesBag;

        /// <summary>
        /// 每个资源的设置
        /// </summary>
        private Dictionary<string, List<ReGoapState<string, object>>> settingsPerResource;

        protected override void Awake()
        {
            base.Awake();
            resourcesBag = GetComponent<ResourcesBag>();
            settingsPerResource = new Dictionary<string, List<ReGoapState<string, object>>>();
        }

        /// <summary>
        /// 检查先决条件
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            return base.CheckProceduralCondition(stackData) && stackData.settings.HasKey("Ware");
        }

        /// <summary>
        /// 获取设置
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
        {
            foreach (var pair in stackData.goalState.GetValues())
            {
                if (pair.Key.StartsWith("collectedResource"))
                {
                    var resourceName = pair.Key.Substring(17);
                    if (settingsPerResource.ContainsKey(resourceName))
                        return settingsPerResource[resourceName];
                    var results = new List<ReGoapState<string, object>>();
                    settings.Set("resourceName", resourceName);
                    // push all available banks
                    //获取当前记忆所有bank
                    foreach (var banksPair in (Dictionary<Ware, Vector3>) stackData.currentState.Get("wares"))
                    {
                        if (resourceName == banksPair.Key.GetName()) {
                            settings.Set("Ware", banksPair.Key);
                            settings.Set("WarePosition", banksPair.Value);
                            results.Add(settings.Clone());
                        }
                       
                    }

                    settingsPerResource[resourceName] = results;
                    return results;
                }
            }

            return base.GetSettings(stackData);
        }

        /// <summary>
        /// 获取效果
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            if (stackData.settings.HasKey("resourceName"))
                effects.Set("collectedResource" + stackData.settings.Get("resourceName") as string, true);
            return effects;
        }

        /// <summary>
        /// 获取前置条件
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData)
        {
            if (stackData.settings.HasKey("Ware"))
                preconditions.Set("isAtPosition", stackData.settings.Get("WarePosition"));
            if (stackData.settings.HasKey("resourceName"))
                preconditions.Set("hasResource" + stackData.settings.Get("resourceName") as string, true);
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
            Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, settings, goalState, done, fail);
            this.settings = settings;
            var bank = settings.Get("Ware") as Ware;
            if (bank != null && bank.AddResource(resourcesBag, (string) settings.Get("resourceName")))
            {
                done(this);
            }
            else
            {   
                fail(this);
            }
        }
    }
}