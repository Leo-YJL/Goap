using ReGoap.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ReGoap.Unity {
    public class ReGoapPlannerManager : MonoBehaviour {
        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }

    /// <summary>
    /// Plan工作内容类（抽象了一个AI代理及其相关的Goal，Action等内容）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public struct ReGoapPlanWork<T, W> {
        /// <summary>
        /// AI代理
        /// </summary>
        public readonly IReGoapAgent<T, W> Agent;
        /// <summary>
        /// 黑名单Goal
        /// </summary>
        public readonly IReGoapGoal<T, W> BlacklistGoal;
        /// <summary>
        /// Action队列
        /// </summary>
        public readonly Queue<ReGoapActionState<T, W>> Actions;
        /// <summary>
        /// 完成后的回调
        /// </summary>
        public readonly Action<IReGoapGoal<T, W>> CallBack;
        /// <summary>
        /// 新的Goal
        /// </summary>
        public IReGoapGoal<T, W> NewGoal;

        public ReGoapPlanWork(IReGoapAgent<T,W> agent,IReGoapGoal<T,W> blacklistGoal,Queue<ReGoapActionState<T,W>> actions,
            Action<IReGoapGoal<T,W>> callback) : this() {

            Agent = agent;
            BlacklistGoal = blacklistGoal;
            Actions = actions;
            CallBack = callback;
        }

    }
}