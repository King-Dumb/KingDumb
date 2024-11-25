using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageUp : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 트리거에 진입했을 때
        Mage mage = other.GetComponent<Mage>(); // 트리거에 진입한 객체가 Mage인지 확인
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (mage != null)
        {
            Debug.Log("DAMAGE UP!!");
            mage.AttackDamageUp(1f);
            mage.AttackSplashDamageUp(1f);
            mage.ApplyKillCooldownReduction();
        }
        
    }
}
