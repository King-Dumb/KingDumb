using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeType : MonoBehaviour
{
    public EffectType newEffectType; // 변경하고자 하는 스킬 타입 (Inspector에서 설정)

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 트리거에 진입했을 때
        Mage mage = other.GetComponent<Mage>(); // 트리거에 진입한 객체가 Mage인지 확인
        if (mage != null)
        {
            mage.ChangeSkillType(newEffectType);
            Debug.Log($"스킬 타입이 {newEffectType}으로 변경되었습니다.");
        }
    }
}
