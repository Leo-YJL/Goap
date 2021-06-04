using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    /// <summary>
    /// AI代理生产者，用于实例化AI
    /// </summary>
    public class AgentsSpawner : MonoBehaviour
    {
        /// <summary>
        /// 要实例化的AI数量
        /// </summary>
        public int BuildersCount;

        /// <summary>
        /// 已实例化的AI数量
        /// </summary>
        private int spawnedBuilders;

        /// <summary>
        /// AI的Prefab引用
        /// </summary>
        public GameObject BuilderPrefab;

        /// <summary>
        /// 两次创建AI的冷却时间
        /// </summary>
        public float DelayBetweenSpawns = 0.1f;

        /// <summary>
        /// 每次创建AI的数量
        /// </summary>
        public int AgentsPerSpawn = 100;

        /// <summary>
        /// 下一次可创建AI的时间点
        /// </summary>
        private float spawnCooldown;

        void Awake()
        {
        }

        void Update()
        {
            if (Time.time >= spawnCooldown && spawnedBuilders < BuildersCount)
            {
                spawnCooldown = Time.time + DelayBetweenSpawns;
                for (int i = 0; i < AgentsPerSpawn && spawnedBuilders < BuildersCount; i++)
                {
                    var gameObj = Instantiate(BuilderPrefab);
                    gameObj.SetActive(true);
                    gameObj.transform.SetParent(transform);

                    spawnedBuilders++;
                }
            }
        }
    }
}