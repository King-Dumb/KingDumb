using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.EventSystems;

public class TowerBuyUI : MonoBehaviourPun
{
    private String towerSpritePath = "ObjectPool/TowerUI/";

    public GameObject towerToggleUI;
    public GameObject towerButtonUI;

    private Toggle[] towerToggles;

    public Button[] towerButtons;
    private GameObject[] towerGoldImages;
    private TextMeshProUGUI[] towerButtonTexts;

    public Image[] towerButtonImages;
    public Image[] towerFlagImages;
    private Color[] towerFlagColors = { Color.red, Color.green, Color.blue, Color.magenta };
    private int selectedTowerType; // 현재 선택된 타워의 타입 / 0: 전사, 1: 궁수, 2: 마법사, 3: 힐러
    private GameObject selectedTowerGround;
    private int selectedTowerGroundIndex;

    private int builtTowerType;
    private int builtTowerLevel;

    private bool isTowerBuilt = false;
    private Tower builtTower;

    public Notification notification;
    public TextMeshProUGUI towerNameUI;
    public TextMeshProUGUI towerLevelUI;
    public TextMeshProUGUI towerInfoUI;
    public GameObject towerStatUI;
    public TextMeshProUGUI statTextForHealTower;
    private Image[] towerStarImages;

    void Awake()
    {
        towerToggles = towerToggleUI.GetComponentsInChildren<Toggle>();

        for (int i = 0; i < towerToggles.Length; i++)
        {
            // i => 0: 전사, 1: 궁수, 2: 마법사, 3: 힐러
            int index = i;
            towerToggles[i].onValueChanged.AddListener((isOn) => OnToggleChanged(isOn, index));
        }

        towerButtonTexts = new TextMeshProUGUI[4];
        towerGoldImages = new GameObject[4];
        for (int i = 0; i < towerButtons.Length; i++)
        {
            int index = i;
            towerButtons[i].onClick.AddListener(() => OnButtonClicked(index));

            towerButtonTexts[i] = towerButtons[i].gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            towerGoldImages[i] = towerButtons[i].gameObject.transform.GetChild(1).gameObject;
        }

        towerStarImages = towerStatUI.GetComponentsInChildren<Image>();
    }

    public void Initialize(GameObject selectedTowerGround, int index)
    {
        builtTowerLevel = 0;
        builtTowerType = 0;

        this.selectedTowerGround = selectedTowerGround;

        //isTowerBuilt = selectedTowerGround.GetComponent<ParticleSystem>().isStopped;
        isTowerBuilt = TowerManager.Inst.GetIsTowerBuilt(index);
        // Debug.Log("건설할 지역은:" + index + "이고 " + isTowerBuilt);
        selectedTowerGroundIndex = index;


        if (isTowerBuilt) // 이미 세워진 타워가 있다면
        {
            builtTower = selectedTowerGround.transform.GetChild(3).gameObject.GetComponent<Tower>();
            builtTowerType = TowerManager.Inst.GetTowerTypeByIndex(index);
            builtTowerLevel = TowerManager.Inst.GetTowerLevelByIndex(index);
        }

        // UI를 SetActive(false->true)해도 초기화 안되는 문제를 해결하기 위해 임시로 작성한 코드
        towerToggles[1].isOn = true;
        towerToggles[0].isOn = true;
    }

    public void OnToggleChanged(bool isOn, int towerType)
    {

        isTowerBuilt = TowerManager.Inst.GetIsTowerBuilt(selectedTowerGroundIndex);
        if (isTowerBuilt)
        {
            builtTowerType = TowerManager.Inst.GetTowerTypeByIndex(selectedTowerGroundIndex);
            builtTowerLevel = TowerManager.Inst.GetTowerLevelByIndex(selectedTowerGroundIndex);
        }

        if (isOn)
        {
            GlobalSoundManager.Instance.PlayUIActiveSound();
            towerToggles[towerType].transform.GetChild(1).gameObject.SetActive(true);

            // 버튼의 텍스트 및 이미지를 변경하고 towerType을 변경하는 로직
            selectedTowerType = towerType;
            // 현재 타워 타입의 최대 설치 가능 레벨 가져옴 -> builtTowerLevel까지만 건설 가능
            // Debug.Log($"현재 설치된 타워 : {builtTowerLevel}");

            for (int i = 0; i < towerButtons.Length; i++)
            {
                //towerButtonTexts[i].text = "select tower";
                // 골드가 제대로 파싱되었는지 확인하는 테스트용 코드: 버튼에 해당 타워 구매금액이 달린다.
                towerGoldImages[i].SetActive(true);
                towerButtonTexts[i].text = TowerManager.Inst.GetRequireGold(towerType, i).ToString();
                towerButtons[i].interactable = true;
                Sprite newSprite = Resources.Load<Sprite>(towerSpritePath + towerType + i);
                towerButtonImages[i].sprite = newSprite;
                towerFlagImages[i].color = towerFlagColors[towerType];
            }

            // 버튼 활성화 상태 초기화
            for (int i = 0; i < towerButtons.Length; i++)
            {
                towerButtons[i].interactable = false;
            }

            // 타워가 건설되어 있고, 건설된 타워가 현재 활성화된 토글의 타워와 일치할 때만 타워 레벨에 따른 버튼 활성화를 고려
            if (isTowerBuilt && builtTowerType == towerType)
            {
                // for (int i = 0; i <= (builtTowerLevel + 1 >= towerButtons.Length ? builtTowerLevel : builtTowerLevel + 1); i++)
                // {
                //     towerButtons[i].interactable = true;
                // }

                // 선택된 타워를 표시
                towerGoldImages[builtTowerLevel].SetActive(false);
                towerButtonTexts[builtTowerLevel].text = "건설됨";

                if (builtTowerLevel + 1 < towerButtons.Length)
                {
                    towerButtons[builtTowerLevel + 1].interactable = true;
                }
            }
            else
            {
                towerButtons[0].interactable = true;
            }
        }
        else
        {
            // Toggle이 off가 되었을 때
            towerToggles[towerType].transform.GetChild(1).gameObject.SetActive(false);
        }

    }

    public void OnButtonClicked(int towerLevel)
    {
        GlobalSoundManager.Instance.PlayBuySound();

        isTowerBuilt = TowerManager.Inst.GetIsTowerBuilt(selectedTowerGroundIndex);
        if (isTowerBuilt)
        {
            builtTowerType = TowerManager.Inst.GetTowerTypeByIndex(selectedTowerGroundIndex);
            builtTowerLevel = TowerManager.Inst.GetTowerLevelByIndex(selectedTowerGroundIndex);
        }

        int requireGold = TowerManager.Inst.GetRequireGold(selectedTowerType, towerLevel);
        // Debug.Log(requireGold);

        if (IngameManager.Inst.TotalPlayerGold < requireGold)
        {
            // 해당 위치에 돈이 부족하다는 모달(alert)창 UI를 띄운다.
            notification.OnPopup();
            notification.SetContent("골드가 부족합니다.");
            // Debug.Log("돈이 없으면 벌고 오세요");
            return;
        }

        if (isTowerBuilt)
        {
            if (builtTowerType == selectedTowerType)
            {
                towerButtonTexts[builtTowerLevel].text = TowerManager.Inst.GetRequireGold(builtTowerType, builtTowerLevel).ToString(); // 선택받지 못한 타워의 UI 텍스트 초기화
                towerGoldImages[builtTowerLevel].SetActive(true);
                towerButtons[builtTowerLevel].interactable = false;

            }
            Tower builtTower = TowerManager.Inst.GetTowerByIndex(selectedTowerGroundIndex);

            builtTower.gameObject.transform.SetParent(null);
            // 마스터 클라이언트에게 타워 파괴를 요청
            builtTower.GetComponent<Tower>().DestroyTowerRequestToMaster();
            //PhotonNetwork.Destroy(builtTower.gameObject);
        }

        towerButtonTexts[towerLevel].text = "건설됨";
        towerGoldImages[towerLevel].SetActive(false);
        towerButtons[towerLevel].interactable = false;

        Vector3 newTowerDirection = (selectedTowerGround.transform.position - Vector3.zero).normalized;
        Quaternion newTowerRotation = Quaternion.LookRotation(newTowerDirection);

        TowerManager.Inst.BuildTowerRequestToMaster(selectedTowerType, towerLevel, selectedTowerGround.transform.position, newTowerRotation, requireGold, selectedTowerGroundIndex);
        //GameObject newTower = PhotonNetwork.Instantiate($"Tower{selectedTowerType}{towerLevel}", selectedTowerGround.transform.position, newTowerRotation);

        builtTowerType = selectedTowerType;
        builtTowerLevel = towerLevel;

        //TowerManager.Inst.SetIsTowerBuilt(selectedTowerGroundIndex, true);
        //isTowerBuilt = true; // 이거 안하면 파괴 로직 호출 안됨

        // maxTowerLevelList[builtTowerType] = towerLevel + 1; // 타워의 최대 건설 가능 레벨 반영

        // 즉각적인 버튼 상태 업데이트 위해서 추가한 코드
        if (builtTowerLevel + 1 < towerButtons.Length)
        {
            towerButtons[builtTowerLevel + 1].interactable = true;
        }
    }

    // 각 버튼의 EventTrigger 컴포넌트에서 호출됨
    public void ShowTowerInfoWhenHovered(int towerLevelIndex)
    {
        // PointerEventData pointerEventData = eventData as PointerEventData;

        TowerData towerData = TowerManager.Inst.GetTowerDataByIndex(selectedTowerType, towerLevelIndex);
        towerNameUI.text = towerData.towerName;
        towerLevelUI.text = $"{towerData.level}단계";
        towerInfoUI.text = towerData.towerInfo;

        int[] towerRateList = TowerInfo.GetTowerRateByIndex(selectedTowerType, towerLevelIndex);

        if (selectedTowerType == 3) // 힐타워일 때
        {
            // 힐타워 전용 텍스트 설정 로직 + 투사체 속도 변수 초기화
            statTextForHealTower.text = "회복량";
            towerRateList[1] = 0;
        }
        else
        {
            statTextForHealTower.text = "공격력";
        }

        for (int i = 0; i < towerRateList.Length - 1; i++)
        {
            int starIndex = i * 5;
            for (int j = 0; j < towerRateList[i]; j++)
            {
                Sprite newSprite = Resources.Load<Sprite>(towerSpritePath + "Stared");
                towerStarImages[starIndex + j].sprite = newSprite;
            }

            for (int k = towerRateList[i]; k < 5; k++)
            {
                Sprite newSprite = Resources.Load<Sprite>(towerSpritePath + "Unstared");
                towerStarImages[starIndex + k].sprite = newSprite;
            }
        }
    }

    private void OnDisable()
    {
        notification.OffPopup();
    }
}
