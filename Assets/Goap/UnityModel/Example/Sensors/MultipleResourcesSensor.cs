using System.Collections.Generic;

using UnityEngine;

using ReGoap.Unity.FSMExample.OtherScripts;

// the agent in this example is a villager which knows the location of trees, so seeTree is always true if there is an available  tree
namespace ReGoap.Unity.FSMExample.Sensors
{
    /// <summary>
    /// 资源信息集合
    /// </summary>
    public struct ResourcePair
    {
        public IResource resource;
        public Vector3 position;
    }

    /// <summary>
    /// 多种资源感知者
    /// </summary>
    public class MultipleResourcesSensor : ReGoapSensor<string, object>
    {
        /// <summary>
        /// 进行memory初始化，获取当前全部资源
        /// </summary>
        void Start()
        {
            var worldState = memory.GetWorldState();

            foreach (var pair in MultipleResourcesManager.Instance.Resources)
            {
                var resourceManager = pair.Value;

                var resources = new List<ResourcePair>(resourceManager.GetResourcesCount());
                foreach (var resource in resourceManager.GetResources())
                {
                    resources.Add(new ResourcePair { position = resource.GetTransform().position, resource = resource });
                }

                worldState.Set("resource" + resourceManager.GetResourceName(), resources);
            }
        }
    }
}
