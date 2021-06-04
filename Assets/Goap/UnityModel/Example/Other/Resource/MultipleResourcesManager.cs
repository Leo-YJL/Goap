using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    /// <summary>
    /// 资源管理器管理者（所有）
    /// </summary>
    public class MultipleResourcesManager : MonoBehaviour
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static MultipleResourcesManager Instance;

        /// <summary>
        /// 所有资源管理者
        /// </summary>
        public Dictionary<string, IResourceManager> Resources;

        /// <summary>
        /// 初始化所有资源管理者，并向其中装填资源
        /// </summary>
        /// <exception cref="UnityException"></exception>
        void Awake()
        {
            if (Instance != null)
                throw new UnityException("[ResourcesManager] Can have only one instance per scene.");
            Instance = this;
            var childResources = GetComponentsInChildren<IResource>();
            Resources = new Dictionary<string, IResourceManager>(childResources.Length);
            foreach (var resource in childResources)
            {
                if (!Resources.ContainsKey(resource.GetName()))
                {
                    var manager = gameObject.AddComponent<ResourceManager>();
                    manager.ResourceName = resource.GetName();
                    Resources[resource.GetName()] = manager;
                }
                Resources[resource.GetName()].AddResource(resource);
            }
        }
    }
}
