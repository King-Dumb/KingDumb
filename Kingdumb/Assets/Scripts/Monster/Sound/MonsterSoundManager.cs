using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSoundManager : MonoBehaviour
{
    public static MonsterSoundManager Instance;

    private int currentPlayingSounds = 0; // 현재 재생 중인 소리 수
    public int maxSimultaneousSounds = 5; // 최대 동시 재생 소리 수
    public float baseVolume = 1.0f; // 기본 볼륨

    private List<AudioSource> activeAudioSources = new List<AudioSource>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioSource source, AudioClip clip)
    {
        // 동시 재생 제한
        if (currentPlayingSounds >= maxSimultaneousSounds)
        {
            //Debug.LogWarning("Maximum simultaneous sounds reached.");
            return;
        }

        // 볼륨 조정
        AdjustVolume();

        // 소리 재생
        source.clip = clip;
        source.Play();
        currentPlayingSounds++;
        activeAudioSources.Add(source);

        // 재생 완료 후 소리 제거
        StartCoroutine(RemoveSourceAfterDelay(source, clip.length));
    }

    private void AdjustVolume()
    {
        float adjustedVolume = baseVolume / Mathf.Max(1, currentPlayingSounds + 1); // 동시 재생 개수에 비례한 볼륨 감소

        foreach (AudioSource source in activeAudioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.volume = adjustedVolume; // 기존 소리 볼륨도 조정
            }
        }
    }

    private IEnumerator RemoveSourceAfterDelay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        activeAudioSources.Remove(source);
        currentPlayingSounds--;
        AdjustVolume(); // 소리가 종료되면 볼륨 재조정
    }
}