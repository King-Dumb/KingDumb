using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class InGameChat : MonoBehaviourPunCallbacks
{    
    public TMP_InputField inputChat;
    public TextMeshProUGUI chatLog;
    public ScrollRect scrollRect;
    public GameObject background;
    public GameObject scroller;

    private const byte _chatEventCode = 5;

    private bool _isChat = false;

    void Start()
    {
        inputChat.interactable = false;
        background.SetActive(false);
        scroller.SetActive(false);
    }
    
    void Update()
    {
        //(toggle) 엔터키를 누르면 인풋 활성화 + 창 최대화
        //다시 엔터 누르면 줄어듬      
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if(!string.IsNullOrEmpty(inputChat.text))
            {
                SendMessageInGame(inputChat.text);
                _isChat = false;                
            }
            else
            {
                _isChat = !_isChat;                
            }
            ChatToggle(_isChat);
        }
    }

    private void SendMessageInGame(string msg)
    {
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

    //알림을 주고 싶을 때 사용하는 함수
    /*
     * ex) 자기장이 형성됩니다! , A 플레이어가 B를 처치했습니다.
     */ 
    public void SendSystemMessage(string msg)
    {
        if(PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            string coloredMsg = $"<b><color=#2F00FF>[Info] {msg}</color></b>";

            object content = coloredMsg;

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            };
            PhotonNetwork.RaiseEvent(_chatEventCode, content, raiseEventOptions, SendOptions.SendReliable);
        }      
    }

    private void ChatToggle(bool enable)
    {
        //창을 키우고 인풋을 활성화
        if (enable)
        {
            inputChat.interactable = true;
            inputChat.ActivateInputField();
            background.SetActive(true);
            scroller.SetActive(true);
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
        else
        {
            //메시지를 보내고 창을 닫기
            SendMessageInGame(inputChat.text);
            background.SetActive(false);
            scroller.SetActive(false);
            inputChat.interactable = false;
            inputChat.DeactivateInputField();
        }
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

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
