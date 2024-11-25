using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;

// 마법사형 스켈레톤
public class MageSkeleton : MonsterBase
{
    [Header("Projectile Info")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private float _projectileDuration;
    [SerializeField] private Transform projectileGenPos; // 투사체 생성지점

    public override void Attack()
    {
        ChangeState(MonsterState.Attacking);
        MonsterAnimationBroadcast("Attack");
        Invoke("OnAttackEnd", attackAnimationLength);
    }

    public void GenerateProjectile()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject projectile = PhotonNetwork.Instantiate("Monster/"+projectilePrefab.name, projectileGenPos.position, projectileGenPos.rotation);
            MonsterProjectile monsterProjectile = projectile.GetComponent<MonsterProjectile>();
            monsterProjectile.Initialize(monsterInfo.attackDamage, _projectileSpeed, _projectileDuration, _target, true, true);
        }
    }

    private void OnAttackEnd()
    {
        ChangeState(MonsterState.AttackReady);
    }
}
