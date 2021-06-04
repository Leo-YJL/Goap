using ReGoap.Core;

namespace ReGoap.Unity
{
    /// <summary>
    /// 高级版AI代理（自动进行Goal更新，不让他闲着）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ReGoapAgentAdvanced<T, W> : ReGoapAgent<T, W>
    {
        #region UnityFunctions
        protected virtual void Update()
        {
            possibleGoalsDirty = true;

            if (currentActionState == null)
            {
                if (!IsPlanning)
                    CalculateNewGoal();
                return;
            }
        }
        #endregion
    }
}