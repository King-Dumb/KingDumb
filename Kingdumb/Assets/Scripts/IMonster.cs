using UnityEngine;

public interface IMonster: IDamageable
{
    void Die(); // 죽는 모션
    void DestroySelf(); // 죽는 모션 없이 그냥 사라짐

    void DebuffSlow(float slowRate, float duration);

    void Knockback(Vector3 force);
}
