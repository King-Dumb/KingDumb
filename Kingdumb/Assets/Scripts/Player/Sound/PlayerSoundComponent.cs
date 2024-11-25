using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerSoundEntry;

public class PlayerSoundComponent : MonoBehaviour
{
    private AudioSource _audioSource;

    [Header("Sound Clips")]
    public List<PlayerSoundEntry> soundEntries; // 상황별 소리 설정 (Inspector에서 관리)

    private Dictionary<PlayerSoundType, List<AudioClip>> soundDictionary;

    private void Awake()
    {
        // AudioSource 초기화
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Dictionary 초기화
        soundDictionary = new Dictionary<PlayerSoundType, List<AudioClip>>();
        foreach (var entry in soundEntries)
        {
            soundDictionary[entry.soundType] = entry.clips;
        }
    }

    public void PlaySound(PlayerSoundType soundType, float pitch = 1.0f, float volume = 1.0f)
    {
        //Debug.Log(gameObject.name);
        try
        {
            if (soundDictionary.ContainsKey(soundType) && soundDictionary[soundType].Count > 0)
            {
                // 랜덤하게 클립 선택
                List<AudioClip> clips = soundDictionary[soundType];
                AudioClip clip = clips[UnityEngine.Random.Range(0, clips.Count)];
                _audioSource.pitch = pitch;   // 피치 조정
                _audioSource.volume = volume; // 볼륨 조정
                PlayerSoundManager.Instance.PlaySound(_audioSource, clip); // 클립 재생
            }
            // else
            // {
            //     Debug.LogWarning($"No clips found for sound type {soundType} in PlayerSoundComponent.");
            // }
        }
        catch (NullReferenceException e)
        {
            return;
        }
    }
}
