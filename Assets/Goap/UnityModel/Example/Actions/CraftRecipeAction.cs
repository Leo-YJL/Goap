using System;
using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions
{
    /// <summary>
    /// 根据配方进行加工Action
    /// </summary>
    [RequireComponent(typeof(ResourcesBag))]
    public class CraftRecipeAction : ReGoapAction<string, object>
    {
        /// <summary>
        /// 原始配方（SO文件）
        /// </summary>
        public ScriptableObject RawRecipe;

        /// <summary>
        /// 配方
        /// </summary>
        private IRecipe recipe;

        /// <summary>
        /// 资源背包
        /// </summary>
        private ResourcesBag resourcesBag;
        
        /// <summary>
        /// 设置
        /// </summary>
        private List<ReGoapState<string, object>> settingsList;

        protected override void Awake()
        {
            base.Awake();
            //recipe = RawRecipe as IRecipe;
            //if (recipe == null)
            //    throw new UnityException("[CraftRecipeAction] The rawRecipe ScriptableObject must implement IRecipe.");
            resourcesBag = GetComponent<ResourcesBag>();

            if (RawRecipe != null) {
                SetRecipe(RawRecipe as IRecipe);
            }
          //  SetRecipe(RawRecipe as Recipe);
            settingsList = new List<ReGoapState<string, object>>();
        }

        public void SetRecipe(IRecipe _re) {
            recipe = _re as IRecipe;
            foreach (var pair in recipe.GetNeededResources()) {
                preconditions.Set("hasResource" + pair.Key, true);
            }

            effects.Set("hasResource" + recipe.GetCraftedResource(), true);

        }

        /// <summary>
        /// 获取设置
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
        {
            if (settingsList.Count == 0)
                CalculateSettingsList(stackData);
            return settingsList;
        }

        /// <summary>
        /// 计算设置列表
        /// </summary>
        /// <param name="stackData"></param>
        private void CalculateSettingsList(GoapActionStackData<string, object> stackData)
        {
            settingsList.Clear();
            // push all available workstations
            foreach (var workstationsPair in (Dictionary<Workstation, Vector3>) stackData.currentState.Get(
                "workstations"))
            {
                if (workstationsPair.Key.GetName() == recipe.GetCraftedResource() ||
                    (recipe.GetCraftedLevel() >= 2 && workstationsPair.Key.GetName() == "Tool"))
                {
                    settings.Set("workstation", workstationsPair.Key);
                    settings.Set("workstationPosition", workstationsPair.Value);
                    settingsList.Add(settings.Clone());

                }
              
            }
        }

        /// <summary>
        /// 计算先决条件
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            return base.CheckProceduralCondition(stackData) && stackData.settings.HasKey("workstation");
        }

        /// <summary>
        /// 获取先决条件
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData)
        {
            if (stackData.settings.TryGetValue("workstationPosition", out var workstationPosition))
                preconditions.Set("isAtPosition", workstationPosition);
            return preconditions;
        }

        /// <summary>
        /// 运行动作结点
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
            var workstation = settings.Get("workstation") as Workstation;
            if (workstation != null && workstation.CraftResource(resourcesBag, recipe))
            {
                ReGoapLogger.Log("[CraftRecipeAction] crafted recipe " + recipe.GetCraftedResource());
                done(this);
            }
            else
            {
                fail(this);
            }
        }
    }
}