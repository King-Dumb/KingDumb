using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateEffect : MonoBehaviour
{
    public float ultimateDamage;
    public int playerPhotonViewId;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            IMonster targetMonster = other.GetComponent<IMonster>();
            if (targetMonster != null)
            {
                targetMonster.OnDamage(ultimateDamage, false, transform.position, playerPhotonViewId);
            }
        }
    }
}
