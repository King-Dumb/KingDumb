using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;

// 기본형의 스켈레톤
public class BasicSkeleton : MonsterBase
{
    [SerializeField] private MonsterWeapon monsterWeapon;

    private BoxCollider weaponCollider;

    protected override void Awake()
    {
        base.Awake();
        weaponCollider = monsterWeapon.GetComponent<BoxCollider>();
        
        monsterWeapon.Initialize(monsterInfo.attackDamage, _viewId); // 의존성 주입
    }

    public override void Attack()
    {
        ChangeState(MonsterState.Attacking);
        weaponCollider.enabled = true;
        MonsterAnimationBroadcast("Attack");
        //_animator.SetTrigger("Attack");
        // Debug.Log($"{monsterInfo.monsterName}의 공격");
        Invoke("OnAttackEnd", attackAnimationLength);
    }

    private void OnAttackEnd()
    {
        weaponCollider.enabled = false;
        if (_state != MonsterState.Dead)
        ChangeState(MonsterState.AttackReady);
    }
}
