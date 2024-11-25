using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;

// 기본형의 스켈레톤
public class RogueSkeleton : MonsterBase
{
    [SerializeField] private MonsterWeapon monsterLeftWeapon;
    [SerializeField] private MonsterWeapon monsterRightWeapon;

    private BoxCollider leftWeaponCollider;
    private BoxCollider rightWeaponCollider;

    protected override void Awake()
    {
        base.Awake();
        leftWeaponCollider = monsterLeftWeapon.GetComponent<BoxCollider>();
        rightWeaponCollider = monsterRightWeapon.GetComponent<BoxCollider>();
        
        monsterLeftWeapon.Initialize(monsterInfo.attackDamage/2, _viewId);
        monsterRightWeapon.Initialize(monsterInfo.attackDamage/2, _viewId);
    }

    public override void Attack()
    {
        ChangeState(MonsterState.Attacking);
        leftWeaponCollider.enabled = true;
        rightWeaponCollider.enabled = true;
        MonsterAnimationBroadcast("Attack");
        //_animator.SetTrigger("Attack");
        //Debug.Log($"{monsterInfo.monsterName}의 공격");
        Invoke("OnAttackEnd", attackAnimationLength);
    }

    private void OnAttackEnd()
    {
        leftWeaponCollider.enabled = false;
        rightWeaponCollider.enabled = false;
        ChangeState(MonsterState.AttackReady);
    }
}
