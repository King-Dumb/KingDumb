using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomChat : MonoBehaviourPunCallbacks
{
    public TMP_InputField inputChat;
    public TextMeshProUGUI chatLog;

    public ScrollRect scrollRect;

    private const byte _chatEventCode = 5;

    public Button sendButton;

    void Start()
    {
        sendButton.onClick.AddListener(SendMessageInRoom);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessageInRoom();
            inputChat.ActivateInputField();
        }
    }

    public void SendMessageInRoom()
    {
        string msg = inputChat.text;

        //Debug.Log("msg : " + msg);

        // _myPlayerColor = PhotonManager.Inst.GetPlayerThemeColorCode();                
        if (!string.IsNullOrEmpty(msg))
        {
            string coloredMsg = $"<b><color={GameConfig.UserColor}>{GameConfig.UserNickName} :</color></b> {msg}";
            object content = coloredMsg;

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            };
            PhotonNetwork.RaiseEvent(_chatEventCode, content, raiseEventOptions, SendOptions.SendReliable);

            inputChat.text = "";
        }
    }

    public void SetInfoMessage(string msg)
    {
        msg = string.Format($"<color=#00ff00>{msg}</color>");
        object content = msg;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };
        PhotonNetwork.RaiseEvent(_chatEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    // 이벤트 수신 처리
    public override void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == _chatEventCode)
        {
            string receivedMessage = (string)photonEvent.CustomData;
            chatLog.text += receivedMessage + "\n";
            // chatLog.text += receivedMessage;

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
