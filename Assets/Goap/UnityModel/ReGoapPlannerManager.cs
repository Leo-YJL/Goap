using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ReGoap.Core;
using ReGoap.Planner;
using ReGoap.Utilities;
using UnityEngine;

namespace ReGoap.Unity
{
    // every thread runs on one of these classes
    /// <summary>
    /// Planner线程，每个线程都运行着其中一个这样的类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapPlannerThread<T, W>
    {
        /// <summary>
        /// Planner
        /// </summary>
        private readonly ReGoapPlanner<T, W> planner;

        /// <summary>
        /// Plan工作内容队列
        /// </summary>
        public static ConcurrentQueue<ReGoapPlanWork<T, W>> WorksQueue;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        private bool isRunning = true;

        /// <summary>
        /// 当完成Plan时会执行回调
        /// </summary>
        private readonly Action<ReGoapPlannerThread<T, W>, ReGoapPlanWork<T, W>, IReGoapGoal<T, W>> onDonePlan;

        public ReGoapPlannerThread(ReGoapPlannerSettings plannerSettings,
            Action<ReGoapPlannerThread<T, W>, ReGoapPlanWork<T, W>, IReGoapGoal<T, W>> onDonePlan)
        {
            planner = new ReGoapPlanner<T, W>(plannerSettings);
            this.onDonePlan = onDonePlan;
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            isRunning = false;
        }

        /// <summary>
        /// 主循环（当前线程是否处于可运行状态，每次执行完毕立即发起一次线程竞争）
        /// </summary>
        public void MainLoop()
        {
            while (isRunning)
            {
                CheckWorkers();
                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// 检查工作内容（就是正式开始工作）
        /// </summary>
        public void CheckWorkers()
        {
            if (WorksQueue.TryDequeue(out ReGoapPlanWork<T, W> checkWork))
            {
                var work = checkWork;
                planner.Plan(work.Agent, work.BlacklistGoal, work.Actions,
                    (newGoal) => onDonePlan(this, work, newGoal));
            }
        }
    }

    // behaviour that should be added once (and only once) to a gameobject in your unity's scene
    /// <summary>
    /// GOAP Plan管理者，总管，需要添加到一个Go上来驱动执行整个系统
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapPlannerManager<T, W> : MonoBehaviour
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static ReGoapPlannerManager<T, W> Instance;

        /// <summary>
        /// 是否开启多线程计算
        /// </summary>
        public bool MultiThread;

        [Header("Used only if MultiThread is set to true.")] [Range(1, 128)]
        public int ThreadsCount = 1;

        /// <summary>
        /// 所有的Planner
        /// </summary>
        private ReGoapPlannerThread<T, W>[] planners;

        /// <summary>
        /// 所有已完成工作
        /// </summary>
        private List<ReGoapPlanWork<T, W>> doneWorks;

        /// <summary>
        /// 所有工作线程
        /// </summary>
        private Thread[] threads;

        /// <summary>
        /// Planner设置
        /// </summary>
        public ReGoapPlannerSettings PlannerSettings;

        /// <summary>
        /// Debug等级
        /// </summary>
        public ReGoapLogger.DebugLevel LogLevel = ReGoapLogger.DebugLevel.Full;

        /// <summary>
        /// 结点预热个数
        /// </summary>
        public int NodeWarmupCount = 1000;

        /// <summary>
        /// 状态预热个数
        /// </summary>
        public int StatesWarmupCount = 10000;

        #region UnityFunctions

        protected virtual void Awake()
        {
            //设置线程同步上下文
            SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);
            //预热
            ReGoapNode<T, W>.Warmup(NodeWarmupCount);
            ReGoapState<T, W>.Warmup(StatesWarmupCount);

            ReGoapLogger.Level = LogLevel;
            Debug.Log($"Log等级为{ReGoapLogger.Level}");
            if (Instance != null)
            {
                Destroy(this);
                var errorString =
                    "[GoapPlannerManager] Trying to instantiate a new manager but there can be only one per scene.";
                ReGoapLogger.LogError(errorString);
                throw new UnityException(errorString);
            }

            Instance = this;

            //初始化工作
            doneWorks = new List<ReGoapPlanWork<T, W>>();
            ReGoapPlannerThread<T, W>.WorksQueue = new ConcurrentQueue<ReGoapPlanWork<T, W>>();
            planners = new ReGoapPlannerThread<T, W>[ThreadsCount];
            threads = new Thread[ThreadsCount];

            //如果是多线程
            if (MultiThread)
            {
                //多个Planner
                ReGoapLogger.Log(String.Format("[GoapPlannerManager] Running in multi-thread mode ({0} threads).",
                    ThreadsCount));
                for (int i = 0; i < ThreadsCount; i++)
                {
                    planners[i] = new ReGoapPlannerThread<T, W>(PlannerSettings, OnDonePlan);
                    var thread = new Thread(planners[i].MainLoop);
                    thread.Start();
                    threads[i] = thread;
                }
            } // no threads run
            else
            {
                //单个Planner
                ReGoapLogger.Log("[GoapPlannerManager] Running in single-thread mode.");
                planners[0] = new ReGoapPlannerThread<T, W>(PlannerSettings, OnDonePlan);
            }
        }

        protected virtual void Start()
        {
        }

        void OnDestroy()
        {
            foreach (var planner in planners)
            {
                if (planner != null)
                    planner.Stop();
            }

            // should wait here?
            foreach (var thread in threads)
            {
                if (thread != null)
                    thread.Abort();
            }
        }

        /// <summary>
        /// 每帧更新，区分多线程和多线程
        /// </summary>
        protected virtual void Update()
        {
            OneThreadSynchronizationContext.Instance.Update();
            ReGoapLogger.Level = LogLevel;
            if (doneWorks.Count > 0)
            {
                lock (doneWorks)
                {
                    foreach (var work in doneWorks)
                    {
                        work.Callback(work.NewGoal);
                    }

                    doneWorks.Clear();
                }
            }

            if (!MultiThread)
            {
                planners[0].CheckWorkers();
            }
        }

        #endregion

        // called in another thread
        /// <summary>
        /// 一个Plan完成时的回调
        /// </summary>
        /// <param name="plannerThread"></param>
        /// <param name="work"></param>
        /// <param name="newGoal"></param>
        private void OnDonePlan(ReGoapPlannerThread<T, W> plannerThread, ReGoapPlanWork<T, W> work,
            IReGoapGoal<T, W> newGoal)
        {
            work.NewGoal = newGoal;
            lock (doneWorks)
            {
                doneWorks.Add(work);
            }

            if (work.NewGoal != null && ReGoapLogger.Level == ReGoapLogger.DebugLevel.Full)
            {
                ReGoapLogger.Log("[GoapPlannerManager] Done calculating plan, actions list:");
                var i = 0;
                foreach (var action in work.NewGoal.GetPlan())
                {
                    ReGoapLogger.Log(string.Format("{0}: {1}", i++, action.Action));
                }
            }
        }

        /// <summary>
        /// 开始进行规划
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="blacklistGoal"></param>
        /// <param name="currentPlan"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ReGoapPlanWork<T, W> Plan(IReGoapAgent<T, W> agent, IReGoapGoal<T, W> blacklistGoal,
            Queue<ReGoapActionState<T, W>> currentPlan, Action<IReGoapGoal<T, W>> callback)
        {
            var work = new ReGoapPlanWork<T, W>(agent, blacklistGoal, currentPlan, callback);
            lock (ReGoapPlannerThread<T, W>.WorksQueue)
            {
                ReGoapPlannerThread<T, W>.WorksQueue.Enqueue(work);
            }

            return work;
        }
    }

    /// <summary>
    /// Plan工作内容类（抽象了一个AI代理及其相关Goal，Action等内容）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public struct ReGoapPlanWork<T, W>
    {
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
        public readonly Action<IReGoapGoal<T, W>> Callback;

        /// <summary>
        /// 新的Goal
        /// </summary>
        public IReGoapGoal<T, W> NewGoal;

        public ReGoapPlanWork(IReGoapAgent<T, W> agent, IReGoapGoal<T, W> blacklistGoal,
            Queue<ReGoapActionState<T, W>> actions, Action<IReGoapGoal<T, W>> callback) : this()
        {
            Agent = agent;
            BlacklistGoal = blacklistGoal;
            Actions = actions;
            Callback = callback;
        }
    }
}