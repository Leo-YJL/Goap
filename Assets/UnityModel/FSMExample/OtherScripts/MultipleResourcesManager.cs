using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts {
    /// <summary>
    /// 资源管理器管理者（所有）
    /// </summary>
    public class MultipleResourcesManager : MonoBehaviour {
        /// <summary>
        /// 单例
        /// </summary>
        public static MultipleResourcesManager Instance;

        /// <summary>
        /// 所有资源管理者
        /// </summary>
        public Dictionary<string, IResourceManager> Resources;

    }
}