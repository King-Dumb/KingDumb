using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class TitlePopup : MonoBehaviour
{
    public TextMeshProUGUI connectionInfoText; // 네트워크 정보를 표시할 텍스트
    public Button startButton; // 게임 시작 버튼
    public TextMeshProUGUI gameVersion; // 게임 버전
    public LoginPopup loginPopup;

    private void Awake()
    {
        gameObject.SetActive(true);
        startButton.onClick.AddListener(OnClickStartButton);
        startButton.gameObject.SetActive(false);
    }

    // 게임 시작 버튼 클릭 시 로그인 팝업
    public void OnClickStartButton()
    {
        // 타이틀 팝업 비활성화
        OffPopup();

        // 로그인 팝업 활성화
        loginPopup.OnPopup();

        // if(PhotonNetwork.IsConnected)
        //     PhotonNetwork.JoinLobby();
    }

    public void SetConnectInfo(string text)
    {
        connectionInfoText.text = text;
    }

    public void OnPopup()
    {
        gameObject.SetActive(true);
    }

    public void OffPopup()
    {
        gameObject.SetActive(false);
    }

    public void OnStartButton()
    {
        startButton.gameObject.SetActive(true);
    }

    public void OffStartButton()
    {
        startButton.gameObject.SetActive(false);
    }

    public void SetGameVersion(string text)
    {
        gameVersion.text = text;
    }
}
