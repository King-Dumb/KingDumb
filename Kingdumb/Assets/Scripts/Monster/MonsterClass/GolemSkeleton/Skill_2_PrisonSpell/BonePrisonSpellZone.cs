using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonePrisonSpellZone : MonsterDamageField
{
    public ParticleSystem bonePrisonEffect;

    private Transform _target; // 따라갈 타겟
    private float _initialSpeed; // 초기 속도
    private float _finalSpeed; // 초기 속도
    private float _zoneDuration; // 존 지속 시간
    private float _elapsedTime; // 경과 시간
    private CapsuleCollider _collider; 

    void Awake()
    {
        _collider = GetComponent<CapsuleCollider>();
        _collider.enabled = false;
    }

    void Update()
    {
        if (_target == null)
            return;

        // 지속 시간이 끝난 경우 이동 종료
        if (_elapsedTime >= _zoneDuration)
        {
            return;
        }

        _elapsedTime += Time.deltaTime;

        // 속도가 점차 감소
        float currentSpeed = Mathf.Lerp(_initialSpeed, _finalSpeed, _elapsedTime / _zoneDuration);

        Vector3 direction = (_target.position - transform.position).normalized;

        direction.y = 0; 

        transform.position += direction * currentSpeed * Time.deltaTime;
    }

    public void Initialize(Transform target, float initialSpeed, float finalSpeed, float zoneDuration, float spellDamage, bool isMagic, float spellDuration, float spellDamageInterval)
    {
        base.Initialize(spellDamageInterval, spellDamage, isMagic, zoneDuration+spellDuration);
        _target = target;
        _initialSpeed = initialSpeed;
        _finalSpeed = finalSpeed;
        _zoneDuration = zoneDuration;
        _elapsedTime = 0f;
        Invoke("EmitPrisonSpell", zoneDuration);
    }

    public void EmitPrisonSpell()
    {
        bonePrisonEffect.Play();
        _collider.enabled = true; 
        // TODO : 특수효과 처리 (이동 불가 등) 
    }

    public override void DestroySelf()
    {
        // base.Initialize에서 실행됨 삭제하기전에 처리해야할 로직들
        bonePrisonEffect.Stop();
        _collider.enabled = false;
        base.DestroySelf();
    }
}
