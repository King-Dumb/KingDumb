using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterWeapon : MonoBehaviour
{
    private float _attackDamage;
    private int _ownerViewId;
    private PhotonView _ownerPhotonView; // ApplyHitEffect를 호출하는 photonView

    public void Initialize(float attackDamage, int ownerViewId)
    {
        _attackDamage = attackDamage;
        _ownerViewId = ownerViewId;
    }

    public void Initialize(float attackDamage, int ownerViewId, PhotonView ownerPhotonView)
    {
        Initialize(attackDamage, ownerViewId);
        _ownerPhotonView = ownerPhotonView;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Nexus"))
        {
            IDamageable targetObject = other.gameObject.GetComponent<IDamageable>();
            if (targetObject != null)
            {
                if (_ownerPhotonView != null)
                {
                    _ownerPhotonView.RPC("ApplyHitEffect", RpcTarget.All, other.ClosestPoint(transform.position));
                }
                targetObject.OnDamage(_attackDamage, false, other.ClosestPoint(transform.position), _ownerViewId);
            }
        }
    }
}
