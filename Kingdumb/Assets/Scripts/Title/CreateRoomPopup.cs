using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class CreateRoomPopup : MonoBehaviourPunCallbacks
{
    [Header("Create")]
    public TMP_InputField roomNameInputField; // 방 제목 입력
    public Toggle toggle; // 비공개 여부 지정 토글
    private bool isPrivate; // 비공개 여부 (초기에는 false)
    public TextMeshProUGUI roomNameValidationInfo; // 방 이름 유효성 검사 정보
    public Button createCancelButton;
    public Button createRoomButton;

    private void Awake()
    {
        OffPopup();
        roomNameValidationInfo.text = "";
        createCancelButton.onClick.AddListener(OnClickCancel);
        createRoomButton.onClick.AddListener(OnClickCreateRoom);
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnClickCreateRoom();
        }
    }

    // 방 생성 메서드
    public void OnClickCreateRoom()
    {
        GlobalSoundManager.Instance.PlaySubmitSound(); // 확인 효과음 재생

        if (PhotonNetwork.IsConnected && PhotonNetwork.InLobby)
        {
            // 방 이름 유효성 검사
            ValidateAndFilterInput(roomNameInputField.text);

            if (!string.IsNullOrEmpty(roomNameInputField.text))
            {

                // RoomOptions 설정
                RoomOptions roomOptions = new RoomOptions();
                // Custom properties 시작여부 설정
                string roomCode = GameConfig.GenerateRoomCode();
                ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
                roomProps["isPrivate"] = isPrivate;
                roomProps["roomCode"] = roomCode;
                roomOptions.CustomRoomProperties = roomProps;
                // // 커스텀 프로퍼티에 방 코드를 저장
                // roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable { { "roomCode", roomCode } };
                // // 방 검색 시 사용할 키 목록 지정 (Photon에서 인덱싱할 프로퍼티)
                roomOptions.CustomRoomPropertiesForLobby = new string[] { "roomCode", "isPrivate" };
                roomOptions.MaxPlayers = GameConfig.MaxPlayersInRoom; // 최대 플레이어 수 설정
                roomOptions.IsOpen = true; // 방이 열려 있는지 여부
                roomOptions.IsVisible = true; // 방이 로비에서 보이는지 여부

                // 방 생성 시도
                //Debug.Log("Photon CreateRoom");
                PhotonNetwork.CreateRoom(roomNameInputField.text + "#" + roomCode, roomOptions);
            }
        }
    }

    // 입력의 유효성을 검사하고 필터링하는 메서드
    private void ValidateAndFilterInput(string input)
    {
        if (input.Length == 0)
        {
            roomNameInputField.text = "Room" + Random.Range(0, 1000);
            return;
        }

        string filteredInput = "";
        int length = 0;

        foreach (char c in input)
        {
            //Debug.Log("한글 ? " + GameConfig.IsKorean(c));
            //Debug.Log("한글 ? " + c);
            // 한글, 영문, 숫자인지 체크
            if (GameConfig.IsKorean(c) || GameConfig.IsEnglish(c) || GameConfig.IsNumber(c))
            {
                length += 1;

                // 길이가 20자를 넘지 않으면 입력 유지
                if (length <= 20)
                {
                    filteredInput += c;
                }
                else
                {
                    roomNameValidationInfo.text = "20글자 내로 입력하세요.";
                    roomNameInputField.text = "";
                    break;
                }
            }
            else
            {
                roomNameValidationInfo.text = "한글, 영문, 숫자만 입력하세요.";
                roomNameInputField.text = "";
            }
        }
    }

    // Toggle 상태 변경 시 호출될 메서드
    private void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            isPrivate = true;
        }
        else
        {
            isPrivate = false;
        }
    }

    // 방 생성 성공 시
    public override void OnCreatedRoom()
    {
        //Debug.Log("방 생성 성공!");
    }

    // 방 생성 실패 시
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        //Debug.LogError("방 생성 실패 : " + message);
    }

    private void OnClickCancel()
    {
        roomNameInputField.text = "";
        roomNameValidationInfo.text = "";
        gameObject.SetActive(false);
    }

    public void OnPopup()
    {
        gameObject.SetActive(true);
    }

    public void OffPopup()
    {
        gameObject.SetActive(false);
    }
}
