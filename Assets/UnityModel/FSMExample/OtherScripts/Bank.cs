
using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts {
    /// <summary>
    /// 银行
    /// </summary>
    public class Bank : MonoBehaviour {
        /// <summary>
        /// 资源背包
        /// </summary>
        private ResourcesBag bankBag;

        private void Awake() {
            bankBag = gameObject.AddComponent<ResourcesBag>();
        }
        /// <summary>
        /// 获取资源
        /// </summary>
        public float GetResource(string resourceName) {
            return bankBag.GetResource(resourceName);
        }
        /// <summary>
        /// 获取所有资源
        /// </summary>
        public Dictionary<string,float> GetResources() {
            return bankBag.GetResources();
        }
        /// <summary>
        /// 增加资源
        /// </summary>
        public bool AddResource(ResourcesBag resourcesBag,string resourceName,float value = 1f) {
            if (resourcesBag.GetResource(resourceName) >= value {
                resourcesBag.RemoveResource(resourceName, value);
                bankBag.AddResource(resourceName, value);
                return true;
            } else {
                return false;
            }
        }
    }
}