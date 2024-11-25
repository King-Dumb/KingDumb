using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Cinemachine.DocumentationSortingAttribute;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;
//using static UnityEditor.PlayerSettings;

public class MonsterGenerator : MonoBehaviourPun
{
    public static MonsterGenerator Inst;

    private string monsterPath = "Monster/";
    enum SpawnPattern
    {
        Normal,//1마리
        Cross,//N방
        Corps, //군단, 한곳에 몰려서 생성
        Boss,
    }
    private int spawnPatternSize;

    public bool isOn { get; set; } //작동 스위치

    //게임에 사용할 몬스터 프리팹 저장
    public List<GameObject> monsterTypeList = new List<GameObject>();
    public GameObject bossMonsterPrefab;

    public GameObject target; //추적 대상
    public float spawnRadius; //생성 반지름    

    
    private float[] spawnDuration = {2f, 1f, 0.5f, 1f}; //생성 쿨타임
    //몬스터가 꽉 찼을때는 절반정도 사라졌을 때 재생성
    private int[] crossCount = { 1, 2, 4, 1 }; //초기 교차 생성 마릿수 1~N
    private int[] corpsCount = { 2, 4, 6, 2 }; //초기 군단 생성 마릿수 1~N
    private int[] maxCrossCount = { 12, 18, 24, 1 }; //교차 최대생성 마릿수
    private int[] maxCorpsCount = { 10, 15, 20, 1 }; //군단 최대 생성 마릿수
    private float[] upgradeCountTime = { 10f, 5f, 3f, 10000f }; //군단 최대마릿수 증가 시간

    // REAL: 실제 사용
    private int[] bigWaveCount = { 10, 20, 30, 1 }; //빅웨이브 동시 생성 마릿수   
    public int[,] flagCount = { { 10, 20, 30, 100000 }, { 20, 40, 60, 100000 }, { 30, 60, 90, 100000 } }; //분기점 마릿수 (3wave)

    // TEST: 테스트용 몬스터 생성기
    //private int[] bigWaveCount = { 1, 1, 1, 1 }; //빅웨이브 동시 생성 마릿수    
    //public int[,] flagCount = { { 1, 1, 1, 100000 }, { 1, 1, 1, 100000 }, { 1, 1, 1, 100000 } }; //분기점 마릿수 (3wave)

    private float[,] randomUpTime = { { 20f, 40f, 60f, 80f, 100f, 100000f }, { 10f, 20f, 30f, 35f, 40f, 100000f }, { 5f, 10f, 15f, 20f, 25f, 100000f } };
    private int randomIdx = 1; //몬스터 종류를 정할 랜덤함수
    private int maxRandomIdx;

    private int flagIdx;
    private float curUpgradeCountTime;

    private SpawnPattern pattern;

    private float curTime = 0f; //현재 쿨타임
    private float gameTime = 0f;// 실제 지난 시간 

    public int maxCount; // 몬스터 토탈 잡을 마릿수 (flagCount)

    private int totalCount; //총 생성된 마릿수
    private int currentCount; //맵에 존재하는 몬스터 마릿수    
    public int killCount; //웨이브 판단용 잡은 마릿수    

    public int maxSpawnCount = 50; //최대 생성 마릿수

    private bool isOneType; //대량생성될때 몬스터 한 마리만 할지 결정
    private bool isBigWave; //true라면 Cross 패턴으로 고정하고 대량생성한다.    

    //테스트용으로 가지고 있는 몬스터 프리팹 리스트        
    public List<GameObject> testList = new List<GameObject>();

    private List<GameObject> allMonsterList = new List<GameObject>();

    public event Action<MonsterBase> OnMonsterSpawned; // 몬스터 생성 시 알림
    public GameObject PortalPrefab;

    private int level = 0;
    public int Level
    {
        get
        {
            return level; // public은 읽기만 가능;
        }
        private set
        { level = value; } 
    }


    private void Awake()
    {
        //싱글톤 선언
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(gameObject);
        }        
    }

    void Start()
    {

    }

    public void InitBroadcast(int level)
    {
        photonView.RPC("Init", RpcTarget.Others, level);
    }

    [PunRPC]
    public void Init(int _level) {
        //Debug.Log("MonsterGeneratorInit Call");
        level = _level - 1;
        currentCount = 0; //맵에 존재하는 몬스터 마릿수    
        killCount = 0; //웨이브 판단용 잡은 마릿수    

        //카운트에 빅웨이브 카운트를 더함
        flagIdx = 0;
        flagCount[level, 1] = flagCount[level, 0] + flagCount[level, 1] + bigWaveCount[level];
        flagCount[level, 2] = flagCount[level, 1] + flagCount[level, 2] + bigWaveCount[level];

        if (level < 2)
        {
            maxCount = flagCount[level, 2] + bigWaveCount[level];
        }
        else
        {
            maxCount = flagCount[level, 2] + 1;
        }                
        curTime = 0f;
        pattern = SpawnPattern.Normal;
        pattern = SpawnPattern.Normal;
        spawnPatternSize = (int)SpawnPattern.Boss;
        gameTime = 0f;
        killCount = 0;
        curUpgradeCountTime = 0f;
        randomIdx = 1;
        maxRandomIdx = monsterTypeList.Count;

        isOn = false;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (isOn)
        {
            gameTime += Time.deltaTime;
            curUpgradeCountTime += Time.deltaTime;

            //활성화 된 동안 시간이 지날수록 한 턴에 최대 생성 마릿수 증가
            if (curUpgradeCountTime >= upgradeCountTime[level])
            {
                curUpgradeCountTime = 0f;
                crossCount[level] = Mathf.Min(crossCount[level] + 1, maxCrossCount[level]);
                corpsCount[level] = Mathf.Min(corpsCount[level] + 1, maxCorpsCount[level]);
            }

            if((gameTime > randomUpTime[level, randomIdx-1]) && randomIdx != maxRandomIdx)
            {
                ++randomIdx;
                //Debug.Log("랜덤 마릿수 증가" + randomIdx);
            }


            //빅웨이브 상태거나 최대 마릿수 초과시에는 생성되지 않음
            CheckBigWave();

            if (isBigWave)
                return;

            curTime += Time.deltaTime;


            if (curTime >= spawnDuration[level])
            {
                curTime = 0f;
                SetSpawnPattern();
                //pattern = SpawnPattern.Corps;
                isOneType = isOneMonsterType();
                int count = SetMonsterRandomCount();
                SpawnMonster(count);
            }
        }

        //
        //몬스터 테스트용
        {
            //if (Input.GetKeyDown(KeyCode.F6))
            //{
            //    //Debug.Log("몬스터생성1");
            //    CreateTestMonster(0);
            //}
            //if (Input.GetKeyDown(KeyCode.F7))
            //{
            //    //Debug.Log("몬스터생성2");
            //    CreateTestMonster(1);
            //}
            //if (Input.GetKeyDown(KeyCode.F8))
            //{
            //    //Debug.Log("몬스터생성3");
            //    CreateTestMonster(2);
            //}
            //if (Input.GetKeyDown(KeyCode.F9))
            //{
            //    //Debug.Log("몬스터생성4");
            //    CreateTestMonster(3);
            //}
            //if (Input.GetKeyDown(KeyCode.F10))
            //{
            //    //Debug.Log("몬스터생성5");
            //    CreateTestMonster(4);
            //}
            // if (Input.GetKeyDown(KeyCode.F11))
            // {
            //     //Debug.Log("보스 몬스터생성");
            //     CreateTestMonster(5);
            // }
            //if (Input.GetKeyDown(KeyCode.F12))
            //{
            //    isOn = !isOn;
            //    //Debug.Log("몬스터 생성기 작동여부 : " + isOn);
            //}
        }
    }

    //일반 패턴
    private void SetSpawnPattern()
    {
        pattern = (SpawnPattern)Random.Range(0, spawnPatternSize);
    }

    //분기점에 따른 빅웨이브
    private void CheckBigWave()
    {
        //빅웨이브라면 몬스터 대량 생성하고
        //플래그(빅웨이브 조건): 몬스터 0마리여야함

        //이미 빅웨이브 상태라면 남아있는 몬스터를 체크하고  0마리라면 빅웨이브를 끄고 인덱스를 올린다.
        if (!isBigWave)
        {
            ////Debug.Log(flagCount[flagIdx]);
            if (killCount >= flagCount[level, flagIdx] && currentCount == 0)
            {
                //Debug.Log("빅웨이브!");
                StartCoroutine(BigWave());
            }
        }
        else
        {
            if (currentCount == 0)
            {
                //Debug.Log("빅웨이브 끝남");
                isBigWave = false;

                if (flagIdx == 3)
                {
                    //Debug.Log("게임 끝!");
                    isOn = false;
                    //level++;

                    IngameManager.Inst.SetStage(3f);
                }
            }
        }
    }

    private bool isOneMonsterType()
    {
        return Random.Range(0, 1) == 1 ? true : false;
    }

    //몬스터 생성될 마릿수를 정한다.
    private int SetMonsterRandomCount()
    {
        int count = 1;
        if (pattern == SpawnPattern.Cross)
        {
            count = Random.Range(1, crossCount[level] + 1);
        }
        else if (pattern == SpawnPattern.Corps)
        {
            count = Random.Range(1, corpsCount[level] + 1);
        }

        return count;
    }

    public Vector3 GetRandomPos()
    {
        Vector2 pos = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = new Vector3(pos.x, 0, pos.y);
        return spawnPosition;
    }

    private void SpawnMonster(int spawnCount)
    {
        //원 둘레 중 랜덤한 위치 생성        
        Vector3 spawnPosition = GetRandomPos();

        switch (pattern)
        {
            case SpawnPattern.Normal:
                ////Debug.Log("노말 1마리 생성");
                CrossSpawn(spawnPosition, 1);
                break;
            case SpawnPattern.Cross:
                ////Debug.Log("랜덤" + spawnCount + " 마리 생성");
                CrossSpawn(spawnPosition, spawnCount);
                break;
            case SpawnPattern.Corps:
                ////Debug.Log("군단" + spawnCount + " 마리 생성");
                CorpSpawn(spawnPosition, spawnCount);
                break;
            case SpawnPattern.Boss:
                //Debug.Log("보스 등장");
                CreateBossObj(spawnPosition);
                //isOn = false;
                break;
            default:
                break;
        }
    }

    void CreateMonsterObj(Vector3 pos, Quaternion rot, int idx = 0)
    {

        //나중에 포톤 생성으로 변경
        GameObject obj = null;
        //obj = Instantiate(monsterTypeList[idx], pos, rot);
        obj = PhotonNetwork.Instantiate("Monster/" + monsterTypeList[idx].name, pos, rot);
        photonView.RPC("CreatePortal", RpcTarget.All, pos, rot);
        currentCount++;
        totalCount++;

        //if (DebugManager.Instance != null)
        //{
        //    DebugManager.Instance.SetInfoText("Cur: " + currentCount + "\nkill : " + killCount);
        //}

        // 몬스터를 구독자에게 알려 이를 
        MonsterBase monster = obj.GetComponent<MonsterBase>();
        //OnMonsterSpawned?.Invoke(monster); // 새 몬스터를 구독자에게 알림
    }

    [PunRPC]
    public void CreatePortal(Vector3 position, Quaternion rotation)
    {
        ////Debug.Log($"Portal position: {position}, rotation: {rotation}");
        Vector3 portalPosition = new Vector3(position.x, position.y + 1, position.z);
        GameObject portal = GameManager.Instance.Instantiate(PortalPrefab.name, portalPosition, rotation);
        GameManager.Instance.Destroy(portal, 4.0f);
    }

    void CreateBossObj(Vector3 startPos)
    {
        Vector3 direction = target.transform.position - startPos;
        float rotY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        Quaternion rot = Quaternion.Euler(0, rotY, 0);

        //나중에 포톤 생성으로 변경
        GameObject obj = null;
        obj = PhotonNetwork.Instantiate("Monster/" + bossMonsterPrefab.name, startPos, rot);

        currentCount++;
        totalCount++;
        //테스트
        //TestMonScript sc = obj.GetComponent<TestMonScript>();
        //if (sc != null)
        //    sc.SetTarget(target);

        //if (DebugManager.Instance != null)
        //{
        //    DebugManager.Instance.SetInfoText("Cur: " + currentCount + "\nkill : " + killCount);
        //}
    }

    void CrossSpawn(Vector3 startPos, int spawnCount)
    {
        int randomMonsterIdx = Random.Range(0, randomIdx);

        //마릿수에따라 각도를 나눈다.
        float angle = 360 / (float)spawnCount;
        float startAngle = 0f;

        //세타값 구함
        Vector3 direction = startPos - transform.position;
        float theta = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        for (int i = 0; i < spawnCount; i++)
        {
            //생성 마릿수 초과하면 리턴
            if (isUnCreatableMonster()) { return; }

            if (!isOneType)
            {
                randomMonsterIdx = Random.Range(0, randomIdx);
            }

            //라디안으로 변환            
            startAngle = (theta + (i * angle)) * Mathf.Deg2Rad;

            float x = transform.position.x + spawnRadius * Mathf.Cos(startAngle);
            float y = transform.position.y + spawnRadius * Mathf.Sin(startAngle);

            Vector3 newPos = new Vector3(x, 0f, y);

            Vector3 rotDirection = (target.transform.position - newPos).normalized;
            Quaternion lookAt = Quaternion.LookRotation(rotDirection);
            CreateMonsterObj(newPos, lookAt, randomMonsterIdx);
        }
    }

    void CorpSpawn(Vector3 startPos, int spawnCount)
    {
        //최대 마릿수 생성 혹은 빅웨이브 이상으로 만들어지면 
        if (isUnCreatableMonster())
            return;

        int randomMonsterIdx = Random.Range(0, randomIdx);

        //마릿수에 따라 Nx2 로 배치한다.
        float spacing = 5f;

        if (spawnCount == 1)
        {
            Vector3 direction = target.transform.position - startPos;
            float rotY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            Quaternion rot = Quaternion.Euler(0, rotY, 0);

            CreateMonsterObj(startPos, rot, randomMonsterIdx);
            return;
        }

        int columns = 2;
        int rows = Mathf.CeilToInt(spawnCount / columns);

        for (int i = 0; i < spawnCount; i++)
        {
            //생성 마릿수 초과하면 리턴
            if (isUnCreatableMonster()) { return; }
            if (!isOneType) { randomMonsterIdx = Random.Range(0, randomIdx); }

            int row = i / columns;
            int col = i % columns;

            float posX = startPos.x + (col - 0.5f) * spacing;
            float posY = startPos.z + (row - (rows - 1) / 2.0f) * spacing;

            Vector3 pos = new Vector3(posX, 0, posY);

            Vector3 direction = (target.transform.position - pos).normalized;
            Quaternion lookAt = Quaternion.LookRotation(direction);
            CreateMonsterObj(pos, lookAt, randomMonsterIdx);
        }
    }

    bool isUnCreatableMonster()
    {
        //현재 마릿수가 최대 마릿수를 넘거나 총 생성된 마릿수가 한 웨이브당 생성되는 몬스터 마릿수를 초과하면 생성 불가
        return currentCount >= maxSpawnCount || totalCount >= flagCount[level, flagIdx];
    }

    public void RemoveMonster(GameObject obj)
    {
        allMonsterList.Remove(obj);
        killCount++;
        currentCount--;
        //if (DebugManager.Instance != null)
        //{
        //    DebugManager.Instance.SetInfoText("Cur: " + currentCount + "\nkill : " + killCount);
        //}
    }

    //빅웨이브 알림 코루틴
    IEnumerator BigWave()
    {
        //Debug.Log(flagIdx + 1 + "차 빅웨이브타임!");
        isOn = false;
        yield return new WaitForSeconds(3);
        isOn = true;
        //Debug.Log("빅웨이브!");
        isBigWave = true;
        flagIdx += 1;

        if (level == 2 && flagIdx == 3)
        {
            //보스생성
            //Debug.Log("보스 생성!");
            photonView.RPC("WarningAlert", RpcTarget.All, 3f, "보스 출현!");
            pattern = SpawnPattern.Boss;
            SpawnMonster(1);
        }
        else
        {
            photonView.RPC("WarningAlert", RpcTarget.All, 3f, flagIdx + "차 빅웨이브 타임!");
            pattern = SpawnPattern.Cross;
            isOneType = false;
            SpawnMonster(bigWaveCount[level]);
        }
    }

    [PunRPC]
    public void WarningAlert(float duration, string message)
    {
        IngameUIManager.Inst.WarningBoss(duration, message);  
    }

    public void CreateTestMonster(int idx)
    {
        GameObject obj = PhotonNetwork.Instantiate(monsterPath + testList[idx].name, GetRandomPos(), Quaternion.identity);
        allMonsterList.Add(obj);
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = new Color(0, 0, 1, 0.3f);

    //    for (float r = 0; r <= spawnRadius; r += 0.1f)
    //    {
    //        Gizmos.DrawWireSphere(transform.position, r);
    //    }
    //}

    public void SetTarget(GameObject obj)
    {
        target = obj;
    }

    public GameObject GetTarget()
    {
        return target;
    }

    public List<GameObject> GetMonsterList()
    {
        return allMonsterList;
    }

    public void SetLevelBroadcast(int _level)
    {
        photonView.RPC("SetLevel", RpcTarget.All, _level);
    }

    [PunRPC]
    public void SetLevel(int _level)
    {
        level = _level - 1;
    }
}
