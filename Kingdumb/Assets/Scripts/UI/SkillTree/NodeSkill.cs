using System.Collections;
using System.Collections.Generic;
using UltimateClean;
using UnityEngine;
using UnityEngine.UI;

public class NodeSkill : MonoBehaviour
{
    private SkillTreeUI skillTree;

    [SerializeField]
    private GameObject particleBG;
    [SerializeField]
    private Image backGroundObj;
    [SerializeField]
    private Image iconObj;

    private Color deactiveBGColor = Color.gray;
    private Color activeBGColor = Color.white;
    private Vector3 scaleUp = Vector3.one;

    private bool isActive = false;
    private int nodeNumber;

    [SerializeField]
    private List<NodeSkill> parentList;
    //private List<NodeSkill> parentList = new List<NodeSkill>();
    [SerializeField]
    private List<NodeSkill> childList;
    //private List<NodeSkill> childList = new List<NodeSkill>();
    private List<NodeSkill> lockedNodeList = new List<NodeSkill>(); //이 리스트에 들어있는 노드가 활성화 되어있다면 활성화 불가

    private string skillNameText;
    private string skillInfoText;

    void Awake()
    {
        // Inspector에서 값이 설정되지 않았을 경우 안전하게 초기화
        parentList ??= new List<NodeSkill>();
        childList ??= new List<NodeSkill>();

        Init();
    }

    void Init()
    {
        //Debug.Log("NodeSkill Init Call");
        SetNodeNumber();
        ParticleSystem ps = particleBG.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Clear();
            ps.Play(); // 파티클 강제 재생
        }

        if (isActive)
        {
            //Debug.Log("간선 활성화 시도");
            //skillTree = GetComponentInParent<SkillTreeUI>();
            Dictionary<string, EdgeUI> edges = skillTree.GetEdges();
            string edgeName = "";
            List<NodeSkill> parents = parentList;
            //Debug.Log($"해당 노드는 부모가 {parents.Count}명이나 있네요 ㄷㄷ");
            if (parents != null || parents.Count > 0)
            {
                foreach (NodeSkill p in parents)
                {
                    int from = p.getNodeNum();
                    int to = nodeNumber;

                    edgeName = from + "-" + to;

                    if (edges.ContainsKey(edgeName))
                    {
                        edges[edgeName].activeEdge();
                    }
                    // else
                    // {
                    //     Debug.Log($"{edgeName}을 포함하지 않네요 왜죠");
                    // }
                }
            }
        }

        if (!isActive)
        { 
        //isActive = false;
            backGroundObj.color = deactiveBGColor;
            particleBG.SetActive(false);
        }
    }

    public void ActiveNode()
    {
        //Debug.Log("ActivateNode Call");
        isActive = true;
        backGroundObj.color = activeBGColor;
        particleBG.SetActive(true);
        iconObj.color = new Color(255f, 255f, 255f, 255f);
        transform.localScale = new Vector3(1f, 1f, 1f);
        iconObj.color = Color.black;
    }

    public void LockNode(NodeSkill skill)
    {
        lockedNodeList.Add(skill);        
    }
   
    //어떤 리스트 노드가 활성화 되어있다면 false 반환
    public bool CheckLock()
    {
        foreach (NodeSkill node in lockedNodeList)
        {
            if(node.GetActive())
            {
                return false;
            }
        }
        return true;
    }
    public void OnButtonClicked()
    {
        GlobalSoundManager.Instance.PlayClickSound();

        if (skillTree != null)
        {
            //Debug.Log("버튼 클릭");            
            skillTree.SelectNode(this);
        }
    }

    //Getter, Setter
    private void SetNodeNumber()
    {
        int lastIdx = gameObject.name.LastIndexOf('_');
        string str = gameObject.name.Substring(lastIdx + 1);
        int.TryParse(str, out nodeNumber);
    }

    public void SetIcon(Sprite img)
    {
        iconObj.color = new Color(255f, 255f, 255f, 255f / 0.8f);
        iconObj.sprite = img;
        
        RectTransform imageRect = iconObj.GetComponent<RectTransform>();

        imageRect.localScale = new Vector3(1.2f, 1.2f, 1.2f);
    }

    public void SetSkillTree(SkillTreeUI _skillTree)
    {
        skillTree = _skillTree;
    }

    public bool GetActive()
    {
        return isActive;
    }

    public int getNodeNum()
    {
        return nodeNumber;
    }

    public void SetSkillNameText(string text)
    {
        skillNameText = text;
    }

    public string GetSkillNameText()
    {
        return skillNameText;
    }

    public void SetInfoText(string text)
    {
        skillInfoText = text;
    }

    public string GetInfoText()
    {
        return skillInfoText;
    }

    public List<NodeSkill> GetParentList()
    {
        return parentList;
    }
}
