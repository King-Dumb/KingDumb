using Cinemachine;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using WebSocketSharp;
using static Cinemachine.DocumentationSortingAttribute;
using Hashtable = ExitGames.Client.Photon.Hashtable;


//게임 플레이의 진행을 담당할 중계자 역할
public class IngameManager : MonoBehaviourPun
{
    public static IngameManager Inst;

    private string monsterGeneratorPath = "";

    public GameObject monsterGeneratorPrefab;
    private MonsterGenerator monsterGenerator;

    private int totalPlayerExp;
    private int totalPlayerGold;
    private int prevTargetPlayerExp = 0;
    private int targetPlayerExp = 1;
    public int TotalPlayerExp => totalPlayerExp; // 읽기 전용
    public int TotalPlayerGold => totalPlayerGold; // 읽기 전용
    public int PrevTargetPlayerExp => prevTargetPlayerExp;
    public int TargetPlayerExp => targetPlayerExp;

    public float pauseDuration = 2f; // 일시정지 지속 시간 (초 단위로 설정)

    private int curLevel;
    public int CurLevel => curLevel;
    // 레벨업 이벤트 정의
    public event Action<int> OnLevelUpEvent;

    //#######################오브젝트풀
    public ObjectPool objPool; 

    public IPunPrefabPool objPoolPun; //네트워크 오브젝트풀
                                      //###################################

    //경험치 테이블 배열로 10레벨까지 MAX
    // REAL: 실제 사용
    private int[] expTable = new int[]
    {
        0,  // index 0
        0,  // 레벨 1
        15, // 레벨 2 
        33, // 레벨 3 
        55, // 레벨 4 
        81, // 레벨 5 
        112,// 레벨 6 
        149,// 레벨 7 
        194,// 레벨 8 
        247,// 레벨 9 
        312,// 레벨 10
        int.MaxValue
    };

    // TEST: 테스트용 경험치 테이블
    //private int[] expTable = new int[]
    //{
    //        0,  // index 0
    //        0,  // 레벨 1
    //        1, // 레벨 2 
    //        3, // 레벨 3 
    //        5, // 레벨 4 
    //        8, // 레벨 5 
    //        11,// 레벨 6 
    //        14,// 레벨 7 
    //        19,// 레벨 8 
    //        24,// 레벨 9 
    //        31,// 레벨 10
    //        int.MaxValue
    //};

    private bool _isGameOver = false; // 게임이 끝났는지 여부

    //코루틴 관리
    public Dictionary<GameObject, Coroutine> coroutineDict = new Dictionary<GameObject, Coroutine>();
    public bool isGameOver
    {
        get
        {
            return _isGameOver; // public은 읽기만 가능;
        }
        private set { _isGameOver = value; } 
    }

    void Awake()
    {
        //Debug.Log("InGameManager Awake");

        //싱글톤 선언
        if (Inst == null)
        {
            Inst = this;
            targetPlayerExp = expTable[curLevel + 1];

            //Awake 시 초기화
            totalPlayerGold = GameManager.Instance.totalPlayerGold;
            totalPlayerExp = GameManager.Instance.totalPlayerExp;
            curLevel = GameManager.Instance.curLevel;
            //GameManager.Instance.SaveTotalGoldExp(GameManager.Instance.initialGold, GameManager.Instance.initialExp, GameManager.Instance.initialLevel);
        }
        else
        {
            Destroy(gameObject);
        }

        //오브젝트풀 초기화
        objPool = ObjectPool.Instance;
        PhotonNetwork.PrefabPool = new PhotonObjectPool();
        objPoolPun = PhotonNetwork.PrefabPool;
    }

    void Start()
    {
        //Debug.Log("InGameManager Start");

        // 인게임 UI 매니저가 있으면 => 인게임 씬이면
        if (IngameUIManager.Inst != null)
        {
            SetStagePanel();


            if (PhotonNetwork.IsMasterClient)
            {
            // 골드, 경험치 초기화 (스테이지 시작 시);
            totalPlayerGold = GameManager.Instance.totalPlayerGold;
            totalPlayerExp = GameManager.Instance.totalPlayerExp;
                curLevel = GameManager.Instance.curLevel;
                //Debug.Log($"골드{totalPlayerGold}, 경험치{totalPlayerExp}, 초기화 RPC 호출" );
                photonView.RPC("UpdatePlayerSharedInfo", RpcTarget.All, totalPlayerGold, totalPlayerExp, expTable[curLevel], expTable[curLevel + 1], curLevel);
            }
            // 골드, 경험치 초기화 (방을 나가거나 게임 종료할 경우 대비)
            GameManager.Instance.SaveTotalGoldExp(GameManager.Instance.initialGold, GameManager.Instance.initialExp, GameManager.Instance.initialLevel);
        }
    }

    public MonsterGenerator CreateMonsterGenerator(int level)
    {
        if (!PhotonNetwork.IsConnected)
            return null;

        //(마스터)몬스터 생성기 작동
        if (PhotonNetwork.IsMasterClient)
        {
            monsterGenerator = PhotonNetwork.Instantiate(monsterGeneratorPath + monsterGeneratorPrefab.name, Vector3.zero, Quaternion.identity)
                .GetComponent<MonsterGenerator>();

            //monsterGenerator.SetLevelBroadcast(level);

            // SetLevel과 Init을 합침 (두 RPC의 Call이 순서가 꼬이는 경우가 발생했었음, 앞으로도 같이 묶을 수 있는 RPC Call은 묶을 예정)
            monsterGenerator.Init(level);
            monsterGenerator.InitBroadcast(level);

            return monsterGenerator;
        }
        return null;
    }

    public void AddGoldAndExp(int gold, int experience)
    {
        totalPlayerGold += gold;
        totalPlayerExp += experience;

        photonView.RPC("UpdatePlayerSharedInfo", RpcTarget.All, totalPlayerGold, totalPlayerExp, expTable[curLevel], expTable[curLevel + 1], curLevel);
    }

    [PunRPC]
    public void UpdatePlayerSharedInfo(int _gold, int _exp, int _prevLevelExp, int _nextLevelExp, int _level)
    {
        totalPlayerGold = _gold;
        totalPlayerExp = _exp;
        curLevel = _level;

        if (_exp >= expTable[_level + 1])
        {
            curLevel++;
            OnLevelUp();
        }

        IngameUIManager.Inst.UpdateWhenMonsterDead(_gold, _exp, expTable[curLevel], expTable[curLevel+1], curLevel);
    }

    public void OnLevelUp()
    {
        ////Debug.Log($"레벨이 올랐습니다!! 현재 레벨: {curLevel}, 새로운 목표:" + expTable[curLevel]);
        // 레벨 업 이벤트 호출
        OnLevelUpEvent?.Invoke(curLevel);
        prevTargetPlayerExp = expTable[curLevel];
        targetPlayerExp = expTable[curLevel + 1];
        IngameUIManager.Inst.SetNeedCheckSkillPoint(true);
    }

    public void GameCountStart()
    {
        IngameUIManager.Inst.GameCountStart();
    }
    public void ToggleGenerate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //Debug.Log("monster Generator : " + monsterGenerator.isOn);
            monsterGenerator.isOn = !monsterGenerator.isOn;
            //Debug.Log("monster Generator : " + monsterGenerator.isOn);
        }
    }


    public void SetStage(float sec)
    {
        photonView.RPC("SetStageRpc", RpcTarget.All, sec);
    }

    [PunRPC]
    public void SetStageRpc(float sec)
    {
        StartCoroutine(SetStageForSecond(sec));
    }


    public IEnumerator SetStageForSecond(float sec)
    {
        // 스테이지 변경 및 게임 종료(승리) 처리
        string sceneName = SceneManager.GetActiveScene().name; // 현재 씬
        switch (sceneName)
        {
            case "InGame":
                // 스테이지 이동 전 게임 클리어 창 띄우기
                IngameUIManager.Inst.ShowStageClearUI("스테이지 1 클리어!");
                SavePlayerSkillInfo();
                GameManager.Instance.SaveTotalGoldExp(totalPlayerGold, totalPlayerExp, curLevel);
                GameManager.Instance.SaveTotalTime(TimerManager.Inst.increaseMin, TimerManager.Inst.increaseSec);
                yield return new WaitForSeconds(sec);
                // 골드, 경험치 저장                
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonManager.SendEvent(2); // 2스테이지로 이동
                }
                break;
            case "InGame_2S":
                SavePlayerSkillInfo();
                IngameUIManager.Inst.ShowStageClearUI("스테이지 2 클리어!");
                GameManager.Instance.SaveTotalGoldExp(totalPlayerGold, totalPlayerExp, curLevel);
                GameManager.Instance.SaveTotalTime(TimerManager.Inst.increaseMin, TimerManager.Inst.increaseSec);
                yield return new WaitForSeconds(sec);                
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonManager.SendEvent(3); // 3스테이지로 이동
                }
                break;
            case "InGame_3S": // 게임 종료(승리)
                              // 게임 종료 패널 띄우기
                if (PhotonNetwork.IsMasterClient)
                {
                    GameWin();
                }
                break;
        }
    }

    public PurchaseType BuyTower(int purchaseGold)
    {
        totalPlayerGold -= purchaseGold;
        photonView.RPC("UpdatePlayerSharedInfo", RpcTarget.All, totalPlayerGold, totalPlayerExp, expTable[curLevel], expTable[curLevel + 1], curLevel);
        return PurchaseType.Success;
    }

    public enum PurchaseType
    {
        Success = 0,         // 구매 성공
        NotEnoughGold = 1,   // 골드 부족
        MaxLevelReached = 2, // 최대 레벨 도달 (더 이상 업그레이드 불가)
        InvalidSelection = 3 // 선택이 올바르지 않음 (예: 이미 존재하는 타워)
    }

    void SetStagePanel()
    {
        string sceneName = SceneManager.GetActiveScene().name; // 현재 씬
        string stageText = "";

        // 스테이지 표시 후
        switch (sceneName)
        {
            case "InGame":
                stageText = "스테이지 1";
                break;
            case "InGame_2S":
                stageText = "스테이지 2";
                break;
            case "InGame_3S":
                stageText = "스테이지 3";
                break;
        }

        //Debug.Log("스테이지 활성화 메서드 호출");
        IngameUIManager.Inst.ShowStageUI(stageText, pauseDuration);
    }

    // TODO: 넥서스 파괴와 연동
    public void GameOver()
    {
        if (isGameOver) return;

        PlayerStatisticsManager.Instance.SetPlayerStatisticsWhenGameEnded();
        // 그밖의 게임오버 처리
        photonView.RPC("GameOverBroadcast", RpcTarget.All);
    }

    [PunRPC]
    public void GameOverBroadcast()
    {
        if (!isGameOver)
        {
            isGameOver = true;
            Invoke(nameof(GameOverWithDelay), 1.5f);
        }
    }

    void GameOverWithDelay()
    {
        // Time.timeScale = 0f; // 시간을 멈춤
        IngameUIManager.Inst.ShowResultUI("패배", 0f);
    }

    public void GameWin()
    {
        if (isGameOver) return;

        PlayerStatisticsManager.Instance.SetPlayerStatisticsWhenGameEnded();
        float nexusHealth = CharacterManager.Inst.GetNexusHp();
        // 그밖의 게임오버 처리
        photonView.RPC("GameWinBroadcast", RpcTarget.All, nexusHealth);
    }

    [PunRPC]
    public void GameWinBroadcast(float nexusHealth)
    {
        _nexusHealth = nexusHealth;

        if (!isGameOver)
        {
            isGameOver = true;
            
            Invoke(nameof(GameWinWithDelay), 1.5f);
        }
    }

    private float _nexusHealth;

    void GameWinWithDelay()
    {
        IngameUIManager.Inst.ShowResultUI("승리", _nexusHealth);
    }


    //스킬포인트,
    void SavePlayerSkillInfo()
    {
        int maxSkillLevel = GameConfig.maxSkillLevel;
        CharacterInfo info = GameManager.Instance.localPlayer.transform.GetComponent<CharacterInfo>();
        GameManager.Instance.savedSkillNode = new bool[maxSkillLevel+1];

        GameManager.Instance.savedPlayerSkillPoint = info.GetSkillPoint();
        
        //깊은복사
        for(int i= 1; i<= maxSkillLevel; i++)
        {
            GameManager.Instance.savedSkillNode[i] = info.savedSkillNode[i];

            if (info.savedSkillNode[i] == true)
            {
                //Debug.Log(i + "번스킬 저장완료");
            }
        }
    }

    //캐릭터 인포로 이동
    //public void LoadPlayerSkillInfo()
    //{
    //    int maxSkillLevel = GameConfig.maxSkillLevel;
    //    CharacterInfo info = GameManager.Instance.localPlayer.transform.GetComponent<CharacterInfo>();
    //    bool[] prevData = GameManager.Instance.savedSkillNode;

    //    //스킬포인트 세팅
    //    info.SetSkillPoint(GameManager.Instance.savedPlayerSkillPoint);

    //    // CharacterInfo에 복사
    //    info.savedSkillNode = new bool[maxSkillLevel + 1];
    //    for (int i = 0; i <= maxSkillLevel; i++)
    //    {
    //        info.savedSkillNode[i] = prevData[i];
    //    }
    //}

    public void ClearSkillInfo()
    {
        //데이터 초기화
        GameManager.Instance.savedPlayerSkillPoint = 0;
        GameManager.Instance.savedSkillNode = null;
    }
}
