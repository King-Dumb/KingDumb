using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//인게임에서 사용되는 UI를 담당하는 매니저
public class IngameUIManager : MonoBehaviour
{
    // 인게임 내에서 활성화 되어 있을 수 있는 커서가 필요한 UI는 한 개
    private GameObject _activedUI;
    public GameObject activedUI
    {
        get { return _activedUI; }
        set
        {
            if (value == null)
            {
                if (_activedUI != null)
                {
                    _activedUI.SetActive(false);
                    _activedUI = null;
                }
                isCursorLocked = true;
                return;
            }

            _activedUI = value;
            _activedUI.SetActive(true);
            isCursorLocked = false;
        }
    }
    public static IngameUIManager Inst;

    [SerializeField]
    private GameObject LoadingUI; // 로딩 UI
    private InputHandler inputHandler;
    private PlayerController playerController;
    private CharacterInfo characterInfo;


    // 상호작용 없는 UI - 커서 필요하지 않음
    public GameObject nexusInfo;
    public GameObject nexusUnderAttack;
    public GameObject towerInfoUI;
    public GameObject towerBuyUI;

    [SerializeField]
    private GameObject settingUI;

    public GameObject skillTreeUI;

    public ResultUI resultUI; // 결과 화면

    // private bool _isSettingUIActive;
    // public bool isSettingUIActive
    // {
    //     get
    //     {
    //         return _isSettingUIActive;
    //     }
    //     set
    //     {
    //         _isSettingUIActive = value;
    //         if (_isSettingUIActive)
    //         {
    //             activedUI = settingUI;
    //         }
    //         else
    //         {
    //             activedUI = null;
    //         }
    //     }
    // }

    private bool _isTowerBuyUIActive;
    public bool isTowerBuyUIActive
    {
        get
        {
            return _isTowerBuyUIActive;
        }
        set
        {
            if (value)
            {
                if (_activedUI != null)
                {
                    ////Debug.Log("여기에서 리턴함");
                    return;
                }

                activedUI = towerBuyUI;
            }
            else
            {
                activedUI = null;
            }

            _isTowerBuyUIActive = value;
        }
    }
    private bool _isSkillTreeUIActive;
    public bool isSkillTreeUIActive
    {
        get
        {
            return _isSkillTreeUIActive;
        }
        set
        {
            if (value)
            {
                if (_activedUI != null)
                {
                    return;
                }
                activedUI = skillTreeUI;
            }
            else
            {
                activedUI = null;
            }

            _isSkillTreeUIActive = value;
        }
    }

    private bool _isResultUIActive;
    public bool isResultUIActive
    {
        get
        {
            return _isResultUIActive;
        }
        set
        {
            activedUI = null;
            activedUI = resultUI.gameObject;
        }
    }


    private bool _isCursorLocked;
    public bool isCursorLocked
    {
        get { return _isCursorLocked; }
        set
        {
            _isCursorLocked = value;

            if (_isCursorLocked)
            {
                CursorLock();
            }
            else
            {
                CursorUnLock();
            }
        }
    }

    [SerializeField]
    private GameObject inGameUI; //플레이어 인게임 UI
    private InGameUI inGameUIScript;

    public Menu menu; // 메뉴 화면
    public GameObject bossWaringUI; // 보스 등장 시 경고창
    public TextMeshProUGUI bossWaringUIText;
    public GameObject stageClearUI; // 게임 클리어 창
    public TextMeshProUGUI stageClearText; // 게임 클리어 창 - 스테이지 텍스트

    //사망 패널
    public GameObject deadPanel;
    public TextMeshProUGUI deadRemainTimeText;

    // 스테이지 패널
    public GameObject stagePanel;
    public TextMeshProUGUI stageText;

    [SerializeField]
    private GameObject informationUI;

    void Awake()
    {
        ////Debug.Log("InGameUIManager Awake");

        //싱글톤 선언
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // InGameUI 컴포넌트 캐싱
        if (inGameUI != null)
        {
            inGameUIScript = inGameUI.GetComponent<InGameUI>();
        }
    }

    void Start()
    {
        ////Debug.Log("InGameUIManager Start");
        CameraControl.Inst.LockCamera();
        //inputHandler = GameManager.Instance.localPlayer.GetComponent<InputHandler>();
        // playerController = GameManager.Instance.localPlayer.GetComponent<PlayerController>();

        // isCursorLocked = true;
        // ////Debug.Log("커서 활성화");
    }

    void Update()
    {
        if (inputHandler == null)
        {
            if (GameManager.Instance.localPlayer != null)
            {
                ////Debug.Log("로컬플레이어가 할당되어 컴포넌트를 가져옴");
                inputHandler = GameManager.Instance.localPlayer.GetComponent<InputHandler>();
                playerController = GameManager.Instance.localPlayer.GetComponent<PlayerController>();
                characterInfo = GameManager.Instance.localPlayer.GetComponent<CharacterInfo>();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activedUI == resultUI.gameObject)
            {
                return;
            }

            if (activedUI != null)
            {
                if (menu.gameObject.activeSelf)
                {
                    menu.ActiveMenu(false);
                }
                activedUI = null;
                return;
            }

            if (menu.gameObject.activeSelf)
            {
                menu.ActiveMenu(false);
                activedUI = null;
            }
            else
            {
                menu.ActiveMenu(true);
                activedUI = menu.gameObject;
            }
        }

        if (Input.GetMouseButton(0) && !isCursorLocked)
        {
            if (activedUI == null)
            {
                isCursorLocked = true;
            }
        }

        // if (Input.GetKeyDown(KeyCode.V))
        if (inputHandler.SkillTreeKeyDown)
        {
            ////Debug.Log("V클릭");
            GlobalSoundManager.Instance.PlayUIActiveSound();
            SetNeedCheckV(false);
            if (isSkillTreeUIActive)
            {
                isSkillTreeUIActive = false;
            }
            else
            {
                isSkillTreeUIActive = true;
            }
        }

        // if (Input.GetKeyDown(KeyCode.M))
        // {
        //     GameObject[] monsterList = GameObject.FindGameObjectsWithTag("Monster");
        //     for (int i = 0; i < monsterList.Length; i++)
        //     {
        //         IMonster targetMonster = monsterList[i].GetComponent<IMonster>();

        //         if (targetMonster != null)
        //         {
        //             targetMonster.OnDamage(3000f, true, transform.position, 0);
        //         }
        //     }
        // }

    }
    public GameObject needCheckV; // 스킬 포인트 확인

    // 스킬포인트 need check 효과
    public void SetNeedCheckV(bool _check)
    {
        if (needCheckV != null)
        {
            needCheckV.SetActive(_check);
        }
        // else {
        //     Debug.Log("InGameUIManager/InGameUI 또는 DefaultUI 프리팹에 NeedCheckV 연결해주세요");
        // }
    }

    public void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked; // 커서를 화면 중앙에 고정
        Cursor.visible = false; // 커서를 숨김
        // ////Debug.Log("여기야!");
        playerController.IsStop(false);
        // playerController.enabled = true;
        characterInfo.EnableMovement();
        CameraControl.Inst.UnlockCamera();
        // CameraControl.Inst.EnableCameraMoving();
    }

    public void CursorUnLock()
    {
        Cursor.lockState = CursorLockMode.None;  // 커서 잠금 해제
        Cursor.visible = true;  // 커서 다시 보이게 설정
        playerController.IsStop(true);
        // playerController.enabled = false;
        characterInfo.DisableMovement();
        CameraControl.Inst.LockCamera();
        // CameraControl.Inst.DisableCameraMoving();
    }

    public void InitIngameUI()
    {
        inGameUI.GetComponent<InGameUI>().Init();
    }

    public void GameCountStart()
    {
        inGameUI.GetComponent<InGameUI>().GameStart(); //카운트다운이 끝나면 게임 시작
    }

    public void SetSkillTree(string className, CharacterInfo info)
    {
        skillTreeUI.transform.GetComponent<SkillTreeUI>().Init(className, info);

        //테스트: 스킬포인트 임시로 줬음
        //info.SetSkillPoint(11);
    }

    public void LoadSkill()
    {
        skillTreeUI.transform.GetComponent<SkillTreeUI>().LoadSkill();
    }

    // 몬스터 사망 시 실행;
    public void UpdateWhenMonsterDead(int _gold, int _exp, int _prevLevelExp, int _nextLevelExp, int _level)
    {
        inGameUIScript.UpdateWhenMonsterDead(_gold, _exp, _prevLevelExp, _nextLevelExp, _level);
    }

    public void ActiveDeadPanel(float remainTime)
    {
        StartCoroutine(ActiveDead(remainTime));
    }

    public IEnumerator ActiveDead(float remainTime)
    {
        deadPanel.SetActive(true);             // 사망 패널 활성화
        float timeLeft = remainTime;           // 남은 시간을 초기화

        while (timeLeft > 0)
        {
            deadRemainTimeText.text = Mathf.Ceil(timeLeft).ToString();  // 남은 시간 업데이트
            yield return new WaitForSeconds(1f);                   // 1초 대기
            timeLeft -= 1f;                                        // 시간 감소
        }

        deadPanel.SetActive(false);             // 패널 비활성화
    }

    public void SetRemainTime(string text)
    {
        deadRemainTimeText.text = "";
    }

    public void WarningBoss(float sec, string text)
    {
        GlobalSoundManager.Instance.PlayWarningSound();
        StartCoroutine(WarningBossForSecond(sec, text));
    }

    private IEnumerator WarningBossForSecond(float sec, string text)
    {
        bossWaringUIText.text = text;
        bossWaringUI.SetActive(true);
        yield return new WaitForSeconds(sec);
        bossWaringUI.SetActive(false);
    }

    public void ShowStageClearUI(string text)
    {
        GlobalSoundManager.Instance.PlayStageClearSound();
        stageClearUI.SetActive(true);
        stageClearText.text = text;
    }

    // TODO: Master가 결과를 뿌려주도록 변경
    // 게임 결과
    /// <param name="result">게임 결과 title에 띄울 값 승리 or 패배 or 다른 것들 </param>
    public void ShowResultUI(string result, float nexusHealth)
    {
        if (result == "패배")
        {
            ////Debug.Log("패배 Call");
            GlobalSoundManager.Instance.PlayFailureSound();
        }
        if (result == "승리")
        {
            ////Debug.Log("승리 Call");
            GlobalSoundManager.Instance.PlayWinningSound();
        }
        // 이부분 수정하려면 수정해도 됩니다.
        // isSettingUIActive = false;
        // isTowerBuyUIActive = false;
        // isSkillTreeUIActive = false;
        // isCursorLocked = false;
        isResultUIActive = true;

        // 인풋 막는 로직이 필요
        resultUI.gameObject.SetActive(true);
        resultUI.SetTitle(result);
        resultUI.SetPlayTime(inGameUIScript.GetTimer().GetIncreasingTimerText()); // TimerManager에서 뿌려주는 값으로 수정
        //float nexusHealth = CharacterManager.Inst.GetNexusHp();

        Debug.Log("결과에 보여줄 현재 넥서스의 남은 체력은: " + nexusHealth);
        resultUI.SetNexusInfo(nexusHealth);

        // 게임 일시정지
        // Time.timeScale = 0f;
    }

    public void ShowStageUI(string text, float dur)
    {
        StartCoroutine(ShowStageForSecond(text, dur));
    }

    public IEnumerator ShowStageForSecond(string text, float dur)
    {
        // Cursor.lockState = CursorLockMode.Locked; // 커서를 화면 중앙에 고정
        // CameraControl.Inst.LockCamera();

        stagePanel.SetActive(true);
        stageText.text = text;
        ////Debug.Log("스테이지 UI 활성화");

        // 게임 일시정지
        Time.timeScale = 0f;

        // 주어진 시간 동안 일시정지 유지
        yield return new WaitForSecondsRealtime(dur);

        // 게임 재개
        Time.timeScale = 1f;

        stagePanel.SetActive(false);

        CameraControl.Inst.UnlockCamera();

        inGameUIScript.GameStart();
    }

    public void SetNeedCheckSkillPoint(bool _check)
    {
        SetNeedCheckV(_check);
    }

}
