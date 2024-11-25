using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SkillTreeUI : MonoBehaviour
{
    [SerializeField]
    private GameObject edgeObject;
    [SerializeField]
    private GameObject nodeObject;

    private List<NodeSkill> nodes;
    private Dictionary<string, EdgeUI> edges;

    [SerializeField]
    private TextMeshProUGUI skillNameText;
    [SerializeField]
    private TextMeshProUGUI skillInfoText;

    [SerializeField]
    private TextMeshProUGUI skillPointText;

    [SerializeField]
    private Button upgradeButton;

    [SerializeField]
    private Button exitButton;

    private int skillPoint;
    private string defaultSkillPointText = "포인트 : ";

    private NodeSkill selectedNode;

    private string classType;

    private string warriorJson = "warrior_skills";
    private string archerJson = "archer_skills";
    private string mageJson = "mage_skills";
    private string priestJson = "priest_skills";

    private object playerSkillObj; //클래스군에 사용되는 변수. MageSkilltreeManager 같은 클래스가 이에 해당됨
    private ISkillTree skillTree;
    private CharacterInfo charInfo;

    void Start()
    {
        //skillPoint = 10;

        //테스트
        //Init(GameConfig.ArcherClass);

        //2노드는 11번노드를 활성화했다면 업그레이드 불가
        //nodes[1].LockNode(nodes[10]);
    }

    //localPlayer의 직업 이름을 가져온다. 그 직업을 기반으로 Json 파일과 이미지를 불러오고 적용시킨다.
    public void Init(string className, CharacterInfo info)
    {
        //Debug.Log("SkillTreeUI Init");
        charInfo = info;

        classType = className;
        //직업정보 받아오기

        nodes = new List<NodeSkill>();
        edges = new Dictionary<string, EdgeUI>();

        //JSON 파일에서 파싱해온 이름
        SkillList skillList = GetSkillDTO(GetJsonName(className));

        //클래스 아이콘 목록을 받아옴
        List<Sprite> skillIcon = GetComponent<SkillIcon>().GetIconList(classType);

        //노드 불러오기
        int idx = 0;
        foreach (Transform child in nodeObject.transform)
        {            
            // 노드 기본 설정
            NodeSkill skill = child.GetComponent<NodeSkill>();

            //문자열 세팅하기
            string skillNameText = skillList.skills[idx].skillName;
            string skillInfoText = skillList.skills[idx].skillInfo;

            skill.SetSkillNameText(skillNameText);
            skill.SetInfoText(skillInfoText);

            //이미지 세팅하기
            skill.SetIcon(skillIcon[idx]);
            
            //skill.ActiveNode();
            
            nodes.Add(skill);            
            skill.SetSkillTree(this);
            idx++;
        }

        //간선 불러오기
        foreach (Transform child in edgeObject.transform)
        {
            edges.Add(child.name, child.GetComponent<EdgeUI>());
        }

        //버튼 세팅
        upgradeButton.onClick.AddListener(OnClickUpgradeButton);
        exitButton.onClick.AddListener(OnClickExitButton);
        
        //스킬포인트 세팅        

        EnableUI();

        skillTree = GameManager.Instance.localPlayer.transform.GetComponent<ISkillTree>();

        //Debug.Log($"{SceneManager.GetActiveScene().name}에서 addSkillPoint를 호출");
        IngameManager.Inst.OnLevelUpEvent += AddSkillPoint;
    }

    private void Update()
    {
        if(charInfo != null)
        {
            skillPoint = charInfo.GetSkillPoint();
            skillPointText.text = defaultSkillPointText + skillPoint;
        }
    }

    private SkillList GetSkillDTO(string jsonName)
    {
        if (SystemManager.Instance == null)
            return null;

        return SystemManager.Instance.LoadJson<SkillList>(jsonName);
    }

    private void OnEnable()
    {
        EnableUI();
    }

    private void EnableUI()
    {
        selectedNode = null;
        skillNameText.text = "스킬 트리";
        skillInfoText.text = "활성화 할 노드를 선택";
        upgradeButton.interactable = false;
        skillPointText.text = defaultSkillPointText + skillPoint;
    }

    private void OnClickUpgradeButton()
    {
        GlobalSoundManager.Instance.PlaySubmitSound();
        ActiveNode();
        UpdateSkillPoint();
        selectedNode = null;
    }

    private void OnClickExitButton()
    {
        GlobalSoundManager.Instance.PlayUIActiveSound();
        gameObject.SetActive(false);
    }

    public void SelectNode(NodeSkill node)
    {
        //Debug.Log("선택한 노드 : "+node.name);
        selectedNode = node;

        SetText();

        bool isActiveUpgrade = CheckUpgrade();
        
        upgradeButton.interactable = CheckUpgrade() && !node.GetActive();
    }

    private void SetText()
    {
        if (selectedNode == null)
            return;
        
        //설명 텍스트 넣기        

        string name = selectedNode.GetSkillNameText();
        string info = selectedNode.GetInfoText();

        string addName = selectedNode.GetActive() ? "\n[강화됨]" : "\n[잠김]";

        string addInfo = "";

        List<NodeSkill> list = selectedNode.GetParentList();

        if(list != null && list.Count > 0)
        {
            int cnt = 0;
            addInfo += "\n(";
            foreach (NodeSkill skill in list)
            {
                if (!skill.GetActive())
                {
                    cnt++;
                    addInfo += skill.GetSkillNameText() +", ";
                }
            }            
            
            addInfo =  cnt > 0 ? addInfo.Substring(0, addInfo.Length - 2) + " 스킬 필요)" : "";                        
        }

        skillNameText.text = name + addName;
        skillInfoText.text = info + addInfo;
    }

    private bool CheckUpgrade()
    {
        //스킬포인트가 0이라면 false;
        if (skillPoint < 1 || selectedNode == null)
        {
            return false;
        }
            
        //부모가 있는데 하나라도 활성화 안되어있다면 false
        List<NodeSkill> parents = selectedNode.GetParentList();
        if (parents != null || parents.Count > 0)
        {
            foreach (NodeSkill p in parents)
            {
                if(!p.GetActive())
                {
                    return false;
                }
            }
        }

        //활성화하면 안되는 노드가 활성화 되어있다면 false
        if(!selectedNode.CheckLock())
        {
            return false;
        }

        return true;
    }

    private void ActiveNode()
    {
        //Debug.Log("@@@@노드 활성화아ㅏㅏ");
        if(selectedNode == null)
        {
            //Debug.Log("selectedNode는 null입니다.");

            return;
        }

        selectedNode.ActiveNode();

        //연결된 간선을 활성화한다.
        string edgeName = "";
        List<NodeSkill> parents = selectedNode.GetParentList();
        if (parents != null || parents.Count > 0)
        {
            foreach (NodeSkill p in parents)
            {
                int from = p.getNodeNum();
                int to = selectedNode.getNodeNum();

                edgeName = from + "-" + to;

                if(edges.ContainsKey(edgeName))
                {
                    edges[edgeName].activeEdge();
                }
            }
        }
    }

    private void UpdateSkillPoint()
    {
        upgradeButton.interactable = false;


        charInfo.UseSkillPoint();
        skillPointText.text = defaultSkillPointText + skillPoint;
        SetText();

        //플레이어에게 알려준다.
        CallbackPlayer(selectedNode.getNodeNum());
    }

    void CallbackPlayer(int nodeNumber)
    {
        skillTree.activateNode(nodeNumber);
    }

    public void AddSkillPoint(int curLevel)
    {
        skillPoint = charInfo.AddSkillPoint();
        skillPointText.text = defaultSkillPointText + skillPoint;
    }
    
    public void LockNode(NodeSkill node1, NodeSkill node2)
    {
        node1.LockNode(node2);
        node2.LockNode(node1);        
    }

    private string GetJsonName(string className)
    {
        switch (className)
        {
            case GameConfig.WarriorClass:
                return warriorJson;
            case GameConfig.ArcherClass:
                return archerJson;
            case GameConfig.MageClass:
                return mageJson;
            case GameConfig.PriestClass:
                return priestJson;

        }
        return null;
    }

    public void LoadSkill()
    {
        //로딩 시 스킬포인트 재할당
        bool[] skillInfo = charInfo.savedSkillNode;
        int size = GameConfig.maxSkillLevel;
        for (int i = 1; i <= size; i++)
        {
            //Debug.Log($"현재{i}번 노드의 값은 {skillInfo[i]}");
            if (skillInfo[i] == true)
            {
                selectedNode = nodes[i - 1];
                ActiveNode();
                SetText();
                selectedNode = null;
            }
        }
    }

    public Dictionary<string, EdgeUI> GetEdges()
    {
        return edges;
    }
}
