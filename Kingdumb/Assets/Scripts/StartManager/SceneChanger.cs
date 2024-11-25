using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

//SceneManager는 유니티에서 사용하는 이름이기 때문에 SceneChanger로 명명
public class SceneChanger : SingleTon<SceneChanger>
{
    public event Action<string> OnSceneLoadComplete;
    //private LoadingManager _loadingManager = null;
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //public IEnumerator LoadSceneAsync(string sceneName)
    //{
    //    // 비동기적으로 씬 로드 시작        
    //    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
    //    asyncLoad.allowSceneActivation = false;

    //    // 로딩이 완료될 때까지 대기
    //    while (!asyncLoad.isDone)
    //    {
    //        // 로딩 진행률을 표시 (0 to 1)
    //        //float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
    //        //Debug.Log($"로딩 중... {progress * 100}%");

    //        if (_loadingManager != null)
    //        {
    //            //Debug.Log($"로딩 중... {asyncLoad.progress * 100}%");
    //            _loadingManager.SetPercent(asyncLoad.progress * 100);
    //        }

    //        if (asyncLoad.progress >= 0.9f)
    //        {
    //            //Debug.Log("로딩 완료!");
    //            //로딩 완료시 콜백이벤트 호출                                       
    //            OnSceneLoadComplete?.Invoke(sceneName);

    //            asyncLoad.allowSceneActivation = true;
    //            _loadingManager = null;

    //            yield return null;

    //        }
    //        yield return null;  // 다음 프레임까지 대기            
    //    }
    //}

    //씬 로드가 "완전히" 완료된 후 수행하는 작업
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("씬로딩 완료" + scene.name);        

        switch (scene.name)
        {
            case "Title":
                break;
            case "Lobby":
                break;
            case "Room":
                //Debug.Log("Scene Changer");
                // 플레이어 생성
                // PhotonNetwork.Instantiate("TempPlayer", new Vector3(UnityEngine.Random.Range(0f, 5f), 1, UnityEngine.Random.Range(0f, 5f)), Quaternion.identity);
                // Debug.Log("Player 생성");
                CharacterManager.Inst.InitPlayer();
                break;
            case "InGame":
                PlayerStatisticsManager.Instance.InitializePlayerStatistics(); // 플레이어의 통계 테이블 생성
                //Debug.Log("SceneChanger Awake");
                // 동기적인 실행을 보장하기 위해 마스터에서 생성을 처리하고 브로드캐스트 해주는 방식으로 변경
                if (PhotonNetwork.IsMasterClient)
                {
                    IngameManager.Inst.CreateMonsterGenerator(1);                                                
                    CharacterManager.Inst.CreateNexus();
                    CharacterManager.Inst.InitPlayerAll();
                }
                break;
            case "InGame_2S":                
                if (PhotonNetwork.IsMasterClient)
                {
                    IngameManager.Inst.CreateMonsterGenerator(2);
                    CharacterManager.Inst.CreateNexus();                    
                    CharacterManager.Inst.InitPlayerAll();
                }               
                break;
            case "InGame_3S":
                if (PhotonNetwork.IsMasterClient)
                {
                    IngameManager.Inst.CreateMonsterGenerator(3);
                    CharacterManager.Inst.CreateNexus();
                    CharacterManager.Inst.InitPlayerAll();
                }
                break;
            default:
                //Debug.Log("씬 이름 찾을 수 없음");
                break;
        }

        GameManager.Instance.ClearObjPool();
    }
}
