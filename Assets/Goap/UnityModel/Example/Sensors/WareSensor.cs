using System.Collections.Generic;
using ReGoap.Unity.FSMExample.OtherScripts;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Sensors
{
    /// <summary>
    /// 银行感知者
    /// </summary>
    public class WareSensor : ReGoapSensor<string, object>
    {
        /// <summary>
        /// 感知到的所有银行
        /// </summary>
        private Dictionary<Ware, Vector3> banks;

        public float MinPowDistanceToBeNear = 1f;

        /// <summary>
        /// 进行memory初始化
        /// </summary>
        void Start()
        {
            int count = WareManager.Instance.GetBanksCount();
            banks = new Dictionary<Ware, Vector3>(count);
            foreach (var bank in WareManager.Instance.Wares)
            {
                banks[bank.Value] = bank.Value.transform.position;
            }

            var worldState = memory.GetWorldState();
            worldState.Set("seeWare", WareManager.Instance != null && count > 0);
            worldState.Set("wares", banks);
        }
    }
}