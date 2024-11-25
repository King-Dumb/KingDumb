using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiTestManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1"; // 게임 버전
    public GameObject playerPrefab;
    public GameObject monsterGeneratorPrefab;
    public GameObject nexusPrefab;
    // public CinemachineVirtualCamera playerCameraPrefab;
    //public GameObject PortalPrefab;

    private GameObject player;
    private new CinemachineVirtualCamera camera;
    //public Text connectionInfoText; // 네트워크 정보를 표시할 텍스트
    //public Button joinButton; // 룸 접속 버튼

    // 게임 실행과 동시에 마스터 서버 접속 시도
    private void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        //joinButton.interactable = false;
        //connectionInfoText.text = "마스터 서버에 접속 중...";
        Debug.Log("마스터 서버 접속 시도");
    }

    // 마스터 서버 접속 성공시 자동 실행
    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버 접속 성공");
        //joinButton.interactable = true;
        //connectionInfoText.text = "온라인 : 마스터 서버와 연결됨";
        Connect();
    }

    // 마스터 서버 접속 실패시 자동 실행
    public override void OnDisconnected(DisconnectCause cause)
    {
        //joinButton.interactable = false;
        //connectionInfoText.text = "오프라인 : 마스터 서버와 연결되지 않음\n 접속 재시도 중...";

        // 마스터 서버로의 재접속 시도
        PhotonNetwork.ConnectUsingSettings();
    }

    // 룸 접속 시도
    public void Connect()
    {
        Debug.Log("룸 접속 시도");
        //joinButton.interactable = false;

        if (PhotonNetwork.IsConnected)
        {
            // 룸 접속 실행
            //connectionInfoText.text = "룸에 접속...";
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
            //PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            //connectionInfoText.text = "오프라인 : 마스터 서버와 연결되지 않음\n접속 재시도 중...";
            PhotonNetwork.ConnectUsingSettings();
        }

    }

    // (빈 방이 없어)랜덤 룸 참가에 실패한 경우 자동 실행
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //connectionInfoText.text = "빈 방이 없음, 새로운 방 생성...";
        Debug.Log("새로운 방 생성 시도");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    // 룸에 참가 완료된 경우 자동 실행
    public override void OnJoinedRoom()
    {
        Debug.Log("룸 접속 성공");
        //connectionInfoText.text = "방 참가 성공";
        //PhotonNetwork.LoadLevel("Main");
        makeCube();
    }

    public void makeCube()
    {
        Vector3 randomSpawnPos = Random.insideUnitSphere * 5f;
        // 위치 y값은 0으로 변경
        randomSpawnPos.y = 0f;
        Debug.Log(playerPrefab);
        // 네트워크 상의 모든 클라이언트들에서 생성 실행
        // 단, 해당 게임 오브젝트의 주도권은, 생성 메서드를 직접 실행한 클라이언트에게 있음

        Debug.Log("플레이어 프리팹의 이름: " + playerPrefab.name);
        player = PhotonNetwork.Instantiate(playerPrefab.name, randomSpawnPos, Quaternion.identity);
        Debug.Log("플레이어 생성: " + player);
        // camera = FindObjectOfType<CinemachineVirtualCamera>();
        // if (camera == null)
        // {
        //     camera = Instantiate(playerCameraPrefab);
        // }
        camera = CameraControl.Inst.virtualCamera;
        camera.Follow = player.transform;

        SetPlayerClass(GameConfig.WarriorClass);

        if (PhotonNetwork.IsMasterClient)
        {
            // 테스트용 몬스터 생성기 활성화
            GameObject monsterGenerator = PhotonNetwork.Instantiate(monsterGeneratorPrefab.name, Vector3.zero, Quaternion.identity);
            monsterGenerator.SetActive(true);

            GameObject nexus = PhotonNetwork.Instantiate(nexusPrefab.name, randomSpawnPos, Quaternion.identity);
            monsterGenerator.GetComponent<MonsterGeneratorForTest>().target = nexus;
        }
    }

    // 플레이어 직업을 설정하는 메서드 예시

    public void SetPlayerClass(string className)
    {
        IPlayerClass pClass = null;

        pClass = player.GetComponentInChildren<IPlayerClass>();
        Debug.Log($"플레이어 클래스 {pClass} 로 설정");
        player.GetComponentInChildren<PlayerController>().SetPlayerClass(pClass);
    }

    //public void CreatePortal(Vector3 position, Quaternion rotation)
    //{
    //    Debug.Log(position + " " + rotation);
        
    //    photonView.RPC("CreatePortalBroadcast", RpcTarget.All, position, rotation);
    //}

    //[PunRPC]
    //public void CreatePortalBroadcast(Vector3 position, Quaternion rotation)
    //{
    //    // 포탈 생성 로직
    //    Debug.Log("Portal created at " + position);
    //    //Instantiate(PortalPrefab, position, rotation);  
    //}
}
