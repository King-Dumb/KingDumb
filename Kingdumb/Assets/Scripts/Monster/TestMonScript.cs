using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMonScript : MonoBehaviour
{   
    public GameObject target;
    private Transform tr;

    public float moveSpeed = 5f;
    public float rotateSpeed = 10f;
    public float destroyTime = 3f; //충돌 후 소멸되는 시간

    private Rigidbody rb;

    private Vector3 direction;
    private Vector3 rotation;
    private Quaternion rotationTarget;

    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if(target != null)
        {
            tr = target.transform;
        }
    }

    private void FixedUpdate()
    {
        if (tr == null)
            return;

        direction = (tr.position - transform.position);
        direction.y = 0;
        direction = direction.normalized;
        
        //rigidbody가 있을때
        if(rb != null)
        {
            rotation = Vector3.Cross(transform.forward, direction);
            rb.angularVelocity = rotateSpeed * rotation;
            rb.velocity = transform.forward * moveSpeed;
        }
        else
        {            
            rotationTarget = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotationTarget, rotateSpeed * Time.fixedDeltaTime);
            transform.position += transform.forward * moveSpeed * Time.fixedDeltaTime;
        }        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("충돌");
        if(other.transform == target)
        {
            Die(1f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if(collision.collider.CompareTag("Missile"))
        {
            //Debug.Log("충돌2");
            Die(0f);
        }
        
    }

    public void SetTarget(GameObject obj)
    {
        target = obj;
        tr = target.transform;
    }

    public void Die(float _destroyTime = 1f)
    {        
        Destroy(gameObject, _destroyTime);
        MonsterGenerator.Inst.RemoveMonster(gameObject);
    }
}
