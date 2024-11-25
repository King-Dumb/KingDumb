using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 몬스터 본체가 아니지만 해당 위치에서 본체의 인터페이스 IMonster를 참조해야할때 본체로 연결하기 위해 씀
public class MonsterProxy : MonoBehaviour, IMonster
{
    public MonsterBase monsterBase; // 연결할 본체

    public Vector3 Position => monsterBase.Position;

    public void DebuffSlow(float slowRate, float duration)
    {
        monsterBase.DebuffSlow(slowRate, duration);
    }

    public void DestroySelf()
    {
        monsterBase.DestroySelf();
    }

    public void Die()
    {
        monsterBase.Die();
    }

    public bool IsDead()
    {
        return monsterBase.IsDead();
    }

    public void Knockback(Vector3 force)
    {
        monsterBase.Knockback(force);
    }

    public void OnDamage(float damage, bool isMagic, Vector3 hitPoint, int sourceViewID)
    {
        monsterBase.OnDamage(damage, isMagic, hitPoint, sourceViewID);
    }
}
