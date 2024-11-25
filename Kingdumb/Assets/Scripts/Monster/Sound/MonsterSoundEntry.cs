using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterSoundEntry 
{
    public enum MonsterSoundType
    {
        Spawn,   // 생성
        Hit,     // 피격
        Death,   // 사망
        Attack   // 공격
    }

    public MonsterSoundType soundType; // 소리 유형
    public List<AudioClip> clips;            // 클립
}
