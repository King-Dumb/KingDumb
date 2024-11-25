using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 타워의 전반적인 관리를 담당하는 역할
public class TowerManager : MonoBehaviourPun
{
    public static TowerManager Inst;

    private GameObject selectedTowerGround;
    private int selectedTowerGroundIndex;
    private bool[] _isTowerBuiltList = new bool[100];

    private string towerJson = "tower_info";
    private List<TowerJsonInfos> _towerJsonInfos = new List<TowerJsonInfos>();
    private Dictionary<int, Tower> _towerDictionary = new Dictionary<int, Tower>();


    // private int maxTowerLevel = 3;
    // private int[] maxTowerLevelList = new int[4]; // 구매한 타워의 최대 레벨을 저장

    void Awake()
    {
        //싱글톤 선언
        if (Inst == null)
        {
            Inst = this;
            TowerList towerList = GetTowerDTO(towerJson);
            _towerJsonInfos = towerList.towers;
            ParsingTowerJson(towerList);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void BuildTowerRequestToMaster(int selectedTowerType, int towerLevel,
        Vector3 position, Quaternion newTowerRotation, int requireGold, int selectedTowerGroundIndex)
    {
        photonView.RPC("BuildTower", RpcTarget.MasterClient, selectedTowerType, towerLevel,
            position, newTowerRotation, requireGold, selectedTowerGroundIndex);
    }

    [PunRPC]
    public void BuildTower(int selectedTowerType, int towerLevel,
        Vector3 position, Quaternion newTowerRotation, int requireGold, int selectedTowerGroundIndex)
    {
        //Debug.Log($"BuildTower called with: Type={selectedTowerType}, Level={towerLevel}, Position={position}");
        //Debug.Log($"Tower{selectedTowerType}{towerLevel}");
        if (PhotonNetwork.IsMasterClient)
        {
            //Debug.Log("마스터가 생성");
            GameObject builtTower = PhotonNetwork.Instantiate($"Tower{selectedTowerType}{towerLevel}", position, newTowerRotation);

            // TOWER:


            Tower tower = builtTower.GetComponent<Tower>();

            // 딕셔너리에 저장
            int viewID = builtTower.GetComponent<PhotonView>().ViewID;
            //_towerPhotonViewIds[selectedTowerGroundIndex] = viewID;

            CharacterManager.Inst.RegisterPlayerToMaster(viewID);


            // 모든 클라이언트에 PhotonView ID 브로드캐스트
            photonView.RPC("SyncTowerData", RpcTarget.All, viewID, selectedTowerGroundIndex, selectedTowerType, towerLevel);

            // 금액 처리
            tower.BuyTowerBroadcast(selectedTowerGroundIndex, requireGold, selectedTowerType, towerLevel);
        }
    }

    [PunRPC]
    public void SyncTowerData(int photonViewID, int groundIndex, int towerType, int towerLevel)
    {
        // PhotonView ID로 타워 찾기
        PhotonView towerPhotonView = PhotonView.Find(photonViewID);
        if (towerPhotonView == null)
        {
            //Debug.LogError($"Failed to find tower with PhotonView ID: {photonViewID}");
            return;
        }

        Tower tower = towerPhotonView.GetComponent<Tower>();
        tower.towerLevel = towerLevel;
        tower.towerType = towerType;
        // 클라이언트의 타워 매핑
        _towerDictionary[groundIndex] = tower;
        //Debug.Log($"Tower synced: GroundIndex={groundIndex}, TowerType={towerType}, Level={towerLevel}");
    }

    private TowerList GetTowerDTO(string jsonName)
    {
        if (SystemManager.Instance == null)
            return null;

        return SystemManager.Instance.LoadJson<TowerList>(jsonName);
    }

    private void ParsingTowerJson(TowerList towerList)
    {
        List<TowerJsonInfos> towerJsonInfos = towerList.towers;
        // 0: 전사, 1: 궁수, 2: 마법사, 3: 힐러
        // if (towerList != null)
        // {
        //     foreach (TowerJsonInfos tower in towerList.towers)
        //     {
        //         ////Debug.Log($"Tower Type: {tower.towerType} ({(int)tower.towerType})");
        //         foreach (TowerData level in tower.towerData)
        //         {
        //             ////Debug.Log($"Level {level.level}: {level.towerName} - {level.towerInfo}, Gold: {level.requiredGold}");
        //         }
        //     }
        // }
        // else
        // {
        //     //Debug.LogError("Failed to load tower data.");
        // }
    }

    public TowerData GetTowerDataByIndex(int towerType, int towerLevel)
    {
        return _towerJsonInfos[towerType].towerData[towerLevel];
    }

    public Tower GetTowerByIndex(int index)
    {
        return _towerDictionary[index];
    }

    public int GetTowerTypeByIndex(int index)
    {
        return _towerDictionary[index].towerType;
    }

    public int GetTowerLevelByIndex(int index)
    {
        return _towerDictionary[index].towerLevel;
    }

    public bool GetIsTowerBuilt(int index)
    {
        return _isTowerBuiltList[index];
    }

    public void SetIsTowerBuilt(int index, bool isBuilt)
    {
        _isTowerBuiltList[index] = isBuilt;
    }

    public int GetRequireGold(int towerType, int towerLevel)
    {
        return _towerJsonInfos[towerType].towerData[towerLevel].requiredGold;
    }

    // public int GetMaxTowerLevel(int index)
    // {
    //     return maxTowerLevelList[index];
    // }

    // public void SetMaxTowerLevel(int index, int level)
    // {
    //     if (level > maxTowerLevel)
    //     {
    //         return;
    //     }

    //     maxTowerLevelList[index] = level;

    //     //Debug.Log($"최대 건설 가능 타워 레벨이 재조정됩니다. 타워 타입 : {index}, 타워 레벨 : {level}");
    // }


    public enum TowerType
    {
        Warrior = 1,
        Archer = 2,
        Mage = 3,
        Priest = 4,
    }
}
