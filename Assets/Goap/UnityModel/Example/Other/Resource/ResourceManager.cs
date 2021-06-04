using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{ // one resourcemanager per type
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourceManager : MonoBehaviour, IResourceManager
    {
        /// <summary>
        /// 所有资源
        /// </summary>
        private List<IResource> resources;

        /// <summary>
        /// 当前资源索引值
        /// </summary>
        private int currentIndex;

        /// <summary>
        /// 资源名称
        /// </summary>
        public string ResourceName;

        #region UnityFunctions

        protected virtual void Awake()
        {
            currentIndex = 0;
            resources = new List<IResource>();
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
        }

        protected virtual void FixedUpdate()
        {
        }

        #endregion

        #region IResourceManager

        /// <summary>
        /// 获取资源名称
        /// </summary>
        /// <returns></returns>
        public virtual string GetResourceName()
        {
            return ResourceName;
        }

        /// <summary>
        /// 获取资源数目
        /// </summary>
        /// <returns></returns>
        public virtual int GetResourcesCount()
        {
            return resources.Count;
        }

        /// <summary>
        /// 获取所有资源
        /// </summary>
        /// <returns></returns>
        public virtual List<IResource> GetResources()
        {
            return resources;
        }

        /// <summary>
        /// 获取当前资源
        /// </summary>
        /// <returns></returns>
        public virtual IResource GetResource()
        {
            var result = resources[currentIndex];
            currentIndex = currentIndex++ % resources.Count;
            return result;
        }

        /// <summary>
        /// 增加资源
        /// </summary>
        /// <param name="resource"></param>
        public void AddResource(IResource resource)
        {
            resources.Add(resource);
        }

        #endregion
    }

    /// <summary>
    /// 资源管理器接口
    /// </summary>
    public interface IResourceManager
    {
        /// <summary>
        /// 获取资源名称
        /// </summary>
        /// <returns></returns>
        string GetResourceName();

        /// <summary>
        /// 获取资源数目
        /// </summary>
        /// <returns></returns>
        int GetResourcesCount();

        /// <summary>
        /// 获取所有资源
        /// </summary>
        /// <returns></returns>
        List<IResource> GetResources();

        // preferably should get a different transform every call
        /// <summary>
        /// 获取当前资源
        /// </summary>
        /// <returns></returns>
        IResource GetResource();

        /// <summary>
        /// 增加资源
        /// </summary>
        /// <param name="resource"></param>
        void AddResource(IResource resource);
    }
}