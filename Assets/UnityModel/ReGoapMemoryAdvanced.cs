using ReGoap.Core;
using UnityEngine;

namespace ReGoap.Unity {
    /// <summary>
    /// 进阶版记忆类，可更新感知器
    /// </summary>
    public class ReGoapMemoryAdvanced<T, W> : ReGoapMemory<T, W> {
        /// <summary>
        /// 感知器
        /// </summary>
        private IReGoapSensor<T, W>[] sensors;
        /// <summary>
        /// 感知器更新间隔
        /// </summary>
        public float SensorsUpdateDelay = 0.3f;
        /// <summary>
        /// 感知器下一次可更新时间点
        /// </summary>
        private float sensorsUpdateCooldown;


        #region UnityFunctions

        protected override void Awake() {
            base.Awake();
            sensors = GetComponents<IReGoapSensor<T, W>>();
            foreach (var sensor in sensors) {
                sensor.Init(this);
            }
        }

        protected virtual void Update() {
            if (Time.time > sensorsUpdateCooldown) {
                sensorsUpdateCooldown = Time.time + SensorsUpdateDelay;

                foreach (var sensor in sensors) {
                    sensor.UpdateSensor();
                }
            }
        }

        #endregion
    }
}