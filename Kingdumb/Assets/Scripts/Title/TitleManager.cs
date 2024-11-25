using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviourPunCallbacks
{
    public TitlePopup titlePopup;

    public static TitleManager Inst;

    void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 게임 실행과 동시에 마스터 서버 접속 시도
    private void Start()
    {
        // 접속 시도 중임을 텍스트로 표시
        titlePopup.SetConnectInfo("서버 연결 중...");

        // 서버 연결
        PhotonManager.Connect();
    }

    private void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked) // 커서가 잠긴 상태면 풀기
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // 마스터 서버 접속 실패 시 재접속 시도
    // public override void OnDisconnected(DisconnectCause cause)
    // {
    //     //Debug.Log("서버 연결 실패");

    //     // 접속 정보 표시
    //     titlePopup.SetConnectInfo("서버 연결 실패\n서버에 재연결 중...");

    //     // 마스터 서버로의 재접속 시도

    //     PhotonManager.Connect();
    // }

    public void SetTitleInfo()
    {
        // 접속 정보 표시
        titlePopup.SetConnectInfo("서버에 연결했습니다!");

        // 게임 버전 표시
        titlePopup.SetGameVersion("Version " + PhotonNetwork.GameVersion);

        // 게임 시작 버튼 활성화
        titlePopup.OnStartButton();
    }
}
