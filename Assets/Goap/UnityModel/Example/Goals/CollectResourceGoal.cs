using ReGoap.Core;

namespace ReGoap.Unity.FSMExample.Goals
{
    /// <summary>
    /// 收集资源Goal
    /// </summary>
    public class CollectResourceGoal : ReGoapGoal<string, object>
    {
        /// <summary>
        /// 资源名称
        /// </summary>
        public string ResourceName;

        protected override void Awake()
        {
            base.Awake();
          //  SetGoalName(ResourceName);
        }

        public void SetGoal(string _name) {
            ResourceName = _name;
            goal.Set("collectedResource" + ResourceName, true);
            goal.Set("reconcilePosition", true);
        }

        public override string ToString()
        {
            return string.Format("GoapGoal('{0}', '{1}')", Name, ResourceName);
        }
    }
}

