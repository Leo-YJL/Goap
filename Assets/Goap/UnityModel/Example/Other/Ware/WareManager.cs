using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    /// <summary>
    /// 仓库管理者
    /// </summary>
    public class WareManager : MonoBehaviour
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static WareManager Instance;


        public Dictionary<string, Ware> Wares;


        protected virtual void Awake()
        {
            if (Instance != null)
                throw new UnityException("[BankManager] Can have only one instance per scene.");
            Instance = this;

            var childResources = GetComponentsInChildren<Ware>();
            Wares = new Dictionary<string, Ware>(childResources.Length);
            foreach (var resource in childResources)
            {
                Wares[resource.GetName()] = resource;
            }
        }

        /// <summary>
        /// 获取当前银行
        /// </summary>
        /// <returns></returns>
        public Ware GetBank(string n)
        {
            return Wares[n];
        }

        /// <summary>
        /// 获取银行数目
        /// </summary>
        /// <returns></returns>
        public int GetBanksCount()
        {
            return Wares.Count;
        }
    }
}
