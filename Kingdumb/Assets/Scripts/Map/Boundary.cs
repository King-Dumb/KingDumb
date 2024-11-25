using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    private GameObject targetTeleportObject;
    private Vector3 knockbackDirection;
    private float pushDuration = 0.1f;
    private float pushRemainTime = 0.1f;
    private float knockbackForce = 30f;

    private void Update()
    {
        if (pushDuration > pushRemainTime)
        {
            pushRemainTime += Time.deltaTime;
            PushEntity();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        targetTeleportObject = other.gameObject;
        knockbackDirection = (Vector3.zero - other.transform.position).normalized;
        //Debug.Log("OnTrigger 호출됨");
        pushRemainTime = 0f;
    }

    private void PushEntity()
    {
        //Debug.Log("PushEntity 호출됨");
        targetTeleportObject.transform.position += knockbackDirection * knockbackForce * Time.deltaTime;
    }
}
