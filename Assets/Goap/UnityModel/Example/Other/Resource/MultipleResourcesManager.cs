using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    /// <summary>
    /// ��Դ�����������ߣ����У�
    /// </summary>
    public class MultipleResourcesManager : MonoBehaviour
    {
        /// <summary>
        /// ����
        /// </summary>
        public static MultipleResourcesManager Instance;

        /// <summary>
        /// ������Դ������
        /// </summary>
        public Dictionary<string, IResourceManager> Resources;

        /// <summary>
        /// ��ʼ��������Դ�����ߣ���������װ����Դ
        /// </summary>
        /// <exception cref="UnityException"></exception>
        void Awake()
        {
            if (Instance != null)
                throw new UnityException("[ResourcesManager] Can have only one instance per scene.");
            Instance = this;
            var childResources = GetComponentsInChildren<IResource>();
            Resources = new Dictionary<string, IResourceManager>(childResources.Length);
            foreach (var resource in childResources)
            {
                if (!Resources.ContainsKey(resource.GetName()))
                {
                    var manager = gameObject.AddComponent<ResourceManager>();
                    manager.ResourceName = resource.GetName();
                    Resources[resource.GetName()] = manager;
                }
                Resources[resource.GetName()].AddResource(resource);
            }
        }
    }
}
