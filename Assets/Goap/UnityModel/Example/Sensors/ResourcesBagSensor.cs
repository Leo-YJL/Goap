using ReGoap.Unity.FSMExample.OtherScripts;

namespace ReGoap.Unity.FSMExample.Sensors
{
    /// <summary>
    /// 资源背包感知者
    /// </summary>
    public class ResourcesBagSensor : ReGoapSensor<string, object>
    {
        /// <summary>
        /// 资源背包
        /// </summary>
        private ResourcesBag resourcesBag;

        void Awake()
        {
            resourcesBag = GetComponent<ResourcesBag>();
        }

        /// <summary>
        /// 更新背包状态
        /// </summary>
        public override void UpdateSensor()
        {
            var state = memory.GetWorldState();
            foreach (var pair in resourcesBag.GetResources())
            {
                state.Set("hasResource" + pair.Key, pair.Value > 0);
            }
        }
    }
}
