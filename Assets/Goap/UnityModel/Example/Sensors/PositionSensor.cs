using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.OtherScripts;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Sensors
{
    /// <summary>
    /// 地点感知者
    /// </summary>
    public class PositionSensor : ReGoapSensor<string, object>
    {
        /// <summary>
        /// 初始化位置
        /// </summary>
        /// <param name="memory"></param>
        public override void Init(IReGoapMemory<string, object> memory)
        {
            base.Init(memory);
            var state = memory.GetWorldState();
            state.Set("startPosition", transform.position);
        }

        /// <summary>
        /// 更新位置
        /// </summary>
        public override void UpdateSensor()
        {
            var state = memory.GetWorldState();
            state.Set("startPosition", transform.position);
        }
    }
}