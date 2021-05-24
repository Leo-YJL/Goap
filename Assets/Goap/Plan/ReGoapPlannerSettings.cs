using System;

namespace ReGoap.Planner {
    /// <summary>
    /// Plan的设置
    /// </summary>
    [Serializable]
    public class ReGoapPlannerSettings {
        /// <summary>
        /// 是否允许提前跳出
        /// </summary>
        public bool PlanningEarlyExit = false;
        /// <summary>
        /// 最大迭代次数
        /// </summary>
        public int MaxIterations = 1000;
        /// <summary>
        /// 最大拓扑节点数
        /// </summary>
        public int MaxNodesToExpand = 10000;
        /// <summary>
        /// 是否使用动态Actions
        /// 动态Action代表其现居条件和效果都是可以动态变化的
        /// 参考GenericGoTo或GatherResourceAction
        /// </summary>
        public bool UsingDynamicActions = false;
        /// <summary>
        /// 是否启用规划的Debug工具，（这是本地的AI规划可视化，使用起来比较麻烦，推荐使用官方提供的Unity中的可视化Debug工具）
        /// </summary>
        public bool DebugPlan = false;
    }
}