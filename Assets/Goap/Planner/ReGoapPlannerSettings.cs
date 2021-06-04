using System;

namespace ReGoap.Planner
{
    /// <summary>
    /// Plan的设置
    /// </summary>
    [Serializable]
    public class ReGoapPlannerSettings
    {
        /// <summary>
        /// 是否允许提前跳出
        /// </summary>
        public bool PlanningEarlyExit = false;

        /// <summary>
        /// increase both if your agent has a lot of actions
        /// 最大迭代次数
        /// </summary>
        public int MaxIterations = 1000;
        /// <summary>
        /// 最大拓扑结点数
        /// </summary>
        public int MaxNodesToExpand = 10000;

        /// <summary>
        /// set this to true if using dynamic actions, such as GenericGoTo or GatherResourceAction
        /// a dynamic action is an action that has dynamic preconditions or effects (changed in runtime/precalcultions)
        /// 是否使用动态Action
        /// 动态Action代表其现居条件和效果都是可以动态变化的
        /// 参考GenericGoTo或GatherResourceAction
        /// </summary>
        public bool UsingDynamicActions = false;

        /// <summary>
        /// 是否启用规划的Debug工具输出（这是本地的AI规划可视化，使用起来比较麻烦，推荐使用官方提供的Unity中的可视化Debug工具）
        /// </summary>
        public bool DebugPlan = false;
    }
}