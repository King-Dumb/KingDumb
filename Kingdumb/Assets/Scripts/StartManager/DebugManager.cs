using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;

//디버그용 모니터, 실제 서비스 시 비활성화
public class DebugManager : SingleTon<DebugManager>
{    
    [SerializeField]
    private TextMeshProUGUI sceneStatusText; //현재 씬이 어디에 있는지 표기
    [SerializeField]
    private TextMeshProUGUI connectStatusText; //Photon 연결 상태를 표기
    [SerializeField]
    private TextMeshProUGUI networkStatusText; //네트워크 상태를 표기 (lobby, room..)
    [SerializeField]
    private TextMeshProUGUI pingStatusText; //네트워크 핑 상태를 표기
    [SerializeField]
    private TextMeshProUGUI fpsStatusText;// 프레임 상태를 표기
    [SerializeField]
    private TextMeshProUGUI memoryStatusText; //메모리 사용량을 표기
    [SerializeField]
    private TextMeshProUGUI infoText; //적고싶은 메시지를 넣고 싶을때 표기

    private static float deltaTime = 0.0f;

    void Start()
    {        
        Application.targetFrameRate = GameConfig.FPS;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateConnectionStatus();
        UpdatePingStatus();
        UpdateSceneNameStatus();
        UpdateFPSStatus();
        UpdateMemoryStatus();
    }

    private void UpdateConnectionStatus()
    {
        if (PhotonNetwork.IsConnected)
        {
            connectStatusText.text = "Connected";
            if (PhotonNetwork.InRoom)
            {
                networkStatusText.text = "In Room";
            }
            else if (PhotonNetwork.InLobby)
            {
                networkStatusText.text = "In Lobby";
            }
            else
            {
                networkStatusText.text = "None";
            }
        }
        else
        {
            connectStatusText.text = "Disconnected";
            networkStatusText.text = "None";
        }
    }

    private void UpdatePingStatus()
    {
        if (PhotonNetwork.IsConnected)
        {
            pingStatusText.text = $"Ping: {PhotonNetwork.GetPing()} ms";
        }
        else
        {
            pingStatusText.text = "Ping: N/A";
        }
    }

    private void UpdateSceneNameStatus()
    {
        sceneStatusText.text = $"Scene: {SceneManager.GetActiveScene().name}";
    }

    private void UpdateFPSStatus()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        fpsStatusText.text = $"FPS : {fps}";
    }

    private void UpdateMemoryStatus()
    {
        long memory = System.GC.GetTotalMemory(false) / (1024 * 1024);
        memoryStatusText.text = $"Memory: {memory}MB";
    }

    //메시지를 입력하고 싶은게 있으면 입력
    public void SetInfoText(string text)
    {
        infoText.text = text;
    }
}
