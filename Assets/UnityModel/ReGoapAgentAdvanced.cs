using ReGoap.Core;

namespace ReGoap.Unity {
    /// <summary>
    /// 高级版AI代理（自动进行Goal更新，不让他闲着）
    /// </summary>
    public class ReGoapAgentAdvanced<T, W> : ReGoapAgent<T, W> {
        #region
        protected virtual void Update() {
            possibleGoalsDirty = true;
            if (currentActionState == null) {
                if (!IsPlanning)
                    CalculateNewGoal();
                return;
            }
        }
        #endregion
    }
}
