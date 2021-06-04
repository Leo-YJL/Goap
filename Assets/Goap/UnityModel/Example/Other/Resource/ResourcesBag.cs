using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    /// <summary>
    /// 资源背包
    /// </summary>
    public class ResourcesBag : MonoBehaviour
    {
        /// <summary>
        /// 包含哪些资源
        /// </summary>
        private Dictionary<string, float> resources;

        void Awake()
        {
            resources = new Dictionary<string, float>();
        }

        /// <summary>
        /// 增加资源
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="value"></param>
        public void AddResource(string resourceName, float value)
        {
            if (!resources.ContainsKey(resourceName))
                resources[resourceName] = 0;
            resources[resourceName] += value;
        }

        /// <summary>
        /// 获取单个资源
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public float GetResource(string resourceName)
        {
            var value = 0f;
            resources.TryGetValue(resourceName, out value);
            return value;
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, float> GetResources()
        {
            return resources;
        }

        /// <summary>
        /// 移除（减少）资源
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="value"></param>
        public void RemoveResource(string resourceName, float value)
        {
            resources[resourceName] -= value;
        }
    }
}