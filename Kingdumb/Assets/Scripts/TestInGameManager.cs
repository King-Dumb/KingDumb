using UnityEngine;
using Cinemachine;
using Photon.Pun;

//테스트를 위한 임시 코드입니다.
public class TestInGameManager : MonoBehaviourPun
{
    public GameObject playerPrefab;
    public CinemachineVirtualCamera playerCameraPrefab;

    private GameObject player;
    private new CinemachineVirtualCamera camera;

    public static TestInGameManager Inst;

    void Awake()
    {
        //싱글톤 선언
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = Instantiate(playerPrefab);
        
        camera = FindObjectOfType<CinemachineVirtualCamera>();
        if (camera == null) {
            camera = Instantiate(playerCameraPrefab);
        }
        camera.Follow = player.transform;
        
        SetPlayerClass(GameConfig.WarriorClass);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))  // 1번 키를 누르면 전사로 변경
        {
            SetPlayerClass(GameConfig.WarriorClass);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))  // 2번 키를 누르면 궁수로 변경
        {
            SetPlayerClass(GameConfig.ArcherClass);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))  // 3번 키를 누르면 마법사로 변경
        {
            SetPlayerClass(GameConfig.MageClass);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))  // 4번 키를 누르면 사제로 변경
        {
            SetPlayerClass(GameConfig.PriestClass);
        }
    }

    public void SetPlayerClass(string className)
    {
        IPlayerClass pClass = null;

        //switch (className)
        //{
        //    case GameConfig.WarriorClass:
        //        Debug.Log("전사로 변경");
        //        pClass = new Warrior();
        //        break;
        //    case GameConfig.ArcherClass:
        //        Debug.Log("궁수로 변경");
        //        pClass = new Archer();
        //        break;
        //    case GameConfig.MageClass:
        //        Debug.Log("마법사로 변경");
        //        pClass = new Mage();
        //        break;
        //    case GameConfig.PriestClass:
        //        Debug.Log("사제로 변경");
        //        pClass = new Priest();
        //        break;
        //}

        //TODO : 이 때 직업이 바뀌니 플레이어의 모든 행동을 초기화해야함

        pClass = player.GetComponentInChildren<IPlayerClass>();
        player.GetComponentInChildren<PlayerController>().SetPlayerClass(pClass);
    }
}
