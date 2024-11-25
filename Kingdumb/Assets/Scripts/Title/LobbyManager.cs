using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using ExitGames.Client.Photon;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Lobby")]
    public Transform scrollContent;
    public GameObject roomPrefab;
    public Button createButton;
    public Button findButton;
    public Button quickEnterButton;
    public TextMeshProUGUI playerCountInLobbyText;
    public Button gameQuitBtn;


    // 룸 목록에 대한 데이터를 저장하기 위한 딕셔너리 자료형
    private List<RoomInfo> rooms = new List<RoomInfo>();

    public CreateRoomPopup createRoomPopup;
    public FindRoomPopup findRoomPopup;

    public Notification notification;

    //public DebugManager debugManager;

    private void Awake()
    {
        createButton.onClick.AddListener(() => OnPopup("CREATE"));
        findButton.onClick.AddListener(() => OnPopup("FIND"));
        quickEnterButton.onClick.AddListener(() => OnPopup("QUICK ENTER"));
        gameQuitBtn.onClick.AddListener(OnClickQuitBtn);
    }

    public void OnClickQuitBtn()
    {
        notification.OnPopup();
        notification.SetButton(2);
        notification.SetContent("게임을 종료하시겠습니까?");
        notification.SetOkType("Quit");
    }

    private void Update()
    {
        HandleCursorUnlock();
        UpdatePlayerCount();
    }

    // private void HandleLobbyState()
    // {
    //     // 현재 클라이언트 상태 확인
    //     var clientState = PhotonNetwork.NetworkClientState;

    //     if (clientState == Photon.Realtime.ClientState.ConnectedToMasterServer)
    //     {
    //         // Debug.Log("서버에 연결되었으나 로비에 접속하지 않았습니다. 로비에 접속합니다.");
    //         PhotonNetwork.JoinLobby(); // 로비에 접속
    //     }
    //     else if (clientState == Photon.Realtime.ClientState.JoiningLobby)
    //     {
    //         // Debug.Log("로비에 접속 중입니다...");
    //     }
    //     else if (clientState == Photon.Realtime.ClientState.JoinedLobby)
    //     {
    //         // Debug.Log("로비에 성공적으로 접속했습니다.");
    //         HandleCursorUnlock();
    //         UpdatePlayerCount();
    //     }
    //     else
    //     {
    //         // Debug.Log($"현재 상태: {clientState}");
    //     }
    // }

    private void HandleCursorUnlock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void UpdatePlayerCount()
    {
        playerCountInLobbyText.text = $"로비 인원 : {PhotonNetwork.CountOfPlayersOnMaster}";
    }

    // 로비 접속 시 생성된 방 목록 업데이트
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InLobby)
        {
            if (roomList == null || roomList.Count == 0)
            {
                return;
            }

            // 변경된 방 정보 저장
            foreach (RoomInfo roomInfo in roomList)
            {
                // 방이 제거된 경우
                if (roomInfo.RemovedFromList)
                {
                    // 목록에 있다면
                    if (rooms.Contains(roomInfo))
                    {
                        rooms.Remove(roomInfo); // 목록에서 삭제
                    }
                }
                else // 방이 추가되거나 변경된 경우
                {
                    // 목록에 없다면
                    if (!rooms.Contains(roomInfo))
                    {
                        rooms.Add(roomInfo); // 목록에 추가
                    }
                    else
                    {
                        // 목록 갱신
                        rooms.Remove(roomInfo);
                        rooms.Add(roomInfo);
                    }
                }
            }

            // 기존 방 목록 UI 초기화
            foreach (Transform child in scrollContent)
            {
                Destroy(child.gameObject);
            }

            int roomNum = 0;

            // 방 목록 UI 표시 -> 추후 문제 확인 (리스트 리셋 안되는 문제)
            foreach (RoomInfo room in rooms)
            {
                string[] roomName = GameConfig.ParseString(room.Name);

                if (room.IsVisible && roomName != null) // 방이 자동 생성되는 오류 방지
                {
                    // 방 목록 항목 생성
                    GameObject roomEntry = Instantiate(roomPrefab, scrollContent);
                    roomEntry.transform.Find("Contents/RoomNumber").GetComponent<TextMeshProUGUI>().text = "" + ++roomNum;
                    roomEntry.transform.Find("Contents/RoomName").GetComponent<TextMeshProUGUI>().text = roomName[0];
                    if (room.CustomProperties.TryGetValue("isPrivate", out object isPrivate))
                    {
                        roomEntry.transform.Find("Contents/PrivateContainer").GetChild(0).gameObject.SetActive((bool)isPrivate);
                    }
                    roomEntry.transform.Find("Contents/PlayerCount").GetComponent<TextMeshProUGUI>().text = room.PlayerCount + "/" + room.MaxPlayers;

                    // 방에 참가하는 버튼에 이벤트 등록
                    Button joinRoomButton = roomEntry.GetComponent<Button>();
                    joinRoomButton.onClick.AddListener(() => JoinRoom(room));
                }
            }
        }
    }

    // 특정 방에 참가하는 메서드
    private void JoinRoom(RoomInfo roomInfo)
    {
        GlobalSoundManager.Instance.PlaySubmitSound();
        if (PhotonNetwork.IsConnected)
        {
            if (roomInfo.CustomProperties.TryGetValue("isPrivate", out object isPrivateProperty))
            {
                // 비공개 방 여부 체크 후 참가
                if ((bool)isPrivateProperty)
                {
                    // 코드 입력 창 띄우기
                    findRoomPopup.OnPopup();
                }
                else
                {
                    PhotonNetwork.JoinRoom(roomInfo.Name);
                }
            }
        }
        else
        {
            // 서버 재접속 시도
            PhotonNetwork.Disconnect();
        }
    }

    // 방 참가 성공 시 Room Scene 로드
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Room");
    }

    // 방 참가 실패 시 호출
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        // 방이 닫혀 있거나, 참가할 수 없을 때 처리할 로직 작성
        if (returnCode == ErrorCode.GameClosed)
        {
            findRoomPopup.OnPopup();
            // 예외 처리: 예를 들어, 재시도하거나 다른 방 검색
        }
        else if (returnCode == ErrorCode.GameFull)
        {
            notification.OnPopup();
            notification.SetContent("방이 가득 찼습니다.");

            PhotonNetwork.JoinLobby();
        }
        else
        {
            notification.OnPopup();
            notification.SetContent("방 참가에 실패했습니다...\n");

            PhotonNetwork.JoinLobby();
        }
    }

    //랜덤매칭 필터
    Hashtable randomFilter = new Hashtable()
    {
        {"isPrivate", false }
    };

    // Quick Enter 버튼 클릭 시
    private void OnClickQuickEnterButton()
    {
        if (PhotonNetwork.IsConnected)
        {
            // 비공개 방의 빠른 입장 제한
            PhotonNetwork.JoinRandomRoom(randomFilter, 0, MatchmakingMode.RandomMatching, TypedLobby.Default, null, null);
            // PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.Disconnect();
        }
    }

    // 랜덤 입장 실패 시 호출되는 콜백
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // 빠른 입장 실패 / 방 찾기 실패 시

        notification.OnPopup();
        notification.SetContent("방 참가에 실패했습니다.\n");

        // JoinLobby() 실행
        PhotonNetwork.JoinLobby();
    }

    public void OnPopup(string type)
    {
        switch (type)
        {
            case "CREATE":
                GlobalSoundManager.Instance.PlayClickSound();
                // 방 생성 팝업 활성화
                createRoomPopup.OnPopup();
                break;
            case "FIND":
                GlobalSoundManager.Instance.PlayClickSound();
                // 방 찾기 팝업 활성화
                findRoomPopup.OnPopup();
                break;
            case "QUICK ENTER":
                GlobalSoundManager.Instance.PlaySubmitSound();
                // 빠른 입장 시도
                OnClickQuickEnterButton();
                break;
        }
    }
}
