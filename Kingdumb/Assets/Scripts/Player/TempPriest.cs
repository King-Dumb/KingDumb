using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class TempPriest : MonoBehaviourPun, IPunObservable
{
    public TextMeshProUGUI playerNameText;
    private string playerName;

    void Awake()
    {
        playerName = PhotonNetwork.NickName;
        playerNameText.text = playerName;

        gameObject.SetActive(false);
    }

    public void SetCharacter(bool isActive)
    {
        photonView.RPC("RPCSetCharacter", RpcTarget.AllBuffered, isActive);
    }

    [PunRPC]
    private void RPCSetCharacter(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    // 닉네임 동기화를 위한 메서드
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // 로컬 오브젝트라면
        {
            // 내 플레이어 정보를 다른 플레이어에게 전송
            stream.SendNext(playerName);
            // stream.SendNext(transform.position);
            // stream.SendNext(transform.rotation);
        }
        else // 리모트 오브젝트라면
        {
            // 다른 플레이어 정보 수신 (보낸 순서대로 받아야 함)
            playerName = (string)stream.ReceiveNext();
            playerNameText.text = playerName; // UI 텍스트에 닉네임 업데이트
            // transform.position = (Vector3)stream.ReceiveNext();
            // transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
