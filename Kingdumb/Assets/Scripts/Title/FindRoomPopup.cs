using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class FindRoomPopup : MonoBehaviourPunCallbacks
{
    [Header("Find")]
    public TMP_InputField roomCodeInputField; // 방 코드 입력
    public TextMeshProUGUI roomCodeValidationInfo; // 방 코드 유효성 검사 정보
    public Button findCancelButton;
    public Button findRoomButton;

    private void Awake()
    {
        OffPopup();
        roomCodeValidationInfo.text = "";
        findCancelButton.onClick.AddListener(OnClickCancel);
        findRoomButton.onClick.AddListener(OnClickFindRoom);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnClickFindRoom();
        }
    }

    void JoinRoomByCode(string roomCode)
    {
        // 방 코드를 기준으로 랜덤 방에 입장 시도
        ExitGames.Client.Photon.Hashtable expectedRoomProperties = new ExitGames.Client.Photon.Hashtable { { "roomCode", roomCode } };

        //Debug.Log("Photon JoinRandomRoom");


        PhotonNetwork.JoinRandomRoom(expectedRoomProperties, 4);
    }

    // 방 찾기 메서드
    private void OnClickFindRoom()
    {
        GlobalSoundManager.Instance.PlaySubmitSound(); // 확인 효과음 재생

        // 코드 미 입력 시 알림
        if (string.IsNullOrEmpty(roomCodeInputField.text))
        {
            roomCodeValidationInfo.text = "코드를 입력하세요.";
        }
        else
        {
            // 방 참가 시도
            if (PhotonNetwork.IsConnected)
            {
                JoinRoomByCode(roomCodeInputField.text); // 방 코드로 입장 가능하도록 커스텀
            }
            else
            {
                roomCodeValidationInfo.text = "방 찾기에 실패했습니다.\n서버에 다시 연결하는 중...";
                // 재접속 시도 or 타이틀 씬 로드
                //Debug.Log("방 찾기 실패! 재접속 시도");
                PhotonNetwork.Disconnect();
            }
        }
    }

    private void OnClickCancel()
    {
        roomCodeInputField.text = "";
        roomCodeValidationInfo.text = "";
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
