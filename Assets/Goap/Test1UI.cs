using ReGoap.Core;
using ReGoap.Unity.FSMExample.Actions;
using ReGoap.Unity.FSMExample.Agents;
using ReGoap.Unity.FSMExample.Goals;
using ReGoap.Unity.FSMExample.OtherScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test1UI : MonoBehaviour
{

    public GameObject creatGo;
    public GameObject ShowGo;
    public GameObject AgentP;
    Dictionary<string, GameObject> agentDic = new Dictionary<string, GameObject>();
    public Dropdown delDrop;
    public Dropdown showDrop;
    [HideInInspector]
    public int agentIdx = 0;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenCreat() {
        creatGo.SetActive(!creatGo.activeSelf);
    }
    public void OpenShow() {
        ShowGo.SetActive(!ShowGo.activeSelf);
        if (ShowGo.activeSelf) {
            ShowInfo();
        }
    }

    #region Creat
    public GameObject agent;
     GameObject cagent;
    public Text info1;
    Dictionary<string, string> showInfo1 = new Dictionary<string, string>();

    public Dropdown goalDrop;

    public Dropdown actionDrop1;
    public Dropdown actionDrop2;
    public Dropdown actionDrop3;

    public Dropdown toolDrop;

    public Dropdown effDrop;
    [HideInInspector]
    public int actionIdx = 0;
    public void CreatAgent() {
        cagent = GameObject.Instantiate(agent, AgentP.transform);
        agentDic.Add(agentIdx.ToString(), cagent);
        
        cagent.SetActive(true);
         agentIdx++;
        InitDic();
        OpenCreat();
        showInfo1.Clear();
    }
    public void DeletAgent() {
        //   Destroy(agentDic)
        Destroy(agentDic[delDrop.captionText.text]);
        agentDic.Remove(delDrop.captionText.text);
        InitDic();
    }
    private void InitDic() {
        delDrop.ClearOptions();
        showDrop.ClearOptions();
        List<string> list = new List<string>();
        foreach (var item in agentDic) {
            list.Add(item.Key);
        }
        delDrop.AddOptions(list);
        showDrop.AddOptions(list);
    }

    public void ShowInfo1() {
        info1.text = "";
        foreach (var item in showInfo1) {
            info1.text += item.Key + "_" + item.Value + "\n";
        }
    }
    public void CreatGoal() {
        showInfo1.Add("Goal", $"Ŀ���Ǵ�� {goalDrop.captionText.text}");
        var g = cagent.AddComponent<CollectResourceGoal>();
        g.SetGoal(goalDrop.captionText.text);
        ShowInfo1();
    }
    public void CtratAction() {

        showInfo1.Add("Action" + actionIdx++, $"��Ҫ{actionDrop1.captionText.text}_{actionDrop2.captionText.text}_{actionDrop3.captionText.text}=>���{effDrop.captionText.text}");
        var g = cagent.AddComponent<CraftRecipeAction>();
        Recipe re = new Recipe();
        re.NeededResourcesName = new List<string>();
        if (actionDrop1.captionText.text != "Null") {
            re.NeededResourcesName.Add(actionDrop1.captionText.text);
        }
        if (actionDrop2.captionText.text != "Null") {
            re.NeededResourcesName.Add(actionDrop2.captionText.text);
        }
        if (actionDrop3.captionText.text != "Null") {
            re.NeededResourcesName.Add(actionDrop3.captionText.text);
        }
        re.CraftName = effDrop.captionText.text;

        if (toolDrop.captionText.text == "����̨") {
            re.CraftLevel = 3;
        } else {
            re.CraftLevel = 1;
        }
        

        g.SetRecipe(re);

        ShowInfo1();
    }
    public void CtratBuyAction() {
        showInfo1.Add("Action" + actionIdx++, "�Ӳֿ�ȡ��Axe");
        cagent.AddComponent<BuyResourceAction>();
        ShowInfo1();
    }
    public void StartTo() {
        cagent.AddComponent<BuilderAgent>();
        creatGo.SetActive(false);
    }
    #endregion

    #region Open
    public Text Openinfo;
    public void ShowInfo() {
        var ag = agentDic[showDrop.captionText.text].GetComponent<BuilderAgent>();
        Openinfo.text = "";
        if (ag != null) {
            string a = "";
            var list1 = ag.GetGoalsSet();
            for (int i = 0; i < list1.Count; i++) {
                Openinfo.text += $"{list1[i].ToString()} \n";
            }
           
            var list2 = ag.GetActionsSet();
            for (int i = 0; i < list2.Count; i++) {
                Openinfo.text += $"{list2[i].ToString()} \n";
            }
            var list3 = ag.GetMemory();
            Openinfo.text += list3.ToString();
        }
    }

    #endregion
}
