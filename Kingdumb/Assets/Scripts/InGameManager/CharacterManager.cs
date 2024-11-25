using Cinemachine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using TMPro;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using Unity.VisualScripting;

//캐릭터 간 상호작용을 중계할 매니저, 마스터 클라이언트가 담당
public class CharacterManager : MonoBehaviourPun
{
    public static CharacterManager Inst;

    private GameManager GM;

    //##플레이어 관련
    //플레이어 는 GameManager.Instance.localPlayer에 있음
    //이 스크립트는 DontDestroyOnload가 아니라서 플레이어 정보가 소멸되기 때문
    public GameObject cameraPrefab;
    public List<GameObject> playerPrefabList;

    private GameObject playerCamera;

    //원점으로부터 십자 모양으로 스폰되는데 그 오프셋
    private List<Vector3> playerSpawnPointList;
    public float playerSpawnOffset = 5f;

    //넥서스 관련
    public GameObject nexusPrefab;
    private GameObject nexus;

    // 마스터 클라이언트의 플레이어 딕셔너리
    private Dictionary<int, GameObject> playerDictionary = new Dictionary<int, GameObject>();

    void Awake()
    {
        ////Debug.Log("ChacterManager Awake");
        //싱글톤 선언
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(gameObject);
        }

        GM = GameManager.Instance;

        playerSpawnPointList = new List<Vector3>();
        playerSpawnPointList.Add(new Vector3(0, 0, playerSpawnOffset));
        playerSpawnPointList.Add(new Vector3(0, 0, playerSpawnOffset * -1f));
        playerSpawnPointList.Add(new Vector3(playerSpawnOffset, 0, 0));
        playerSpawnPointList.Add(new Vector3(playerSpawnOffset * -1f, 0, 0));
    }

    // Start is called before the first frame update
    //void Start()
    //{
    ////Debug.Log("ChacterManager Start");
    //}

    // Update is called once per frame
    //void Update()
    //{
    //#####직업 변경 테스트용
    //1. 전사 2, 궁수, 3, 마법사, 4. 메이지        
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        SetPlayer(0, GM.localPlayer.transform.position, Quaternion.identity);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        SetPlayer(1, GM.localPlayer.transform.position, Quaternion.identity);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.Alpha3))
    //    {
    //        SetPlayer(2, GM.localPlayer.transform.position, Quaternion.identity);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.Alpha4))
    //    {
    //        SetPlayer(3, GM.localPlayer.transform.position, Quaternion.identity);
    //    }
    //}
    //}

    //플레이어 생성 및 컨트롤러 세팅
    public void SetPlayer(int classCode, Vector3 spawnPos, Quaternion rot)
    {
        GameObject prevPlayer = GM.localPlayer;
        GameObject player = null;
        if (PhotonNetwork.IsConnected)
        {
            //플레이어 세팅
            player = PhotonNetwork.Instantiate(playerPrefabList[classCode].name, spawnPos, rot);
            //DontDestroyOnLoad(player);
            ////Debug.Log("Chracter Manager를 통해 " + SceneManager.GetActiveScene().name + "에 플레이어 생성");

            GameManager.Instance.SetPlayerInfo(classCode);
            SetCamera(player);
            SetController(player);

            GM.localPlayer = player;

            if (prevPlayer != null)
            {
                GameManager.Instance.DestroyPhotonObj(prevPlayer);
                Destroy(prevPlayer);
            }

            // 닉네임 세팅
            TextMeshProUGUI nickname = player.GetComponentInChildren<TextMeshProUGUI>();
            nickname.text = PhotonNetwork.LocalPlayer.NickName;
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv.IsMine)
            {
                nickname.gameObject.SetActive(false);
                photonView.RPC("RegisterPlayerToMaster", RpcTarget.MasterClient, pv.ViewID);
            }
        }
    }

    public void SetCamera(GameObject obj)
    {
        //카메라 세팅
        // if(playerCamera == null)
        // {
        //     playerCamera = Instantiate(cameraPrefab);
        // }

        // playerCamera.GetComponent<CinemachineVirtualCamera>().Follow = obj.transform;

        if (CameraControl.Inst != null)
        {
            // // 카메라 회전 막기
            // CameraControl.Inst.LockCamera();

            CameraControl.Inst.virtualCamera.Follow = obj.transform;
        }
    }

    //플레이어 직업에 따라 컨트롤러를 세팅한다.
    private void SetController(GameObject obj)
    {
        if (obj == null)
            return;

        IPlayerClass pClass = null;

        pClass = obj.GetComponentInChildren<IPlayerClass>();
        //Debug.Log($"클래스 {pClass} 로 설정");
        obj.GetComponentInChildren<PlayerController>().SetPlayerClass(pClass);
    }

    //캐릭터 데미지 처리 (몬스터, 캐릭터, 넥서스 등)
    //src : 공격대상,
    //dest : 피격 대상
    // public void OnDamage(GameObject src, GameObject dest)
    // {

    // }

    // public void CalcDamage(float damage)
    // {

    // }


    // public void ShareEXP(int exp)
    // {

    // }

    // public void SetEXP(int exp)
    // {

    // }

    // //골드
    // public void ShareGold(int gold)
    // {

    // }

    // public void UseGold(int gold)
    // {

    // }

    // //궁 게이지 
    // public void SharePoint(int point)
    // {

    // }

    // public void LevelUpPlayer()
    // {
    //     //스킬 포인트 배분
    // }

    // public void ReviveCharacter()
    // {

    // }

    // public void KillCharacter()
    // {

    // }

    private bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public List<Vector3> GetPlayerSpawnList()
    {
        return playerSpawnPointList;
    }

    public GameObject CreateNexus()
    {
        if (nexus != null)
            return null;

        if (PhotonNetwork.IsConnected)
        {
            nexus = PhotonNetwork.Instantiate(nexusPrefab.name, Vector3.zero, Quaternion.identity);
            PhotonView pv = nexus.GetComponent<PhotonView>();
            if (pv.IsMine)
            {
                RegisterPlayerToMaster(pv.ViewID);
            }
            Nexus nexusScript = nexus.GetComponent<Nexus>();
            nexusScript.OnDeath += IngameManager.Inst.GameOver;
        }
        else
        {
            //Debug.Log("넥서스 로컬 객체로 생성");
            nexus = Instantiate(nexusPrefab);
        }

        if (nexus != null)
        {
            MonsterGenerator.Inst.SetTarget(nexus);
        }

        return nexus;
    }

    public void InitPlayerAll()
    {
        photonView.RPC("InitPlayerBroadcast", RpcTarget.All);

    }

    [PunRPC]
    public void InitPlayerBroadcast()
    {
        //IngameManager.Inst.InitPlayer();
        InitPlayer();
        IngameUIManager.Inst.InitIngameUI(); // UI 설정
    }

    public void InitPlayer()
    {
        int idx = 0;
        if (PhotonNetwork.IsConnected)
        {
            //미갱신 시 -1 반환
            idx = PhotonNetwork.LocalPlayer.ActorNumber;
            if (idx == -1)
                idx = 0;
        }

        //Debug.Log("스폰 리스트 사이즈"+spawnList.Count+" , 인덱스 "+idx);
        SetPlayer(GameManager.Instance.playerClassCode, playerSpawnPointList[(idx - 1) % 4], Quaternion.identity);
    }



    public GameObject GetNexus()
    {
        return nexus;
    }

    public float GetNexusHp()
    {
        if (nexus != null && nexus.TryGetComponent<Nexus>(out Nexus nexusScript))
        {
            Debug.Log("받으려는 현재 넥서스의 체력은: " + nexusScript._nexusHealth);

            return (nexusScript._nexusHealth > 0f ? nexusScript._nexusHealth / nexusScript.nexusMaxHealth : 0f) * 100;
        }
        //Debug.Log("넥서스가 없음");
        return 0f;
    }

    // 플레이어 리스트(딕셔너리)를 가지기 위한 메서드----------------------------------------------------------------------------------------
    [PunRPC]
    public void RegisterPlayerToMaster(int viewID)
    {
        // Photon View로 오브젝트 찾기
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
        {
            GameObject playerObject = targetView.gameObject;

            // 딕셔너리에 추가
            if (!playerDictionary.ContainsKey(viewID))
            {
                playerDictionary.Add(viewID, playerObject);
                //Debug.Log($"Player added to dictionary: ViewID={viewID}, Object={playerObject.name}");
            }
            // else
            // {
            //     //Debug.LogWarning($"ViewID {viewID} is already registered.");
            // }
        }
        // else
        // {
        //     //Debug.LogError($"Failed to find PhotonView with ViewID {viewID}");
        // }
    }

    public Dictionary<int, GameObject> GetPlayerDictionary()
    {
        return playerDictionary;
    }
}
