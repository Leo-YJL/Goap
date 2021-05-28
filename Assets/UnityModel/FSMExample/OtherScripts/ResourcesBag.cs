using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts {
    /// <summary>
    /// 资源背包
    /// </summary>
    public class ResourcesBag : MonoBehaviour {

        /// <summary>
        /// 包含那些资源
        /// </summary>
        private Dictionary<string, float> resources;

        private void Awake() {
            resources = new Dictionary<string, float>();
        }
        /// <summary>
        /// 增加资源
        /// </summary>
        public void AddResource(string resourceName , float value) {
            if (!resources.ContainsKey(resourceName)) {
                resources[resourceName] = 0;
            }
            resources[resourceName] += value;
        }
        /// <summary>
        /// 获取单个资源
        /// </summary>
        public float GetResource(string resourceName) {
            var val = 0f;
            resources.TryGetValue(resourceName ,out val);
            return val;
        }
        /// <summary>
        /// 获取所有资源
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, float> GetResources() {
            return resources;
        }

        /// <summary>
        /// 移除或者减少
        /// </summary>
        public void RemoveResource(string resourceName, float value) {
            resources[resourceName] -= value;
        }
    }
}