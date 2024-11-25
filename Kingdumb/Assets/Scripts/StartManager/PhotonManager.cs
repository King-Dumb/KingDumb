using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    //싱글톤
    public static PhotonManager Inst { get; private set; }

    // 룸을 저장하기 위한 딕셔너리 자료형
    private Dictionary<string, GameObject> roomList = new Dictionary<string, GameObject>();

    public GameObject roomPrefab; // 룸을 표시할 프리팹
    public Transform scrollContent; // 룸의 부모 객체

    public static void SetNickname(string nickname) => PhotonNetwork.NickName = nickname;

    private void Awake()
    {
        //싱글톤 선언
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 서버에 연결
    public static void Connect()
    {
        // 서버 연결 전 게임 설정
        PhotonNetwork.GameVersion = GameConfig.GameVersion;
        // PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "kr";

        PhotonNetwork.ConnectUsingSettings();
    }

    // 마스터 서버 접속 성공 시 시작 버튼 활성화
    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터서버로그인");
        if (TitleManager.Inst != null) // 타이틀이라면
        {
            TitleManager.Inst.SetTitleInfo();
        }
        else
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("네트워크 연결 끊김");
        PhotonNetwork.LoadLevel("Title");
    }

    public override void OnJoinedLobby()
    {
        //Debug.Log("플레이어 정보 초기화ㅁㅁ");

        if (GameManager.Instance != null)
            GameManager.Instance.ClearPlayerInfo();

        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.Equals("Lobby"))
        {
            PhotonNetwork.LoadLevel("Lobby");
        }
    }

    public override void OnLeftLobby()
    {
        // 로비를 떠남
        PhotonNetwork.LoadLevel("Title");
    }

    //플레이어의 고유정보를 서버에 등록하는 함수
    //value가 T타입이라 디폴트 값 지정 순서가 안맞아서 Key, Value 순이 아닌 Value, Key 순이다.
    public static void SetPlayerCustomProperty<T>(T value, string name = null, Player player = null)
    {

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            //Debug.Log("PhotonNetwork에 연결되지 않아 return합니다.");
            return;
        }

        Player p = player;
        if (p == null)
        {
            p = PhotonNetwork.LocalPlayer;
        }

        ExitGames.Client.Photon.Hashtable prop = new Hashtable
        {
            {name, value }
        };
        p.SetCustomProperties(prop);
    }

    // 플레이어 정보를 
    public static T GetPlayerCustomProperty<T>(string key, Player player = null, T defaultValue = default)
    {
        // player가 null이면 로컬 플레이어 사용
        player ??= PhotonNetwork.LocalPlayer;

        if (player.CustomProperties.TryGetValue(key, out object value))
        {
            // 타입 검사를 수행하여 안전하게 반환
            if (value is T typedValue)
            {
                return typedValue;
            }
            // else
            // {
            //     Debug.LogWarning($"CustomProperty '{key}' is not of type {typeof(T)}.");
            // }
        }
        // else
        // {
        //     Debug.LogWarning($"CustomProperty '{key}' not found for player {player.NickName}.");
        // }

        // 기본값 반환
        return defaultValue;
    }

    //플레이어 프로퍼티 초기화
    public void ResetPlayerProperty()
    {
        Player player = PhotonNetwork.LocalPlayer;

        ExitGames.Client.Photon.Hashtable playerProp = player.CustomProperties;

        List<string> removeKeyList = new List<string>();
        foreach (string key in playerProp.Keys)
        {
            removeKeyList.Add(key);
        }
        string[] removeKeyArr = removeKeyList.ToArray();

        PhotonNetwork.RemovePlayerCustomProperties(removeKeyArr);
    }

    // 게임 종료
    public void QuitGame()
    {
        // 빌드된 게임에서 종료
        Application.Quit();

        // 유니티 에디터에서 종료
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // 게임 나가기
    public void ExitGame()
    {
        // //if (PhotonNetwork.InRoom) // 현재 방에 있다면
        // {
        //     Debug.Log("결과화면에서  나가기");
        //     Time.timeScale = 1f;
        //     PhotonNetwork.LoadLevel("Lobby");
        //     PhotonNetwork.LeaveRoom();
        //     PhotonNetwork.Disconnect();
        // }
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(); // 방을 나가면 자동으로 로비 접속
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("방 나감");
        //룸 매니저 SetReadyStatus(false);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearPlayerInfo();
        }

        // PhotonNetwork.JoinLobby();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Debug.Log("ㅇㄹㅇㄹㅇㄹ플레이어 나감");
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "InGame" || sceneName == "InGame_2S" || sceneName == "InGame_3S")
        {
            GameManager.Instance.playerCnt = PhotonNetwork.CurrentRoom.Players.Count;
        }
    }

    // 씬 로드 동기화
    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceived;
    }

    public static void SendEvent(byte eventCode)
    {
        // 모든 클라이언트에게 게임 시작 이벤트 전송
        PhotonNetwork.RaiseEvent(eventCode, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
    }

    private void OnEventReceived(EventData photonEvent)
    {
        //Debug.Log("OnEventReceived 실행으로 인한 씬 변경");
        // 게임 시작 이벤트 수신 시 씬 이동
        switch (photonEvent.Code)
        {
            case 1:
                LoadInGameScene(1);
                GameManager.Instance.playerCnt = PhotonNetwork.CurrentRoom.PlayerCount;
                break;
            case 2:
                LoadInGameScene(2);
                GameManager.Instance.playerCnt = PhotonNetwork.CurrentRoom.PlayerCount;
                break;
            case 3:
                LoadInGameScene(3);
                GameManager.Instance.playerCnt = PhotonNetwork.CurrentRoom.PlayerCount;
                break;
            case 4:
                LoadInGameScene(4);
                GameManager.Instance.playerCnt = 0;
                break;
        }
    }

    private void LoadInGameScene(int sceneNum)
    {
        switch (sceneNum)
        {
            case 1:
                // InGame 씬 로드
                PhotonNetwork.LoadLevel("InGame");
                break;
            case 2:
                PhotonNetwork.LoadLevel("InGame_2S");
                break;
            case 3:
                PhotonNetwork.LoadLevel("InGame_3S");
                break;
            case 4:
                PhotonNetwork.LoadLevel("Lobby");
                break;
        }
    }
}
