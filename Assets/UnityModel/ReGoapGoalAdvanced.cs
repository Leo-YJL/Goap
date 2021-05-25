using UnityEngine;

// generic goal, should inherit this to do your own goal
namespace ReGoap.Unity {
    /// <summary>
    /// 高级版Goal自动进行Goal更替
    /// </summary>
    public class ReGoapGoalAdvanced<T, W> : ReGoapGoal<T, W> {
        public float WarnDelay = 2f;
        private float warnCooldown;

        #region UnityFunctions
        protected virtual void Update() {
            if (planner != null && !planner.IsPlanning() && Time.time > warnCooldown) {
                warnCooldown = Time.time + WarnDelay;
                var currentGoal = planner.GetCurrentGoal();
                var plannerPlan = currentGoal == null ? null : currentGoal.GetPlan();
                var equalsPlan = ReferenceEquals(plannerPlan, plan);
                var isGoalPossible = IsGoalPossible();
                // check if this goal is not active but CAN be activated
                //  or
                // if this goal is active but isn't anymore possible
                if ((!equalsPlan && isGoalPossible) || (equalsPlan && !isGoalPossible))
                    planner.GetCurrentAgent().WarnPossibleGoal(this);
            }
        }
        #endregion
    }
}