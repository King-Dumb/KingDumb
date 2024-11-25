using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class NicknameSync : MonoBehaviourPun
{
    public GameObject nicknamePanel;
    public TextMeshProUGUI playerNameText;
    private string playerName;
    private Transform mainCameraTransform;

    void Awake()
    {
        mainCameraTransform = Camera.main.transform;
        if (photonView.IsMine)
        {
            playerName = PhotonNetwork.LocalPlayer.NickName;
        }
        playerNameText.text = playerName;
    }

    [PunRPC]
    void SyncNickname(string name)
    {
        playerName = name;
        playerNameText.text = name;
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Room")
        {
            nicknamePanel.SetActive(false);
        }

        if (photonView.IsMine)
        {
            photonView.RPC("SyncNickname", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName);
            nicknamePanel.SetActive(false);
        }
    }

    // public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    // {
    //     if (stream.IsWriting)
    //     {
    //         stream.SendNext(playerName);
    //     }
    //     else
    //     {
    //         playerName = (string)stream.ReceiveNext();
    //         playerNameText.text = playerName;
    //     }
    // }

    void LateUpdate()
    {
        playerNameText.transform.parent.rotation = mainCameraTransform.rotation;

        // 거리 기반 크기 조절
        float distance = Vector3.Distance(transform.position, mainCameraTransform.position);
        playerNameText.transform.localScale = Vector3.one * Mathf.Clamp(1 / distance, 0.5f, 2f);
    }
}
