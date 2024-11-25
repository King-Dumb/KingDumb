using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UltimateClean;
using Photon.Pun;
using System;
using Unity.VisualScripting;
using static Cinemachine.DocumentationSortingAttribute;

public class InGameUI : MonoBehaviour
{
    private IngameManager igm;
    private GameObject myPlayer;
    private CharacterInfo playerInfo;
    private PlayerController pc;

    public Timer timer;
    private int gold;
    private int exp;
    private int prevLevelExp;
    private int nextLevelExp;
    private int level;

    private float hp;
    private float maxHP;

    private float maxSkillCooldown;
    private float maxUltimateCooldown;
    private float skillCooldown;
    private float ultimateCooldown;

    private float reviveTime;

    //######UI
    //프로필
    private string playerNickName;

    public List<Sprite> playerIconList;
    public List<Sprite> skillIconList;
    public List<Sprite> ultimateIconList;

    public TextMeshProUGUI playerName; //내 닉네임
    public Image playerProfile;
    public Image playerSkillIcon;
    public Image playerUltimateIcon;

    public TextMeshProUGUI goldText;    //골드 텍스트
    public TextMeshProUGUI timerText;    //타이머 텍스트

    //진행 바
    public Slider hpBar; //체력바
    public Slider expBar; //경험치바

    public TextMeshProUGUI hpText;

    //스킬아이콘
    public Image skillCoolDownUI;
    public Image ultimateCoolDownUI;
    private Color ultimateOriginalColor;
    private Color ultimateHighlightColor = Color.yellow; // 반짝일 때의 색상
    private float pulseSpeed = 0.5f; // 반짝이는 주기 (수치가 낮을 수록 천천히)
    private Coroutine ultimateGlowCoroutine;
    private bool isUltimateReady = false;

    //몬스터 진행도
    public Slider waveProgress;
    public List<RectTransform> wavePin;

    //웨이브 대기 타임
    public GameObject waitingTimePanel;
    public TextMeshProUGUI waitingTimeText;

    //스킬트리    
    public GameObject skillTreeUI; //스킬트리 UI                                    

    // public GameObject needCheckV; // 스킬 포인트 확인

    //내 레벨

    //몬스터 관련
    private MonsterGenerator monsterGenerator;

    //내 체력 //현재 체력, 최대 체력

    //내 경험치
    //스킬 쿨
    //궁극기 쿨

    void Awake()
    {
        //Debug.Log("InGameUI Awake");

        //Debug.Log("타이머 구독 처리");
        //if (TimerManager.Inst != null)
        //{
        //    TimerManager.Inst.OnIncreaseTimerSecondPassed += HandleIncreseTimer;
        //    TimerManager.Inst.OnDecreaseTimerSecondPassed += HandleDecreseTimer;
        //}

        //if (timer != null)
        //{
        //    timer.OnIncreaseTimerSecondPassed += HandleIncreseTimer;
        //    timer.OnDecreaseTimerSecondPassed += HandleDecreseTimer;
        //}
        igm = IngameManager.Inst;
    }


    private void OnDestroy()
    {
        if (TimerManager.Inst != null)
        {
            TimerManager.Inst.OnIncreaseTimerSecondPassed -= HandleIncreseTimer;
            TimerManager.Inst.OnDecreaseTimerSecondPassed -= HandleDecreseTimer;
        }
        //if (timer != null)
        //{
        //    timer.OnIncreaseTimerSecondPassed -= HandleIncreseTimer;
        //    timer.OnDecreaseTimerSecondPassed -= HandleDecreseTimer;
        //}
    }

    void Start()
    {
        //Debug.Log("InGameUI Start");

        ultimateOriginalColor = ultimateCoolDownUI.color;
        //Debug.Log("타이머 구독 처리");
        if (TimerManager.Inst != null)
        {
            TimerManager.Inst.OnIncreaseTimerSecondPassed += HandleIncreseTimer;
            TimerManager.Inst.OnDecreaseTimerSecondPassed += HandleDecreseTimer;
        }
    }

    public void Init()
    {
        // 현재는 씬을 넘어가면 정보가 마스터를 제외하고는 날라가기 때문에 인게임 내에서 유지해야하는 정보인 골드, 경험치, 레벨은 포톤뷰로 생성되는 InGameManager에서 관리하고 업데이트가 발생할 시에만 마스터가 뿌려줄예정
        playerNickName = PhotonNetwork.LocalPlayer.NickName;
        myPlayer = GameManager.Instance.localPlayer;

        if (myPlayer != null)
        {
            pc = myPlayer.transform.GetComponent<PlayerController>();
            SetPlayerIcon(GameManager.Instance.playerClassCode);
        }

        playerInfo = myPlayer.transform.GetComponent<CharacterInfo>();

        UpdatePlayerName(IngameManager.Inst.CurLevel);

        InitPlayerInfo();

        // SkillTree UI 설정
        IngameUIManager.Inst.SetSkillTree(GameManager.Instance.playerClassName, playerInfo);
        
        InitMonsterGeneratorUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInfo != null && pc != null && monsterGenerator != null)
        {
            //체력
            float curHp = playerInfo.GetHP();

            if (hp != curHp)
            {
                // 피격 효과
            }
            hp = curHp;
            maxHP = playerInfo.GetMaxHP();
            ConvertValue(hpBar, playerInfo.GetHP(), playerInfo.GetMaxHP());
            hpText.text = hp.ToString("F0");

            //쿨다운            
            maxSkillCooldown = pc.skillCooldownDuration;
            skillCooldown = pc.skillRemainingCooldown;

            //궁극기 
            maxUltimateCooldown = pc.ultimateCooldownDuration;
            ultimateCooldown = pc.ultimateRemainingCooldown;
            GlowUltimateUI();

            skillCoolDownUI.fillAmount = (maxSkillCooldown - skillCooldown) / maxSkillCooldown;
            ultimateCoolDownUI.fillAmount = (maxUltimateCooldown - ultimateCooldown) / maxUltimateCooldown;

            //사망 패널

            //몬스터 진행도
            //Debug.Log("킬카운트 : "+ monsterGenerator.killCount );
            //Debug.Log("총카운트 : " + monsterGenerator.maxCount);
            waveProgress.value = (float)monsterGenerator.killCount / (float)monsterGenerator.maxCount;
        }
    }

    public Timer GetTimer()
    {
        return timer;
    }

    private void SetPlayerIcon(int classCode)
    {
        //switch (classCode)
        //{
        //    case 0: //전사                
        //        break;
        //    case 1: //궁수
        //        break;
        //    case 2: //마법사
        //        break;
        //    case 3: //사제
        //        break;

        //}

        playerProfile.sprite = playerIconList[classCode];
        playerSkillIcon.sprite = skillIconList[classCode];
        playerUltimateIcon.sprite = ultimateIconList[classCode];

        RectTransform imageRect = playerProfile.GetComponent<RectTransform>();
        imageRect.localScale = new Vector3(0.9f, 0.9f, 0.9f);

        imageRect = playerSkillIcon.GetComponent<RectTransform>();
        imageRect.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        imageRect = playerUltimateIcon.GetComponent<RectTransform>();
        imageRect.localScale = new Vector3(1.5f, 1.5f, 1.5f);


    }

    private void SetSkillMaxCoolDown()
    {

    }

    private void GlowUltimateUI()
    {
        // 궁극기 준비 상태 변경 감지
        if (ultimateCoolDownUI.fillAmount == 1)
        {
            if (!isUltimateReady)
            {
                isUltimateReady = true;
                StartUltimateGlow(); // 한 번만 호출
            }
        }
        else
        {
            if (isUltimateReady)
            {
                isUltimateReady = false;
                StopUltimateGlow(); // 한 번만 호출
            }
        }
    }

    void StartUltimateGlow()
    {
        ultimateGlowCoroutine = StartCoroutine(UltimateGlowEffect());
    }

    void StopUltimateGlow()
    {
        if (ultimateGlowCoroutine != null)
        {
            StopCoroutine(ultimateGlowCoroutine);
            ultimateGlowCoroutine = null; // 핸들 초기화
        }
        ultimateCoolDownUI.color = ultimateOriginalColor; // 원래 색상 복원
    }

    IEnumerator UltimateGlowEffect()
    {
        while (true)
        {
            float lerpValue = Mathf.PingPong(Time.time * pulseSpeed, 1f); // 0에서 1 사이로 반복
            ultimateCoolDownUI.color = Color.Lerp(ultimateOriginalColor, ultimateHighlightColor, lerpValue);
            yield return null;
        }
    }

    private void SetUltimateMaxCoolDown()
    {

    }

    private void UpdateGold(int _gold)
    {
        //인게임 매니저에서 골드 정보 받아오기
        //gold = IngameManager.Inst.TotalPlayerGold;
        gold = _gold;
        goldText.text = gold + "";
    }
    private void UpdateExp(int _exp, int _prevLevelExp, int _nextLevelExp)
    {
        //exp = IngameManager.Inst.TotalPlayerExp;
        //prevLevelExp = IngameManager.Inst.PrevTargetPlayerExp;
        //nextLevelExp = IngameManager.Inst.TargetPlayerExp;
        exp = _exp;
        prevLevelExp = _prevLevelExp;
        nextLevelExp = _nextLevelExp;

        //Debug.Log($"exp:{exp}, prevExp:{prevLevelExp}, nextExp:{nextLevelExp}");
        ConvertValue(expBar, exp - prevLevelExp, nextLevelExp - prevLevelExp);
    }

    private void UpdatePlayerName(int _level)
    {
        //level = IngameManager.Inst.CurLevel;

        level = _level;
        playerName.text = playerNickName + "[LV " + level + "]";
    }

    public void UpdatePlayerShareInfo(int _gold, int _exp, int _prevLevelExp, int _nextLevelExp, int _level)
    {
        gold = _gold;
        goldText.text = gold + "";

        exp = _exp;
        prevLevelExp = _prevLevelExp;
        nextLevelExp = _nextLevelExp;
        ConvertValue(expBar, exp - prevLevelExp, nextLevelExp - prevLevelExp);

        level = _level;
        playerName.text = playerNickName + "[LV " + level + "]";
    }

    private void HandleIncreseTimer()
    {
        timerText.text = timer.GetIncreasingTimerText();
    }

    private void HandleDecreseTimer(bool isZero)
    {
        waitingTimeText.text = "게임 시작까지 " + timer.GetDecreasingTimerText();

        if (isZero)
        {
            timer.ActiveIncreseTimer(true);
            timer.ActiveDecreasingTimer(false);
            IngameManager.Inst.ToggleGenerate();
            waitingTimePanel.SetActive(false);

            GlobalSoundManager.Instance.PlayBattleStartSound();
        }
    }

    //float 값을 백분율로 환산
    private void ConvertValue(Slider slider, float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
    }

    private void InitPlayerInfo()
    {
        //Debug.Log("InitPlayerInfo");

        hp = playerInfo.GetHP();
        maxHP = playerInfo.GetMaxHP();
        gold = IngameManager.Inst.TotalPlayerGold;
        exp = IngameManager.Inst.TotalPlayerExp;
        int prevLevelExp = IngameManager.Inst.PrevTargetPlayerExp;
        int nextLevelExp = IngameManager.Inst.TargetPlayerExp;
        level = IngameManager.Inst.CurLevel;
        //exp = playerInfo.GetExp();
        //level = playerInfo.GetLevel();
        reviveTime = playerInfo.GetReviveTime();

        // 골드
        UpdateGold(gold);
        //경험치
        UpdateExp(exp, prevLevelExp, nextLevelExp);
        // 레벨
        UpdatePlayerName(level);
    }
    public void GameStart()
    {
        waitingTimePanel.SetActive(true);
        timer.SetDecreseStartTime(TimerManager.Inst.decreaseStartTime);
        timer.ActiveDecreasingTimer(true);
    }

    private void InitMonsterGeneratorUI()
    {
        // level이 generator가 아닌 플레이어 레벨과 같아서 변경
        monsterGenerator = MonsterGenerator.Inst;
        int generatorLevel = monsterGenerator.Level;
        //Debug.Log("InitMonsterGeneratorUI의 현재 레벨: " + generatorLevel);

        //웨이브 정보에 따른 핀을 배치한다.
        RectTransform sliderRect = waveProgress.GetComponent<RectTransform>();

        float sliderWidth = sliderRect.rect.width;

        float wave1 = (float)monsterGenerator.flagCount[generatorLevel, 0] / (float)monsterGenerator.maxCount;
        float wave2 = (float)monsterGenerator.flagCount[generatorLevel, 1] / (float)monsterGenerator.maxCount;

        //Debug.Log("1: " + wave1 + "2: " + wave2);

        float normalValue1 = (wave1 - waveProgress.minValue) / (waveProgress.maxValue - waveProgress.minValue);
        float normalValue2 = (wave2 - waveProgress.minValue) / (waveProgress.maxValue - waveProgress.minValue);

        float pinPos1 = sliderWidth * normalValue1 - (sliderWidth / 2);
        float pinPos2 = sliderWidth * normalValue2 - (sliderWidth / 2);

        wavePin[0] = GameObject.Find("WavePin_1").GetComponent<RectTransform>();
        wavePin[1] = GameObject.Find("WavePin_2").GetComponent<RectTransform>();

        wavePin[0].anchoredPosition = new Vector2(pinPos1, wavePin[0].anchoredPosition.y);
        wavePin[1].anchoredPosition = new Vector2(pinPos2, wavePin[1].anchoredPosition.y);
    }

    // 몬스터 사망 시 실행;
    public void UpdateWhenMonsterDead(int _gold, int _exp, int _prevLevelExp, int _nextLevelExp, int _level)
    {
        // 골드
        UpdateGold(_gold);
        //경험치
        UpdateExp(_exp, _prevLevelExp, _nextLevelExp);
        // 레벨
        UpdatePlayerName(_level);
    }
    
    // // 스킬포인트 need check 효과
    // public void SetNeedCheckV(bool _check)
    // {
    //     if (needCheckV != null)
    //     {
    //         needCheckV.SetActive(_check);
    //     }
    //     // else {
    //     //     Debug.Log("InGameUIManager/InGameUI 또는 DefaultUI 프리팹에 NeedCheckV 연결해주세요");
    //     // }
    // }

    // 피격 효과 연출

}
