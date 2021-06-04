using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    /// <summary>
    /// 银行
    /// </summary>
    public class Ware : MonoBehaviour
    {
        /// <summary>
        /// 资源背包
        /// </summary>
        private ResourcesBag bankBag;

        public string Name;

        public Text Txt;
        void Awake()
        {
            bankBag = gameObject.AddComponent<ResourcesBag>();
            Txt.text = "0";
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public float GetResource(string resourceName)
        {
            return bankBag.GetResource(resourceName);
        }

        /// <summary>
        /// 获取所有资源
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, float> GetResources()
        {
            return bankBag.GetResources();
        }

        /// <summary>
        /// 增加资源
        /// </summary>
        /// <param name="resourcesBag"></param>
        /// <param name="resourceName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddResource(ResourcesBag resourcesBag, string resourceName, float value = 1f)
        {
            if (resourcesBag.GetResource(resourceName) >= value)
            {
                resourcesBag.RemoveResource(resourceName, value);
                bankBag.AddResource(resourceName, value);
                Txt.text = bankBag.GetResource(resourceName).ToString();
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetName()
        {
            return Name;
        }
    }
}
