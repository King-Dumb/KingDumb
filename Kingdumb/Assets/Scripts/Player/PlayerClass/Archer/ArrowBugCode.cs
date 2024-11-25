using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ArrowBug : MonoBehaviour
{
    private int _damage;
    private Vector3 dir;
    public float speed = 50f;

    public void SetDamage(int damage) {
        _damage = damage;
    }

    void Start()
    {
        SetDirection();
        Destroy(gameObject, 3f);
    }

    void SetDirection()
    {
        dir = Camera.main.transform.forward.normalized;
    }

    void Update()
    {
        // 화살을 dir 방향으로 speed 속도로 이동
        transform.position += speed * Time.deltaTime * dir;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Floor"))
        {
            Destroy(gameObject);
        }
    }
}
