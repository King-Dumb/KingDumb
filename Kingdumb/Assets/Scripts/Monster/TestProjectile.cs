using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TestProjectile : MonoBehaviour
{
    public GameObject targetedMonster;
    
    public Vector3 knockbackDirection;
    public  float KnockbackStrength = 100f;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("실행됨 1");
            // 원하는 이벤트 함수 호출
            IDamageable target = targetedMonster.GetComponent<IDamageable>();
            IMonster monster = targetedMonster.GetComponent<IMonster>();
        
            if (target != null)
            {
                Debug.Log("실행됨 2");
                
                
                // Y축으로 충분히 뜨도록 Vector3.up 값 조정
                Vector3 knockbackForceVector = knockbackDirection * KnockbackStrength;
                monster.Knockback(knockbackForceVector);
                Debug.Log("실행됨3");
                
            }
        }
    }

}
