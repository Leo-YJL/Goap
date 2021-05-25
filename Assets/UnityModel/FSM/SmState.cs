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


        public void Enter() {
            
        }

        public void Exit() {

        }
        /// <summary>
        /// 获取优先级（越大越受青睐）
        /// </summary>
        public int GetPriority() {
            return priority;
        }

        public void Init(StateMachine stateMachine) {
       
        }

        public bool IsActive() {
            return enabled;
        }
        #endregion

    }
}