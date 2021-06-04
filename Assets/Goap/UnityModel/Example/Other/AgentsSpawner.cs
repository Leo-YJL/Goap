using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    /// <summary>
    /// AI���������ߣ�����ʵ����AI
    /// </summary>
    public class AgentsSpawner : MonoBehaviour
    {
        /// <summary>
        /// Ҫʵ������AI����
        /// </summary>
        public int BuildersCount;

        /// <summary>
        /// ��ʵ������AI����
        /// </summary>
        private int spawnedBuilders;

        /// <summary>
        /// AI��Prefab����
        /// </summary>
        public GameObject BuilderPrefab;

        /// <summary>
        /// ���δ���AI����ȴʱ��
        /// </summary>
        public float DelayBetweenSpawns = 0.1f;

        /// <summary>
        /// ÿ�δ���AI������
        /// </summary>
        public int AgentsPerSpawn = 100;

        /// <summary>
        /// ��һ�οɴ���AI��ʱ���
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