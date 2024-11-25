using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingSword : MonoBehaviour
{
    private Vector3 initialPosition;
    private Vector3 rotation;
    private Vector3 fixedDirection;
    private float travelProgress = 0f;
    private float maxDistance = 5f;
    private float speed = 5f;

    public GameObject defaultSword;
    public float throwingDamage;
    public int playerPhotonViewId;
    public float curveAmount = 10f; // 스킬트리 공격 사거리 증가 시 10f를 더함

    public GameObject warriorCollisionEffect;

    void OnEnable()
    {
        defaultSword.SetActive(false);
        initialPosition = transform.parent.position;
        travelProgress = 0f;
        fixedDirection = transform.parent.forward;
    }

    void Update()
    {
        // 검 회전
        rotation = transform.eulerAngles;
        rotation.y += Time.deltaTime * 3000;
        transform.eulerAngles = rotation;

        // 플레이어로 돌아오는 곡선 이동
        travelProgress += speed * Time.deltaTime / maxDistance;
        float curvedX = Mathf.Sin(travelProgress * Mathf.PI) * curveAmount;
        transform.position = Vector3.Lerp(initialPosition, transform.parent.position, travelProgress) + fixedDirection * curvedX;

        // 플레이어에게 도달하면 부메랑 이동 종료
        if (travelProgress >= 1f)
        {
            defaultSword.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            IMonster targetMonster = other.GetComponent<IMonster>();
            if (targetMonster != null)
            {
                Vector3 collisionPosition = other.ClosestPoint(transform.position);

                GameObject collisionEffect = GameManager.Instance.Instantiate(warriorCollisionEffect.name, collisionPosition, Quaternion.identity);

                collisionEffect.transform.LookAt(transform.position);

                GameManager.Instance.Destroy(collisionEffect, 1f);

                targetMonster.OnDamage(throwingDamage, false, transform.position, playerPhotonViewId);
            }
        }
    }
}