using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PriestAttack : MonoBehaviour
{
    private int ownerPvId;
    private Vector3 dir;
    public float speed;
    private float damage;
    public GameObject novaEffect;

    void Start()
    {
        GameManager.Instance.Destroy(gameObject, 3.0f);
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.position += dir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("충돌 대상 : " + other.gameObject.name);

        // 충돌 대상이 몬스터라면 데미지주고 넉백 처리 후 이펙트 적용
        if (other.CompareTag("Monster"))
        {
            //Debug.Log("몬스터와 충돌");

            // 충돌 대상의 데미지 적용
            IDamageable target = other.GetComponent<IDamageable>();
            if (target != null)
            {
                //Debug.Log("사제 공격력 : " + damage);
                target.OnDamage(damage, false, other.transform.position, ownerPvId);
            }

            // 충돌 대상의 넉백 적용
            IMonster cc = other.GetComponent<IMonster>();
            if (cc != null)
            {
                // // 투사체의 진행 방향을 사용하여 넉백 방향 설정 (transform.forward로 변경)
                // Vector3 knockbackDirection = transform.forward.normalized;


                // Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                // knockbackDirection = (knockbackDirection + Vector3.up) * 3f;
                // // Y축으로 충분히 뜨도록 Vector3.up 값 조정
                // Vector3 knockbackForceVector = knockbackDirection * 2f + Vector3.up * 2f;  // Y축 값을 증가
                // // 넉백 강도 설정
                // float knockbackStrength = 5f;
                // Vector3 knockbackForce = knockbackDirection * knockbackStrength;

                cc.Knockback(dir * 2f);

                // 충돌 이펙트 적용
                GameManager.Instance.Instantiate(novaEffect.name, other.transform.position, Quaternion.identity);

                // 투사체 삭제
                GameManager.Instance.Destroy(gameObject);
            }
        }
    }

    public void SetOwnerPvId(int id)
    {
        ownerPvId = id;
    }

    public void SetDirection(Vector3 direction)
    {
        dir = direction.normalized;
    }

    public void SetDamage(float dam)
    {
        damage = dam;
    }

    // 속도 변경을 위한 메서드 추가
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
