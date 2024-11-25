using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TestPhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Inst { get; private set; }   

    void Start()
    {        
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("테스트 모드로 포톤 연결");
            PhotonNetwork.GameVersion = "1";
            PhotonNetwork.ConnectUsingSettings();
        }        
    }

    public override void OnConnectedToMaster()
    {        
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        //Debug.Log("로비 연결됨");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedRoom()
    {
        //Debug.Log("방 참가함");

        //if (IngameManager.Inst != null)
        //    IngameManager.Inst.SetPlayer(0, Vector3.zero, Quaternion.identity);

        //if (GameManager.Instance.localPlayer == null)
        //{
        //    IngameManager.Inst.Init();
        //}
    }
}
