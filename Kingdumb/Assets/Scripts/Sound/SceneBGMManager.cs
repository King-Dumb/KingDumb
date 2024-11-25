using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 추후 성공, 실패 등 여러 오디오 클립 추가 예정
public class SceneBGMManager : MonoBehaviourPun
{
    [SerializeField]
    private AudioClip _bgmClip;
    
    [SerializeField]
    private List<AudioClip> _audioClipList;


    private void Start()
    {
        //Debug.Log("ScneneBGMManager 시작");
        if (GlobalSoundManager.Instance != null && _bgmClip != null)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "InGame" || sceneName == "InGame_2S")
            {
                GlobalSoundManager.Instance.PlayBGM(_bgmClip, 1.0f, 0.7f); // 1초 페이드 전환
            }
            else
            {
                GlobalSoundManager.Instance.PlayBGM(_bgmClip, 1.0f); // 1초 페이드 전환
            }
        }
        // else
        // {
        //     Debug.LogWarning("GlobalBGMManager나 BGM 클립이 설정되지 않았습니다.");
        // }
    }

    public void PlaySceneSound(int index, float volume = 1.0f)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        

        GlobalSoundManager.Instance.PlaySound(_audioClipList[index], volume);
    }
}
