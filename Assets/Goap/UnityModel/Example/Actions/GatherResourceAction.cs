using System;
using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions
{
    /// <summary>
    /// 收集资源Action
    /// </summary>
    public class GatherResourceAction : ReGoapAction<string, object>
    {
        /// <summary>
        /// 最大资源数
        /// </summary>
        public float MaxResourcesCount = 5.0f;

        /// <summary>
        /// 资源权值因数
        /// </summary>
        public float ResourcesCostMultiplier = 10.0f;

        /// <summary>
        /// 存储权值因数
        /// </summary>
        public float ReservedCostMultiplier = 50.0f;

        /// <summary>
        /// 是否展开所有资源
        /// </summary>
        public bool ExpandOnAllResources = false;

        /// <summary>
        /// 用于收集的时间
        /// </summary>
        public float TimeToGather = 0.5f;
        
        /// <summary>
        /// 每次收集数目
        /// </summary>
        public float ResourcePerAction = 1f;
        
        /// <summary>
        /// 资源背包
        /// </summary>
        protected ResourcesBag bag;
        
        /// <summary>
        /// 资源位置
        /// </summary>
        protected Vector3? resourcePosition;
        
        /// <summary>
        /// 资源
        /// </summary>
        protected IResource resource;

        /// <summary>
        /// 下一次可收集资源的时间点
        /// </summary>
        private float gatherCooldown;

        protected override void Awake()
        {
            base.Awake();

            bag = GetComponent<ResourcesBag>();
        }

        /// <summary>
        /// 从Goal获取所需的资源
        /// </summary>
        /// <param name="goalState"></param>
        /// <returns></returns>
        protected virtual string GetNeededResourceFromGoal(ReGoapState<string, object> goalState)
        {
            foreach (var pair in goalState.GetValues())
            {
                if (pair.Key.StartsWith("hasResource"))
                {
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
        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData)
        {
            preconditions.Clear();
            if (stackData.settings.HasKey("resource")
                && stackData.settings.TryGetValue("resourcePosition", out var resourcePosition))
            {
                preconditions.Set("isAtPosition", resourcePosition);
            }

            return preconditions;
        }

        /// <summary>
        /// 获取效果
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            effects.Clear();

            if (stackData.settings.TryGetValue("resource", out var obj))
            {
                var resource = (IResource) obj;
                if (resource != null)
                    effects.Set("hasResource" + resource.GetName(), true);
            }

            return effects;
        }

        /// <summary>
        /// 获取设置
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
        {
            var newNeededResourceName = GetNeededResourceFromGoal(stackData.goalState);
            settings.Clear();
            if (newNeededResourceName != null && stackData.currentState.HasKey("resource" + newNeededResourceName))
            {
                var results = new List<ReGoapState<string, object>>();
                Sensors.ResourcePair best = new Sensors.ResourcePair();
                var bestScore = float.MaxValue;
                foreach (var wantedResource in (List<Sensors.ResourcePair>) stackData.currentState.Get(
                    "resource" + newNeededResourceName))
                {
                    if (wantedResource.resource.GetCapacity() < ResourcePerAction) continue;
                    // expanding on all resources is VERY expansive, expanding on the closest one is usually the best decision
                    if (ExpandOnAllResources)
                    {
                        settings.Set("resourcePosition", wantedResource.position);
                        settings.Set("resource", wantedResource.resource);
                        results.Add(settings.Clone());
                    }
                    else
                    {
                        var score = stackData.currentState.TryGetValue("isAtPosition",
                            out object isAtPosition)
                            ? (wantedResource.position - (Vector3) isAtPosition).magnitude
                            : 0.0f;
                        score += ReservedCostMultiplier * wantedResource.resource.GetReserveCount();
                        score += ResourcesCostMultiplier * (MaxResourcesCount - wantedResource.resource.GetCapacity());
                        if (score < bestScore)
                        {
                            bestScore = score;
                            best = wantedResource;
                        }
                    }
                }

                if (!ExpandOnAllResources)
                {
                    settings.Set("resourcePosition", best.position);
                    settings.Set("resource", best.resource);
                    results.Add(settings.Clone());
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
        public override float GetCost(GoapActionStackData<string, object> stackData)
        {
            var extraCost = 0.0f;
            if (stackData.settings.HasKey("resource"))
            {
                var resource = (Resource) stackData.settings.Get("resource");
                extraCost += ReservedCostMultiplier * resource.GetReserveCount();
                extraCost += ResourcesCostMultiplier * (MaxResourcesCount - resource.GetCapacity());
            }

            return base.GetCost(stackData) + extraCost;
        }

        /// <summary>
        /// 检查先决条件
        /// </summary>
        /// <param name="stackData"></param>
        /// <returns></returns>
        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            return base.CheckProceduralCondition(stackData) && bag != null && stackData.settings.HasKey("resource");
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
                IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, settings, goalState, done, fail);
            var thisSettings = settings;
            resourcePosition = (Vector3) thisSettings.Get("resourcePosition");
            resource = (IResource) thisSettings.Get("resource");
            if (resource == null || resource.GetCapacity() < ResourcePerAction)

                failCallback(this);
            else

            {
                gatherCooldown = Time.time + TimeToGather;
            }
        }

        /// <summary>
        /// 当Planner把这个Action加入规划时
        /// </summary>
        /// <param name="previousAction"></param>
        /// <param name="nextAction"></param>
        /// <param name="settings"></param>
        /// <param name="goalState"></param>
        public override void PlanEnter(IReGoapAction<string, object> previousAction, IReGoapAction<string, object>
            nextAction, ReGoapState<string, object> settings, ReGoapState<string, object> goalState)
        {
            if (settings.HasKey("resource"))
            {
                ((IResource) settings.Get("resource")).Reserve(GetHashCode());
            }
        }

        /// <summary>
        /// 当Planner把这个Action从规划中排除时
        /// </summary>
        /// <param name="previousAction"></param>
        /// <param name="nextAction"></param>
        /// <param name="settings"></param>
        /// <param name="goalState"></param>
        public override void PlanExit(IReGoapAction<string, object> previousAction,
            IReGoapAction<string, object> nextAction
            , ReGoapState<string, object> settings, ReGoapState<string, object> goalState)
        {
            if (settings.HasKey("resource"))
            {
                ((IResource) settings.Get("resource")).Unreserve(GetHashCode());
            }
        }

        protected void Update()
        {
            if (resource == null || resource.GetCapacity() < ResourcePerAction)
            {
                failCallback(this);
                return;
            }

            if (Time.time > gatherCooldown)
            {
                gatherCooldown = float.MaxValue;
                ReGoapLogger.Log("[GatherResourceAction] acquired " + ResourcePerAction + " " + resource.GetName());
                resource.RemoveResource(ResourcePerAction);
                bag.AddResource(resource.GetName(), ResourcePerAction);
                doneCallback(this);
                if (settings.HasKey("resource"))
                {
                    ((IResource) settings.Get("resource")).Unreserve(GetHashCode());
                }
            }
        }
    }
}