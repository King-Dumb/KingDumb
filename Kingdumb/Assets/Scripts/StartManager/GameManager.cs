using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//게임의 절대적인 관리를 맡는 매니저
public class GameManager : SingleTon<GameManager>
{    
    //플레이어
    private enum EPLAYERCLASS
    {
        Warrior,
        Archer,
        Mage,
        Priest
    }

    public GameObject localPlayer;
    public string playerClassName;
    public int playerClassCode;

    public bool[] savedSkillNode;
    public int savedPlayerSkillPoint;

    public int playerCnt = 0;

    private HashSet<int> loadedPlayerList = new HashSet<int>();

    // 골드
    public int initialGold = 1000;
    private int _totalPlayerGold = 1000;
    public int totalPlayerGold
    {
        get { return _totalPlayerGold; }
        set
        {
            _totalPlayerGold = value;
        }
    }

    // 경험치
    public int initialExp = 0;
    private int _totalPlayerExp = 0;
    public int totalPlayerExp
    {
        get { return _totalPlayerExp; }
        set
        {
            _totalPlayerExp = value;
        }
    }

    // 레벨
    public int initialLevel = 1;
    private int _curLevel = 1;
    public int curLevel
    {
        get { return _curLevel; }
        set
        {
            _curLevel = value;
        }
    }

    // 타이머
    public float totalIncreaseMin = 0f;
    public float totalIncreaseSec = 0f;

    public void DestroyPhotonObj(GameObject obj)
    {
        StopAndRemoveCoroutine(obj);
        PhotonNetwork.Destroy(obj);
    }
    public void DestroyPhotonObj(GameObject obj, float delay)
    {        
        StopAndRemoveCoroutine(obj); // 기존 코루틴이 있다면 중지한다.
        Coroutine cor = StartCoroutine(SetTimePhoton(obj, delay));
        IngameManager.Inst.coroutineDict[obj] = cor;
    }

    public GameObject Instantiate(string objName)
    {
        return IngameManager.Inst.objPool.CreateObj(objName);
    }

    public GameObject Instantiate(string objName, Vector3 pos, Quaternion rot)
    {
        return IngameManager.Inst.objPool.CreateObj(objName, pos, rot);
    }

    public void Destroy(GameObject obj)
    {
        StopAndRemoveCoroutine(obj);
        IngameManager.Inst.objPool.DestroyObj(obj.name, obj);
    }
    public void Destroy(GameObject obj, float delay)
    {
        StopAndRemoveCoroutine(obj);

        Coroutine cor = StartCoroutine(SetDestroyTime(obj, delay));
        IngameManager.Inst.coroutineDict[obj] = cor;
    }

    private IEnumerator SetTimePhoton(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        StopAndRemoveCoroutine(obj);
        PhotonNetwork.Destroy(obj);
    }

    private IEnumerator SetDestroyTime(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);        
        
        if(obj != null)
        {
            StopAndRemoveCoroutine(obj);
            IngameManager.Inst.objPool.DestroyObj(obj.name, obj);
        }        
    }

    private void StopAndRemoveCoroutine(GameObject obj)
    {
        if (IngameManager.Inst.coroutineDict.TryGetValue(obj, out Coroutine coroutine))
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        IngameManager.Inst.coroutineDict.Remove(obj);
    }

    //플레이어 관련
    public string GetPlayerClassStr(int classCode)
    {
        EPLAYERCLASS playerClass = (EPLAYERCLASS)classCode;
        string className = null;
        switch (playerClass)
        {
            case EPLAYERCLASS.Warrior:
                className = GameConfig.WarriorClass;
                break;
            case EPLAYERCLASS.Archer:
                className = GameConfig.ArcherClass;
                break;
            case EPLAYERCLASS.Mage:
                className = GameConfig.MageClass;
                break;
            case EPLAYERCLASS.Priest:
                className = GameConfig.PriestClass;
                break;
        }
        return className;
    }

    public void SetPlayerInfo(int classCode)
    {
        playerClassCode = classCode;
        playerClassName = GetPlayerClassStr(classCode);
    }
    public void RemovePlayer()
    {
        DestroyPhotonObj(localPlayer);
        localPlayer = null;
    }

    public void ClearObjPool()
    {
        if(IngameManager.Inst != null)
            IngameManager.Inst.objPool.ClearObjPool();
    }

    public void SaveTotalGoldExp(int gold, int exp, int level)
    {
        totalPlayerGold = gold;
        totalPlayerExp = exp;
        curLevel = level;
    }

    public void SaveTotalTime(float stageIncreaseMin, float stageIncreaseSec)
    {
        totalIncreaseMin = stageIncreaseMin;
        totalIncreaseSec = stageIncreaseSec;
    }


    //만들었지만 결국 안써도될듯
    public void CheckPlayerLoaded(int actorNumber)
    {

        //로드된 플레이어 등록
        if (!loadedPlayerList.Contains(actorNumber)) 
        {
            playerCnt++;
            loadedPlayerList.Add(actorNumber);
        }

        if(loadedPlayerList.Count == playerCnt)
        {
            //Debug.Log("★ 모든 플레이어 로딩 완료!");
        }
    }

    public void ClearPlayerInfo()
    {
        if (PlayerStatisticsManager.Instance != null)
        {
            UnityEngine.Object.Destroy(PlayerStatisticsManager.Instance.gameObject);
        }

        if(PhotonManager.Inst != null)
        {
            PhotonManager.Inst.ResetPlayerProperty();
        }
        
        if(localPlayer != null)
        {
            DestroyPhotonObj(localPlayer);
        }        
        playerClassName = null;
        playerClassCode = 0;
        savedSkillNode = null;
        savedPlayerSkillPoint = 0;
        initialGold = 1000;
        _totalPlayerGold = 1000;
        totalPlayerGold = 1000;
        initialExp = 0;
        _totalPlayerExp = 0;
        totalPlayerExp = 0;
        initialLevel = 1;
        _curLevel = 1;
        curLevel = 1;
        totalIncreaseMin = 0f;
        totalIncreaseSec = 0f;        
    }

}
