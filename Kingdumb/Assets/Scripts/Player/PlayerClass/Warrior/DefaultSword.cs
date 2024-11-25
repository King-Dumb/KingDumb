using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultSword : MonoBehaviour
{
    public float defaultAttackDamage;
    public int playerPhotonViewId;
    public GameObject warriorCollisionEffect;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(defaultAttackDamage);
        if (other.CompareTag("Monster"))
        {
            IMonster targetMonster = other.GetComponent<IMonster>();
            if (targetMonster != null)
            {
                Vector3 collisionPosition = other.ClosestPoint(transform.position);

                GameObject collisionEffect = GameManager.Instance.Instantiate(warriorCollisionEffect.name, collisionPosition, Quaternion.identity);

                collisionEffect.transform.LookAt(transform.position);

                GameManager.Instance.Destroy(collisionEffect, 1f);

                targetMonster.OnDamage(defaultAttackDamage, false, transform.position, playerPhotonViewId);
            }
        }
    }
}
