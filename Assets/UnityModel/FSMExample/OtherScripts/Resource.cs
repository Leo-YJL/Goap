using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace ReGoap.Unity.FSMExample.OtherScripts {
    public class Resource : MonoBehaviour , IResource {
        /// <summary>
        /// 资源名称
        /// </summary>
        public string ResourceName;
        /// <summary>
        /// 容量
        /// </summary>
        public float Capacity = 1f;
        /// <summary>
        /// 起始容量
        /// </summary>
        protected float startingCapacity;
        /// <summary>
        /// 存储列表
        /// </summary>
        protected HashSet<int> reservationList;

        protected virtual void Awake() {

            startingCapacity = Capacity;
            reservationList = new HashSet<int>();
        }

        /// <summary>
        /// 获取容量
        /// </summary>
        public float GetCapacity() {
            return Capacity;
        }
        /// <summary>
        /// 获取资源名称
        /// </summary>
        public string GetName() {
            return ResourceName;
        }
        /// <summary>
        /// 获取存储数量
        /// </summary>
        public int GetReserveCount() {
            return reservationList.Count;
        }
        /// <summary>
        /// 获取Transform
        /// </summary>
        public Transform GetTransform() {
            return transform;
        }
        /// <summary>
        /// 移除资源
        /// </summary>
        public void RemoveResource(float value) {
            if (Capacity > value)
                Capacity -= value;
            else
                Capacity = 0.0f;
        }
        /// <summary>
        /// 存储
        /// </summary>
        public void Reserve(int id) {
            reservationList.Add(id);
        }
        /// <summary>
        /// 移除
        /// </summary>
        public void Unreserve(int id) {
            reservationList.Remove(id);
        }
    }

    public interface IResource {
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