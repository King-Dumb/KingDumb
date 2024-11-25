using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using Cinemachine;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("Room Info")]
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI roomCode;

    [Header("Button")]
    public Button startButton;
    public Button exitButton;

    [Header("Player")]
    private Player[] playerList;
    public Transform playerStatus;
    public GameObject playerStatusPrefab;


    [Header("Character")]
    public GameObject selectCharacterPopup;

    // Player Character Prefabs
    //private GameObject warrior;
    //private GameObject archer;
    //private GameObject mage;
    //private GameObject priest;

    // Select buttons
    public Button warriorBtn;
    public Button archerBtn;
    public Button mageBtn;
    public Button priestBtn;
    public Button selectButton;

    // Character Desc
    public Image roleIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI characterRole;
    public TextMeshProUGUI attackDesc;
    public TextMeshProUGUI skillDesc;
    public TextMeshProUGUI ultimateDesc;

    // Slider
    public Slider healthSlider;
    public Slider attackSlider;
    public Slider speedSlider;
    public Slider defenseSlider;
    public Slider magicSlider;

    // 3D Objects
    public GameObject warriorObject;
    public GameObject archerObject;
    public GameObject mageObject;
    public GameObject priestObject;
    private int selectedObject;

    // Animation
    public Animator warriorAnimator;
    public Animator archerAnimator;
    public Animator mageAnimator;
    public Animator priestAnimator;


    // Camera
    public CinemachineVirtualCamera playerCameraPrefab;
    private new CinemachineVirtualCamera camera;
    public Camera characterCamera;

    // Canvas
    public Canvas canvas;

    private CursorController cursor;
    public Notification notification;
    public Menu menu;
    //public DebugManager debugManager;

    private int characterIdx;

    private SceneBGMManager sceneBGMManager;

    private void Awake()
    {
        //Debug.Log("Awake");
        // 방 정보 초기화 (방 이름, 방 코드)
        roomName.text = GameConfig.ParseString(PhotonNetwork.CurrentRoom.Name)[0];
        roomCode.text = PhotonNetwork.CurrentRoom.CustomProperties["roomCode"].ToString();

        // 버튼 초기화
        SetButton();

        startButton.onClick.AddListener(OnClickStartButton);
        exitButton.onClick.AddListener(OnClickExitButton);

        // 캐릭터 선택 UI 초기화
        selectCharacterPopup.SetActive(false);
        warriorObject.SetActive(false);
        archerObject.SetActive(false);
        mageObject.SetActive(false);
        priestObject.SetActive(false);

        characterCamera.gameObject.SetActive(false);

        warriorBtn.onClick.AddListener(() => OnClickCharacterBtn("WARRIOR"));
        archerBtn.onClick.AddListener(() => OnClickCharacterBtn("ARCHER"));
        mageBtn.onClick.AddListener(() => OnClickCharacterBtn("MAGE"));
        priestBtn.onClick.AddListener(() => OnClickCharacterBtn("PRIEST"));
        selectButton.onClick.AddListener(OnClickSelectButton);

        // 커서 초기화
        cursor = GetComponent<CursorController>();


        sceneBGMManager = FindObjectOfType<SceneBGMManager>();

        //초기 캐릭터 인덱스는 전사
        //캐릭터는 인게임 매니저에서 만들어짐
        characterIdx = 0;
        SetClassCode(0);

    }

    private void Start()
    {
        //Debug.Log("Start");
        // 플레이어 상태 UI 업데이트
        // UpdatePlayerStatusUI();

        // 클라이언트가 방에 입장했는지 확인
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SetReadyStatus(true);
            }
            else
            {
                SetReadyStatus(false);
            }
        }
    }

    private void Update()
    {
        SetButton();

        if (!selectCharacterPopup.activeSelf && !menu.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Tab))
        {
            if (Cursor.lockState == CursorLockMode.Locked) // 커서가 잠긴 상태면 풀기
            {
                cursor.CursorUnLock();
            }

            // Camera Settings
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = characterCamera;
            characterCamera.gameObject.SetActive(true);

            selectCharacterPopup.SetActive(true);

            int code = GameManager.Instance.playerClassCode;
            string className = GetTypeForClassCode(code);
            OnClickCharacterBtn(className);
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            OnClickStartButton();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (selectCharacterPopup.activeSelf)
            {
                cursor.enabled = false;
            }

            if (!menu.gameObject.activeSelf)
            {
                // UI Object 비활성화
                switch (selectedObject)
                {
                    case 0:
                        warriorObject.SetActive(false);
                        break;
                    case 1:
                        archerObject.SetActive(false);
                        break;
                    case 2:
                        mageObject.SetActive(false);
                        break;
                    case 3:
                        priestObject.SetActive(false);
                        break;
                }
                // 메뉴 활성화
                cursor.CursorUnLock();
                menu.ActiveMenu(true);
            }
            else
            {
                // UI Object 활성화
                switch (selectedObject)
                {
                    case 0:
                        warriorObject.SetActive(true);
                        break;
                    case 1:
                        archerObject.SetActive(true);
                        break;
                    case 2:
                        mageObject.SetActive(true);
                        break;
                    case 3:
                        priestObject.SetActive(true);
                        break;
                }

                // 메뉴 비활성화
                if (!selectCharacterPopup.activeSelf)
                {
                    cursor.CursorLock();
                }
                menu.ActiveMenu(false);
            }
        }
    }

    private void OnClickSelectButton()
    {
        sceneBGMManager.PlaySceneSound(8);
        // Camera Settings
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        characterCamera.gameObject.SetActive(false);

        // UI Off -> 캐릭터 체험 화면으로 돌아가기
        selectCharacterPopup.SetActive(false);

        cursor.enabled = true;
        cursor.CursorLock();

        SetCharacter(characterIdx);
        SetClassCode(characterIdx);
    }

    // 선택창의 캐릭터 애니메이션 설정
    private void SetAnimation(int type, Animator animator)
    {
        switch (type)
        {
            case 0:
                animator.SetTrigger("Warrior");
                break;
            case 1:
                animator.SetTrigger("Archer");
                break;
            case 2:
                animator.SetTrigger("Mage");
                break;
            case 3:
                animator.SetTrigger("Priest");
                break;

        }
    }

    // 캐릭터 선택 동기화
    private void OnClickCharacterBtn(string type)
    {
        switch (type)
        {
            case "WARRIOR":
                sceneBGMManager.PlaySceneSound(0);
                sceneBGMManager.PlaySceneSound(4);
                characterIdx = 0;
                //SetCharacter(0);
                //SetClassCode(0);

                // 3D Objects
                warriorObject.SetActive(true);
                archerObject.SetActive(false);
                mageObject.SetActive(false);
                priestObject.SetActive(false);
                selectedObject = 0;

                // Animation 활성화
                SetAnimation(0, warriorAnimator);

                // 파티 관리 UI 변경
                UpdatePlayerStatusUI();

                // 캐릭터 설명 설정
                SetCharacterDesc(0);
                break;
            case "ARCHER":
                sceneBGMManager.PlaySceneSound(1);
                sceneBGMManager.PlaySceneSound(5);
                characterIdx = 1;
                //SetCharacter(1);
                //SetClassCode(1);

                // 3D Objects
                warriorObject.SetActive(false);
                archerObject.SetActive(true);
                mageObject.SetActive(false);
                priestObject.SetActive(false);
                selectedObject = 1;

                SetAnimation(1, archerAnimator);

                // 파티 관리 UI 변경
                UpdatePlayerStatusUI();

                // 캐릭터 설명 설정
                SetCharacterDesc(1);
                break;
            case "MAGE":
                sceneBGMManager.PlaySceneSound(2);
                sceneBGMManager.PlaySceneSound(6);
                characterIdx = 2;
                //SetCharacter(2);
                //SetClassCode(2);

                // 3D Objects
                warriorObject.SetActive(false);
                archerObject.SetActive(false);
                mageObject.SetActive(true);
                priestObject.SetActive(false);
                selectedObject = 2;

                SetAnimation(2, mageAnimator);

                // 파티 관리 UI 변경
                UpdatePlayerStatusUI();

                // 캐릭터 설명 설정
                SetCharacterDesc(2);
                break;
            case "PRIEST":
                sceneBGMManager.PlaySceneSound(3, 0.4f);
                sceneBGMManager.PlaySceneSound(7);
                characterIdx = 3;
                //SetCharacter(3);
                //SetClassCode(3);

                // 3D Objects
                warriorObject.SetActive(false);
                archerObject.SetActive(false);
                mageObject.SetActive(false);
                priestObject.SetActive(true);
                selectedObject = 3;

                SetAnimation(3, priestAnimator);

                // 파티 관리 UI 변경
                UpdatePlayerStatusUI();

                // 캐릭터 설명 설정
                SetCharacterDesc(3);
                break;
        }
    }

    public void SetCharacterDesc(int type)
    {
        switch (type)
        {
            case 0:
                // 직업 아이콘 변경
                roleIcon.sprite = Resources.Load<Sprite>("Sprites/Role_Warrior");

                // 캐릭터 이름 변경
                characterName.text = "발더 (Baldur)";

                // 캐릭터 직업 변경
                characterRole.text = "전사";

                // 기본 공격 설명
                attackDesc.text = "검을 휘둘러 근접한 적을 공격합니다.";

                // 스킬 설명
                skillDesc.text = "검이 전방으로 회전하며 적을 관통한 뒤 발더에게 돌아옵니다. 경로상의 적들은 큰 피해를 입습니다.";

                // 궁극기 설명
                ultimateDesc.text = "검을 휘둘러 특정 거리까지 강력한 충격파를 생성합니다. 범위 내 적들은 큰 피해를 입습니다.";

                // 스탯 정보 변경
                healthSlider.value = 1f;
                attackSlider.value = 1f;
                speedSlider.value = 0.5f;
                defenseSlider.value = 1f;
                magicSlider.value = 0f;
                break;
            case 1:
                // 직업 아이콘 변경
                roleIcon.sprite = Resources.Load<Sprite>("Sprites/Role_Archer");

                // 캐릭터 이름 변경
                characterName.text = "슌 (Shawn)";

                // 캐릭터 직업 변경
                characterRole.text = "궁수";

                // 기본 공격 설명
                attackDesc.text = "상대를 향해 화살을 쏩니다. 마우스 우클릭으로 기를 모은 후 발사하면 데미지가 올라갑니다.";

                // 스킬 설명
                skillDesc.text = "10초간 마법화살을 3연속 발사합니다. 스킬 활성화 동안에는 차징을 할 수 없습니다.";

                // 궁극기 설명
                ultimateDesc.text = "천천히 전진하는 거대한 화살을 소환하여 범위에 닿는 모든 적들에게 지속 데미지를 입힙니다.";

                // 스탯 정보 변경
                healthSlider.value = 0.8f;
                attackSlider.value = 0.65f;
                speedSlider.value = 0.5f;
                defenseSlider.value = 0f;
                magicSlider.value = 0f;
                break;
            case 2:
                // 직업 아이콘 변경
                roleIcon.sprite = Resources.Load<Sprite>("Sprites/Role_Mage");

                // 캐릭터 이름 변경
                characterName.text = "아르피아 (Arpia)";

                // 캐릭터 직업 변경
                characterRole.text = "마법사";

                // 기본 공격 설명
                attackDesc.text = "에너지 구체를 발사하여 접촉한 적에게 피해를 입히고 주변 적에게 약간의 피해를 추가로 입힙니다.";

                // 스킬 설명
                skillDesc.text = "지면에 마법진을 그리고 해당 범위 내에 에너지 파동을 발생시켜 광역 데미지를 입힙니다.";

                // 궁극기 설명
                ultimateDesc.text = "공중에 커다란 에너지 덩어리를 만들어 주변 모든 적을 휩쓸어버릴 정도로 강력한 에너지 폭발을 일으킵니다.";

                // 스탯 정보 변경
                healthSlider.value = 0.5f;
                attackSlider.value = 0.8f;
                speedSlider.value = 0.5f;
                defenseSlider.value = 0f;
                magicSlider.value = 1f;
                break;
            case 3:
                // 직업 아이콘 변경
                roleIcon.sprite = Resources.Load<Sprite>("Sprites/Role_Priest");

                // 캐릭터 이름 변경
                characterName.text = "엘리나 (Elina)";

                // 캐릭터 직업 변경
                characterRole.text = "사제";

                // 기본 공격 설명
                attackDesc.text = "빛의 구체를 발사합니다. 맞은 적은 피해를 입고 뒤로 밀려납니다.";

                // 스킬 설명
                skillDesc.text = "일정 시간 동안 영역을 생성하여 아군을 치유하고 적에게는 피해를 입힙니다.";

                // 궁극기 설명
                ultimateDesc.text = "일정 시간 동안 소환수를 불러옵니다. 소환수는 엘리나 주변의 적을 공격합니다.";

                // 스탯 정보 변경
                healthSlider.value = 0.6f;
                attackSlider.value = 0.35f;
                speedSlider.value = 0.5f;
                defenseSlider.value = 0f;
                magicSlider.value = 0.5f;
                break;
        }
    }

    public void SetReadyStatus(bool isReady)
    {
        //Debug.Log("SetReadyStatus : " + isReady);
        Hashtable newProperties = new Hashtable { { "IsReady", isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(newProperties);
    }

    public void SetClassCode(int classCode)
    {
        Hashtable newProperties = new Hashtable { { "ClassCode", classCode } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(newProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        UpdatePlayerStatusUI();
    }

    private void SetButton()
    {
        // 마스터 여부 확인
        if (PhotonNetwork.IsMasterClient)
        {
            // debugManager.SetInfoText("Master");
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "시작";
        }
        else
        {
            // debugManager.SetInfoText("Client");
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "준비";
        }
    }

    private void UpdatePlayerStatusUI()
    {
        // UI 초기화
        foreach (Transform child in playerStatus)
        {
            Destroy(child.gameObject);
        }

        // 플레이어 목록 불러오기
        playerList = PhotonNetwork.PlayerList;

        // 플레이어 상태 프리팹 생성 및 UI 초기화
        foreach (Player player in playerList)
        {
            // UI 표시
            GameObject playerUIEntry = Instantiate(playerStatusPrefab, playerStatus);
            playerUIEntry.transform.Find("Nickname").GetComponent<TextMeshProUGUI>().text = player.NickName;
            // 본인 UI 표시
            if (player.IsLocal)
            {
                playerUIEntry.transform.Find("Frame").GetComponent<Image>().color = Color.cyan;
            }
            bool type = player.CustomProperties.TryGetValue("ClassCode", out object classCode);
            if (type)
            {
                string spritePath = "";
                switch (classCode)
                {
                    case 0: // 전사
                        spritePath = "Sprites/Warrior_Head";
                        break;
                    case 1: // 궁수
                        spritePath = "Sprites/Archer_Head";
                        break;
                    case 2: // 마법사
                        spritePath = "Sprites/Mage_Head";
                        break;
                    case 3: // 사제
                        spritePath = "Sprites/Priest_Head";
                        break;
                }
                Sprite newSprite = Resources.Load<Sprite>(spritePath);
                playerUIEntry.transform.Find("Pic").GetComponent<Image>().sprite = newSprite;
            }
            if (player.IsMasterClient)
            {
                playerUIEntry.transform.Find("Ready").GetComponent<TextMeshProUGUI>().text = "MASTER";
            }
            else
            {
                playerUIEntry.transform.Find("Ready").GetComponent<TextMeshProUGUI>().text = "READY";
            }
            player.CustomProperties.TryGetValue("IsReady", out object isReady);
            if (isReady != null)
            {
                playerUIEntry.transform.Find("Ready").gameObject.SetActive((bool)isReady);
            }
            else
            {
                playerUIEntry.transform.Find("Ready").gameObject.SetActive(false);
            }
        }
    }

    // 다른 플레이어가 입장했을 때 호출되는 메서드
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SetReadyStatus(true);
        }

        UpdatePlayerStatusUI();
    }

    // 다른 플레이어가 퇴장했을 때 호출되는 메서드
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SetReadyStatus(true);
        }

        UpdatePlayerStatusUI();
    }

    private bool isStarted = false;
    private void OnClickStartButton()
    {
        // 모든 플레이어 레디 상태 확인
        if (PhotonNetwork.IsMasterClient)
        {
            //debugManager.SetInfoText("Master : " + PhotonNetwork.LocalPlayer.NickName);

            // 모두가 레디했으면 시작
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                player.CustomProperties.TryGetValue("IsReady", out object isReady);
                if (isReady == null || (bool)isReady == false)
                {
                    //debugManager.SetInfoText(player.NickName + " : " + (bool)isReady);
                    if ((bool)isReady == false)
                    {
                        notification.OnPopup();
                        notification.SetContent("모든 플레이어가 준비 상태여야 합니다.");
                    }
                    return;
                }
            }

            if (!isStarted)
            {
                isStarted = true;

                // 룸을 로비에서 보이지 않게 설정
                PhotonNetwork.CurrentRoom.IsVisible = false;

                // 모든 클라이언트에게 게임 시작 이벤트 전송 -> InGame 씬으로 이동
                PhotonManager.SendEvent(1);
            }
        }
        else
        {
            // 레디 상태 변경
            PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsReady", out object isReady);
            // debugManager.SetInfoText("Client : " + (bool)isReady);
            if (isReady == null)
            {
                isReady = false;
            }
            SetReadyStatus(!(bool)isReady);
        }
    }

    private void OnClickExitButton()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        // else
        // {
        //     Debug.Log("방에 있지 않습니다.");
        // }
    }

    //0 전사 1 궁수 2 마법사 3 사제
    private void SetCharacter(int classCode)
    {
        CharacterManager mgr = CharacterManager.Inst;
        mgr.SetPlayer(classCode, GameManager.Instance.localPlayer.transform.position, Quaternion.identity);
    }

    private string GetTypeForClassCode(int classCode)
    {
        switch (classCode)
        {
            case 0:
                return "WARRIOR";
            case 1:
                return "ARCHER";
            case 2:
                return "MAGE";
            case 3:
                return "PRIEST";
        }

        return null;
    }
}
