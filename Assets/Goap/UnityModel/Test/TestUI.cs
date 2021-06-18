using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReGoap.Unity;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.Actions;
using ReGoap.Unity.FSMExample.OtherScripts;
using System.Linq;
using ReGoap.Unity.FSMExample.Goals;
using ReGoap.Unity.FSMExample.Agents;
public class TestUI : MonoBehaviour
{
    public InputField pre;
    public InputField effect;
    public InputField level;
    public InputField goalName;

    public GameObject Agent;
    public GameObject AgentParent;
    [HideInInspector]
    public GameObject CurAgent;

    Stack<GameObject> gameObjects;
    void Start()
    {
        gameObjects = new Stack<GameObject>();
    }



    public void AddCraftAction() {
        var ac = CurAgent.AddComponent<CraftRecipeAction>();
        Recipe recipe = new Recipe();
        recipe.CraftLevel = int.Parse(level.text);
        recipe.CraftName = effect.text;
        string[] split = pre.text.Split(',');
        recipe.NeededResourcesName = split.ToList();
        ac.SetRecipe(recipe);
    }
    public void AddBuyCraftAction() {
        CurAgent.AddComponent<BuyResourceAction>();
   
    }
    public void AddGoal() {
        var ac = CurAgent.AddComponent<CollectResourceGoal>();
        //ac.ResourceName = goalName.name;
        ac.SetGoal(goalName.text);
    }

    public void CreatAgent() {
        CurAgent = GameObject.Instantiate(Agent);
        gameObjects.Push(CurAgent);
        CurAgent.SetActive(true);
        CurAgent.transform.SetParent(AgentParent.transform);
    }

    public void StartAgent() {
        CurAgent.AddComponent<BuilderAgent>();
    }

    public void ClearAgent() {
        var go = gameObjects.Pop();
        Destroy(go);
    }

}
