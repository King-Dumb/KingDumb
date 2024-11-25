using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSoundEntry 
{
    public enum PlayerSoundType
    {
        Hit,     // 피격
        Death,   // 사망
        Attack,   // 공격
        Skill,   // 스킬
        Ultimate,   // 궁극기
    }

    public PlayerSoundType soundType; // 소리 유형
    public List<AudioClip> clips;            // 클립
}
