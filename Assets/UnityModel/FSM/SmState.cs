using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ReGoap.Unity.FSM {
    public class SmState : MonoBehaviour, ISmState {

        /// <summary>
        /// 所包含的切换
        /// </summary>
        public List<ISmTransition> Transitions { get; set; }

        /// <summary>
        /// 自身优先级（越大越受青睐）
        /// </summary>
        public int priority;

        #region UnityFunctions

        protected virtual void Awake() {
            Transitions = new List<ISmTransition>();
        }

        protected virtual void Start() {
        }

        protected virtual void FixedUpdate() {
        }

        protected virtual void Update() {
        }

        #endregion


        #region ISmState


        public virtual void Enter() {
            
        }

        public virtual void Exit() {

        }
        /// <summary>
        /// 获取优先级（越大越受青睐）
        /// </summary>
        public virtual int GetPriority() {
            return priority;
        }

        public virtual void Init(StateMachine stateMachine) {
       
        }

        public virtual bool IsActive() {
            return enabled;
        }
        #endregion

    }
}