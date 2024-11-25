using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalSoundManager : SingleTon<GlobalSoundManager>
{
    [SerializeField] private AudioSource bgmSource; // BGM 전용 AudioSource
    [SerializeField] private AudioSource effectSource; // 효과음 전용 AudioSource
    [SerializeField] private AudioClip impactSound;     // 묵직한 느낌의 효과음 (현재는 닉네임 입력 후 입장에 사용)
    [SerializeField] private AudioClip clickSound;     // 클릭음 효과음
    [SerializeField] private AudioClip submitSound;     // 선택, 확정 효과음 (생성, 확인 등)
    [SerializeField] private AudioClip battleStartSound; // 전투 시작 시 사용하는 효과음
    [SerializeField] private AudioClip uiActiveSound; // UI 활성화 비활성화 시 사용하는 소리
    [SerializeField] private AudioClip buySound; // 구매 시 효과음
    [SerializeField] private AudioClip warningSound; // 경고음 소리
    [SerializeField] private AudioClip stageClearSound; // 스테이지 클리어 소리
    [SerializeField] private AudioClip winningSound; // 게임 전체 클리어 소리
    [SerializeField] private AudioClip failureSound; // 게임 실패 클리어 소리
    [SerializeField] private AudioClip princeCarrySound; //  왕자님 안기 소리

    private bool isBgmOn = true;

    // 창이 포커스를 잃었을 때 음소거하는 코드
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            AudioListener.volume = Mathf.Clamp01(1);
        }
        else
        {
            AudioListener.volume = Mathf.Clamp01(0);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Insert))
        {
            if (isBgmOn)
            {
                bgmSource.volume = 0f;
                isBgmOn = false;
            }
            else
            {
                bgmSource.volume = 1f;
                isBgmOn = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (AudioListener.volume == 0f)
            {
                AudioListener.volume = 1f;
            }
            else
            {
                AudioListener.volume = 0f;
            }
        }
    }

    private void Start()
    {
        //Debug.Log("GlobalSoundManagerStart");   
        bgmSource.loop = true;
    }

    public void PlayBGM(AudioClip bgmClip, float fadeDuration = 1.0f, float maxVolume = 1.0f)
    {
        ////Debug.Log(bgmSource.clip.name);
        //Debug.Log(bgmClip.name);
        if (bgmSource.clip == bgmClip) return; // 동일한 BGM이면 무시
        StartCoroutine(FadeOutAndPlayNewBGM(bgmClip, fadeDuration, maxVolume));
    }

    private IEnumerator FadeOutAndPlayNewBGM(AudioClip newClip, float fadeDuration, float maxVolume)
    {
        float startVolume = bgmSource.volume;

        startVolume = maxVolume;
        // 페이드 아웃
        while (bgmSource.volume > 0)
        {
            bgmSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();

        // bgm이 켜져 있다면
        if (isBgmOn)
        {
            // 페이드 인
            while (bgmSource.volume < startVolume)
            {
                bgmSource.volume += startVolume * Time.deltaTime / fadeDuration;
                yield return null;
            }
        }
    }

    public void PlayImpactSound()
    {
        if (impactSound != null && effectSource != null)
        {
            effectSource.PlayOneShot(impactSound); // 클릭음 재생
        }
        // else
        // {
        //     //Debug.LogWarning("효과음 또는 AudioSource가 설정되지 않았습니다.");
        // }
    }

    public void PlayClickSound()
    {
        if (clickSound != null && effectSource != null)
        {
            effectSource.PlayOneShot(clickSound); // 클릭음 재생
        }
        // else
        // {
        //     //Debug.LogWarning("클릭음 또는 AudioSource가 설정되지 않았습니다.");
        // }
    }

    public void PlaySubmitSound()
    {
        if (submitSound != null && effectSource != null)
        {
            effectSource.PlayOneShot(submitSound); // 클릭음 재생
        }
        // else
        // {
        //     //Debug.LogWarning("확인음 또는 AudioSource가 설정되지 않았습니다.");
        // }
    }

    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null && effectSource != null)
        {
            effectSource.PlayOneShot(clip, volume); // 전달받은 클립 재생
        }
        // else
        // {
        //     //Debug.LogWarning("재생할 AudioClip 또는 AudioSource가 설정되지 않았습니다.");
        // }
    }

    public void PlayBattleStartSound()
    {
        if (battleStartSound != null && effectSource != null)
        {
            effectSource.PlayOneShot(battleStartSound, 0.5f); // 클릭음 재생
        }
        // else
        // {
        //     //Debug.LogWarning("시작음 또는 AudioSource가 설정되지 않았습니다.");
        // }
    }

    public void PlayUIActiveSound()
    {
        if (uiActiveSound != null && effectSource != null)
        {
            effectSource.PlayOneShot(uiActiveSound); // 클릭음 재생
        }
        // else
        // {
        //     //Debug.LogWarning("ui음 또는 AudioSource가 설정되지 않았습니다.");
        // }
    }
    public void PlayBuySound()
    {
        if (buySound != null && effectSource != null)
        {
            effectSource.PlayOneShot(buySound); // 클릭음 재생
        }
        // else
        // {
        //     //Debug.LogWarning("ui음 또는 AudioSource가 설정되지 않았습니다.");
        // }
    }

    public void PlayWarningSound()
    {
        if (warningSound != null && effectSource != null)
        {
            StartCoroutine(PlayAudioForDuration(3f));
        }
        // else
        // {
        //     //Debug.LogWarning("경고음 또는 AudioSource가 설정되지 않았습니다.");
        // }
    }

    IEnumerator PlayAudioForDuration(float duration)
    {
        effectSource.clip = warningSound;
        effectSource.Play(); // 재생 시작
        yield return new WaitForSeconds(duration); // 설정한 시간 동안 대기
        effectSource.Stop(); // 재생 중지
    }

    public void PlayStageClearSound()
    {
        if (stageClearSound != null && effectSource != null)
        {
            effectSource.PlayOneShot(stageClearSound, 0.8f); // 클리어음 재생
        }
        // else
        // {
        //     //Debug.LogWarning("스테이지 클리어음 또는 AudioSource가 설정되지 않았습니다.");
        // }
    }

    public void PlayWinningSound()
    {
        //Debug.Log("WinningSound Call");
        bgmSource.clip = winningSound;
        bgmSource.Play();
    }

    public void PlayFailureSound()
    {
        //Debug.Log("FailureSound Call");
        bgmSource.clip = failureSound;
        bgmSource.Play();
    }

    public void PlayPrinceCarrySound()
    {
        if (princeCarrySound != null && effectSource != null)
        {
            effectSource.clip = princeCarrySound; // 클릭음 재생
            effectSource.volume = 0.6f;
            effectSource.Play();
        }
    }

    public void StopEffectSourceClip()
    {
        effectSource.volume = 1.0f;
        effectSource.Stop();
    }
}
