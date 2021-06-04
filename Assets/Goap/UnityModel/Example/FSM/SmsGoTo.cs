using System;
using ReGoap.Unity.FSM;
using ReGoap.Utilities;
using UnityEngine;

// generic goto state, can be used in most games, override Tick and Enter if you are using 
//  a navmesh / pathfinding library 
//  (ex. tell the library to search a path in Enter, when done move to the next waypoint in Tick)
namespace ReGoap.Unity.FSMExample.FSM
{
    /// <summary>
    /// 走向某个目的地状态
    /// </summary>
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(SmsIdle))]
    public class SmsGoTo : SmState
    {
        /// <summary>
        /// 目的地
        /// </summary>
        private Vector3? objective;

        /// <summary>
        /// 目的地Transform
        /// </summary>
        private Transform objectiveTransform;

        /// <summary>
        /// 当完成移动时回调
        /// </summary>
        private Action onDoneMovementCallback;

        /// <summary>
        /// 当移动失败时回调
        /// </summary>
        private Action onFailureMovementCallback;

        /// <summary>
        /// 自身状态
        /// </summary>
        private enum GoToState
        {
            /// <summary>
            /// 不可用
            /// </summary>
            Disabled,

            /// <summary>
            /// 就绪
            /// </summary>
            Pulsed,

            /// <summary>
            /// 激活态
            /// </summary>
            Active,

            /// <summary>
            /// 成功
            /// </summary>
            Success,

            /// <summary>
            /// 失败
            /// </summary>
            Failure
        }

        /// <summary>
        /// 当前状态
        /// </summary>
        private GoToState currentState;

        /// <summary>
        /// 刚体
        /// </summary>
        private Rigidbody body;

        /// <summary>
        /// 在FixedUpdate中调用
        /// </summary>
        public bool WorkInFixedUpdate;

        /// <summary>
        /// 使用刚体
        /// </summary>
        public bool UseRigidBody;

        /// <summary>
        /// 使用刚体速度
        /// </summary>
        public bool UseRigidbodyVelocity;

        /// <summary>
        /// 速度
        /// </summary>
        public float Speed;

        // when the magnitude of the difference between the objective and self is <= of this then we're done
        /// <summary>
        /// 当前点与目标点具体小于多少时认为完成
        /// </summary>
        public float MinPowDistanceToObjective = 0.5f;

        // additional feature, check for stuck, userful when using rigidbody or raycasts for movements
        /// <summary>
        /// 附加功能，使用刚体或射线进行运动时检查是否卡住，有用
        /// </summary>
        private Vector3 lastStuckCheckUpdatePosition;
        /// <summary>
        /// 下一次检查卡住的时间点
        /// </summary>
        private float stuckCheckCooldown;
        /// <summary>
        /// 检查是否卡住
        /// </summary>
        public bool CheckForStuck;
        /// <summary>
        /// 每次检测时间间隔
        /// </summary>
        public float StuckCheckDelay = 1f;
        /// <summary>
        /// 最大卡住距离
        /// </summary>
        public float MaxStuckDistance = 0.1f;

        protected override void Awake()
        {
            base.Awake();
            if (UseRigidBody)
            {
                body = GetComponent<Rigidbody>();
            }
        }

        // if your games handle the speed from something else (ex. stats class) you can override this function
        /// <summary>
        /// 获取速度（可重写）
        /// </summary>
        /// <returns></returns>
        protected virtual float GetSpeed()
        {
            return Speed;
        }

        #region Work

        
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!WorkInFixedUpdate) return;
            Tick();
        }

        protected override void Update()
        {
            base.Update();
            if (WorkInFixedUpdate) return;
            Tick();
        }

        // if you're using an animation just override this, call base function (base.Tick()) and then 
        //  set the animator variables (if you want to use root motion then also override MoveTo)
        /// <summary>
        /// 进行移动
        /// </summary>
        protected virtual void Tick()
        {
            var objectivePosition =
                objectiveTransform != null ? objectiveTransform.position : objective.GetValueOrDefault();
            MoveTo(objectivePosition);
        }

        /// <summary>
        /// 移动到目标点
        /// </summary>
        /// <param name="position"></param>
        protected virtual void MoveTo(Vector3 position)
        {
            var delta = position - transform.position;
            var movement = delta.normalized * GetSpeed();
            if (UseRigidBody)
            {
                if (UseRigidbodyVelocity)
                {
                    body.velocity = movement;
                }
                else
                {
                    body.MovePosition(transform.position + movement * Time.deltaTime);
                }
            }
            else
            {
                transform.position += movement * Time.deltaTime;
            }

            if (delta.sqrMagnitude <= MinPowDistanceToObjective)
            {
                currentState = GoToState.Success;
            }

            //看是否检查卡住
            if (CheckForStuck && CheckIfStuck())
            {
                currentState = GoToState.Failure;
            }
        }

        /// <summary>
        /// 检查是否卡住，如果前一次位置与当前位置距离小于MaxStuckDistance即为卡住
        /// </summary>
        /// <returns></returns>
        private bool CheckIfStuck()
        {
            if (Time.time > stuckCheckCooldown)
            {
                stuckCheckCooldown = Time.time + StuckCheckDelay;
                if ((lastStuckCheckUpdatePosition - transform.position).magnitude < MaxStuckDistance)
                {
                    ReGoapLogger.Log("[SmsGoTo] '" + name + "' is stuck.");
                    return true;
                }

                lastStuckCheckUpdatePosition = transform.position;
            }

            return false;
        }

        #endregion

        #region StateHandler

        public override void Init(StateMachine stateMachine)
        {
            base.Init(stateMachine);
            var transition = new SmTransition(GetPriority(), Transition);
            var doneTransition = new SmTransition(GetPriority(), DoneTransition);
            stateMachine.GetComponent<SmsIdle>().Transitions.Add(transition);
            Transitions.Add(doneTransition);
        }

        private Type DoneTransition(ISmState state)
        {
            if (currentState != GoToState.Active)
                return typeof(SmsIdle);
            return null;
        }

        private Type Transition(ISmState state)
        {
            if (currentState == GoToState.Pulsed)
                return typeof(SmsGoTo);
            return null;
        }

        public void GoTo(Vector3? position, Action onDoneMovement, Action onFailureMovement)
        {
            objective = position;
            GoTo(onDoneMovement, onFailureMovement);
        }

        public void GoTo(Transform transform, Action onDoneMovement, Action onFailureMovement)
        {
            objectiveTransform = transform;
            GoTo(onDoneMovement, onFailureMovement);
        }

        void GoTo(Action onDoneMovement, Action onFailureMovement)
        {
            currentState = GoToState.Pulsed;
            onDoneMovementCallback = onDoneMovement;
            onFailureMovementCallback = onFailureMovement;
        }

        public override void Enter()
        {
            base.Enter();
            currentState = GoToState.Active;
        }

        public override void Exit()
        {
            base.Exit();
            if (currentState == GoToState.Success)
                onDoneMovementCallback();
            else
                onFailureMovementCallback();
        }

        #endregion
    }
}