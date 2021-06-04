using UnityEngine;
using System.Collections.Generic;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    /// <summary>
    /// 工作站管理者
    /// </summary>
    public class WorkstationsManager : MonoBehaviour
    {
        public static WorkstationsManager Instance;
     
        public Dictionary<string, Workstation> Workstations;

        protected virtual void Awake()
        {
            if (Instance != null)
                throw new UnityException("[WorkstationsManager] Can have only one instance per scene.");
            Instance = this;

            var childResources = GetComponentsInChildren<Workstation>();
            Workstations = new Dictionary<string, Workstation>(childResources.Length);
            foreach (var resource in childResources)
            {
                Workstations[resource.GetName()] = resource;
            }
        }

        public Workstation GetWorkstation(string resourceName)
        {
            return Workstations[resourceName];
        }
    }
}
