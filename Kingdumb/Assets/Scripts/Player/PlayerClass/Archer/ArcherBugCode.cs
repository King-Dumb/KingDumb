using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherBug : CharacterInfo, IPlayerClass
{
    public Arrow arrow;
    public Transform arrowPos;

    public ArcherBug()
    {
        _classType = "Archer";
        _maxHp = 100;
        _hp = 100;
        _attackDamage = 12;
        _defencePower = 0;
        _attackDuration = 0.6f;
        _skillDuration = 3f;
        _moveSpeed = 5f;
        _reviveTime = 12f;
    }

    public void Attack()
    {
        Debug.Log("궁수 공격");
        Instantiate(arrow, arrowPos);
    }

    public void Charging()
    {
        Debug.Log("궁수 차징");
    }

    public void Skill()
    {
        Debug.Log("궁수 스킬");
    }

    public void Ultimate()
    {
        Debug.Log("궁수 궁");
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
