using System;
using System.Collections.Generic;
using UnityEngine;

// simple FSM, feel free to use this or your own or unity animator's behaviour or anything you like with ReGoap
namespace ReGoap.Unity.FSM
{
    /// <summary>
    /// 简易状态机，当然你也可以使用自己的状态机(Unity那个就算了，伤身体)来配合ReGoap
    /// </summary>
    public class StateMachine : MonoBehaviour
    {
        /// <summary>
        /// 包含的状态
        /// </summary>
        private Dictionary<Type, ISmState> states;

        /// <summary>
        /// FSM中的自定义数据
        /// </summary>
        private Dictionary<string, object> values;
        
        /// <summary>
        /// FSM中的全局自定义数据
        /// </summary>
        private static Dictionary<string, object> globalValues;

        /// <summary>
        /// 通用转换列表
        /// </summary>
        private List<ISmTransition> genericTransitions;

        /// <summary>
        /// 开启栈式状态
        /// </summary>
        public bool enableStackedStates;

        /// <summary>
        /// 当前状态集
        /// </summary>
        public Stack<ISmState> currentStates;

        /// <summary>
        /// 获取当前状态
        /// </summary>
        public ISmState CurrentState
        {
            get
            {
                if (enableStackedStates)
                    return currentStates.Count == 0 ? null : currentStates.Peek();
                return currentState;
            }
        }

        /// <summary>
        /// 当前状态
        /// </summary>
        private ISmState currentState;

        /// <summary>
        /// 初始状态
        /// </summary>
        public MonoBehaviour initialState;

        /// <summary>
        /// 允许循环切换
        /// </summary>
        public bool permitLoopTransition = true;

        /// <summary>
        /// 顺序切换
        /// </summary>
        public bool orderTransitions;

        void OnDisable()
        {
            if (CurrentState != null)
                CurrentState.Exit();
        }

        /// <summary>
        /// 初始化内容
        /// </summary>
        void Awake()
        {
            enabled = true;
            states = new Dictionary<Type, ISmState>();
            values = new Dictionary<string, object>();
            currentStates = new Stack<ISmState>();
            genericTransitions = new List<ISmTransition>();
            globalValues = new Dictionary<string, object>();
        }

        /// <summary>
        /// 自动添加状态
        /// </summary>
        void Start()
        {
            foreach (var state in GetComponents<ISmState>())
            {
                AddState(state);
                var monoB = (MonoBehaviour) state;
                monoB.enabled = false;
            }

            Switch(initialState.GetType());
        }

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="state"></param>
        public void AddState(ISmState state)
        {
            state.Init(this);
            states[state.GetType()] = state;
        }

        /// <summary>
        /// 添加通用切换
        /// </summary>
        /// <param name="func"></param>
        public void AddGenericTransition(ISmTransition func)
        {
            genericTransitions.Add(func);
            if (orderTransitions)
                genericTransitions.Sort();
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetValue<T>(string key)
        {
            if (!HasValue(key))
                return default(T);
            return (T) values[key];
        }

        /// <summary>
        /// 是否拥有某个值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasValue(string key)
        {
            return values.ContainsKey(key);
        }

        /// <summary>
        /// 设置某个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public void SetValue<T>(string key, T value)
        {
            values[key] = value;
        }

        /// <summary>
        /// 移除某个值
        /// </summary>
        /// <param name="key"></param>
        public void RemoveValue(string key)
        {
            values.Remove(key);
        }

        /// <summary>
        /// 获取某个全局值
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetGlobalValue<T>(string key)
        {
            return (T) globalValues[key];
        }

        /// <summary>
        /// 是否拥有某个全局值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasGlobalValue(string key)
        {
            return globalValues.ContainsKey(key);
        }

        /// <summary>
        /// 设置全局值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static void SetGlobalValue<T>(string key, T value)
        {
            globalValues[key] = value;
        }

        /// <summary>
        /// 定帧调用
        /// </summary>
        void FixedUpdate()
        {
            Check();
        }

        /// <summary>
        /// 检查，切换
        /// </summary>
        void Check()
        {
            for (var index = genericTransitions.Count - 1; index >= 0; index--)
            {
                var trans = genericTransitions[index];
                var result = trans.TransitionCheck(CurrentState);
                if (result != null)
                {
                    Switch(result);
                    return;
                }
            }

            if (CurrentState == null) return;
            for (var index = CurrentState.Transitions.Count - 1; index >= 0; index--)
            {
                var trans = CurrentState.Transitions[index];
                var result = trans.TransitionCheck(CurrentState);
                if (result != null)
                {
                    Switch(result);
                    return;
                }
            }
        }

        /// <summary>
        /// 切换到指定状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Switch<T>() where T : MonoBehaviour, ISmState
        {
            Switch(typeof(T));
        }

        /// <summary>
        /// 切换到指定状态
        /// </summary>
        /// <param name="T"></param>
        public void Switch(Type T)
        {
            if (CurrentState != null)
            {
                if (!permitLoopTransition && (CurrentState.GetType() == T)) return;
                ((MonoBehaviour) CurrentState).enabled = false;
                CurrentState.Exit();
            }

            if (enableStackedStates)
                currentStates.Push(states[T]);
            else
                currentState = states[T];
            ((MonoBehaviour) CurrentState).enabled = true;
            CurrentState.Enter();

            if (orderTransitions)
                CurrentState.Transitions.Sort();
        }

        /// <summary>
        /// 出栈顶层状态
        /// </summary>
        /// <exception cref="UnityException"></exception>
        public void PopState()
        {
            if (!enableStackedStates)
            {
                throw new UnityException(
                    "[StateMachine] Trying to pop a state from a state machine with disabled stacked states.");
            }

            currentStates.Peek().Exit();
            ((MonoBehaviour) currentStates.Pop()).enabled = false;
            ((MonoBehaviour) currentStates.Peek()).enabled = true;
            currentStates.Peek().Enter();
        }
    }
}