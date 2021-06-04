using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    /// <summary>
    /// 工作站。加工地点
    /// </summary>
    public class Workstation : MonoBehaviour
    {
        public string Name;

        public string GetName()
        {
            return Name;
        }
        public bool CraftResource(ResourcesBag crafterBag, IRecipe recipe, float value = 1f)
        {
           
            foreach (var pair in recipe.GetNeededResources())
            {
                if (crafterBag.GetResource(pair.Key) < pair.Value * value)
                {
                    return false;
                }
            }
            foreach (var pair in recipe.GetNeededResources())
            {
                crafterBag.RemoveResource(pair.Key, pair.Value * value);
            }
            var resource = recipe.GetCraftedResource();
            crafterBag.AddResource(resource, value);
            return true;
        }
    }
}