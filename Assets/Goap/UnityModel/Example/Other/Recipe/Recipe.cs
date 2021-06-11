using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    /// <summary>
    /// 配方
    /// </summary>
    [CreateAssetMenu(fileName = "Recipe", menuName = "New Recipe", order = 1)]
    public class Recipe : ScriptableObject, IRecipe
    {
        /// <summary>
        /// 所需的资源名称
        /// </summary>
        public List<string> NeededResourcesName;

        /// <summary>
        /// 配方名称
        /// </summary>
        public string CraftName;

        /// <summary>
        /// 配方等级
        /// </summary>
        public int CraftLevel;

        /// <summary>
        /// 获取所需原料
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, float> GetNeededResources()
        {
            var dict = new Dictionary<string, float>();
            foreach (var resourceName in NeededResourcesName)
            {
                // could implement a more flexible system that has dynamic resources's count (need to create ad-hoc actions or a generic one that handle number of resources)
                dict[resourceName] = 1f;
            }

            return dict;
        }

        /// <summary>
        /// 获取配方名称
        /// </summary>
        /// <returns></returns>
        public string GetCraftedResource()
        {
            return CraftName;
        }
        /// <summary>
        /// 获取配方等级
        /// </summary>
        /// <returns></returns>
        public int GetCraftedLevel() {
            return CraftLevel;
        }
    }

    /// <summary>
    /// 配方接口
    /// </summary>
    public interface IRecipe
    {
        Dictionary<string, float> GetNeededResources();
        string GetCraftedResource();

        int GetCraftedLevel();
    }
}