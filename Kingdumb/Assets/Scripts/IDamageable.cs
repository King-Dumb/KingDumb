using UnityEngine;

public interface IDamageable
{
    Vector3 Position { get; }   // 대상의 현재 위치
    void OnDamage(float damage, bool isMagic, Vector3 hitPoint, int sourceViewID);
    bool IsDead();
}
