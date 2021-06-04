using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ReGoap.Unity.FSMExample.OtherScripts
{ // craftable items as well primitive resources
    /// <summary>
    /// 配方里提到的资源，同时也是原始资源（资源基类）
    /// </summary>
    public class Resource : MonoBehaviour, IResource
    {
        /// <summary>
        /// 资源名称
        /// </summary>
        public string ResourceName;

        /// <summary>
        /// 容量
        /// </summary>
        public float Capacity = 1f;
        
        /// <summary>
        /// 容量显示
        /// </summary>
        public Text CapcityText;

        /// <summary>
        /// 起始容量
        /// </summary>
        protected float startingCapacity;

        /// <summary>
        /// 存储列表
        /// </summary>
        protected HashSet<int> reservationList;


        protected virtual void Awake()
        {
            startingCapacity = Capacity;
            reservationList = new HashSet<int>();

            SetText();
        }

        /// <summary>
        /// 获取资源名称
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return ResourceName;
        }

        /// <summary>
        /// 获取Transform
        /// </summary>
        /// <returns></returns>
        public virtual Transform GetTransform()
        {
            return transform;
        }

        /// <summary>
        /// 获取容量
        /// </summary>
        /// <returns></returns>
        public virtual float GetCapacity()
        {
            return Capacity;
        }

        /// <summary>
        /// 移除资源
        /// </summary>
        /// <param name="value"></param>
        public virtual void RemoveResource(float value)
        {
            if (Capacity > value)
                Capacity -= value;
            else
                Capacity = 0.0f;
            SetText();
        }

        /// <summary>
        /// 存储
        /// </summary>
        /// <param name="id"></param>
        public virtual void Reserve(int id)
        {
            reservationList.Add(id);
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="id"></param>
        public virtual void Unreserve(int id)
        {
            reservationList.Remove(id);
        }

        /// <summary>
        /// 获取存储数量
        /// </summary>
        /// <returns></returns>
        public virtual int GetReserveCount()
        {
            return reservationList.Count;
        }

        protected void SetText()
        {
            if (CapcityText != null)
            {
                CapcityText.text = Capacity.ToString();
            }
        }
    }

    /// <summary>
    /// 资源接口
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// 获取资源名称
        /// </summary>
        /// <returns></returns>
        string GetName();

        /// <summary>
        /// 获取Transform组件
        /// </summary>
        /// <returns></returns>
        Transform GetTransform();

        /// <summary>
        /// 获取容量
        /// </summary>
        /// <returns></returns>
        float GetCapacity();

        /// <summary>
        /// 移除资源（指定大小）
        /// </summary>
        /// <param name="value"></param>
        void RemoveResource(float value);

        /// <summary>
        /// 存储
        /// </summary>
        /// <param name="id"></param>
        void Reserve(int id);

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="id"></param>
        void Unreserve(int id);

        /// <summary>
        /// 获取存储数量
        /// </summary>
        /// <returns></returns>
        int GetReserveCount();
    }
}