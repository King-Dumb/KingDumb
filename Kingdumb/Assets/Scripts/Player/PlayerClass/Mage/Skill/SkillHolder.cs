using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillHolder : EffectHolder
{
    [SerializeField] public GameObject energyBlastPrefab;
    [SerializeField] public GameObject darknessBlastPrefab;
    [SerializeField] public GameObject frostBlastPrefab;

    private float _damage = 30; // 에너지 볼의 데미지
    private float _skillCastDelay = 1.0f;
    private Vector3 _scale = new Vector3(5f, 5f, 5f); // 이펙트 스케일
    private Vector3 _blastScale = new Vector3(2f, 2f, 6f); // 블래스트 스플래시 스케일
    public override void InitializeEffects()
    {
        effectPrefabs = new Dictionary<EffectType, GameObject>
        {
            { EffectType.Energy, energyBlastPrefab },
            { EffectType.Darkness, darknessBlastPrefab },
            { EffectType.Frost, frostBlastPrefab }
        };
    }

    public override void Attack(EffectType effectType, Vector3 position, Vector3 direction, int ownerPhotonViewID)
    {
        switch (effectType)
        {
            case EffectType.Energy:
                StartCoroutine(EnergyBlast(position, direction, ownerPhotonViewID));
                break;
            case EffectType.Darkness:
                StartCoroutine(DarknessBlast(position, direction, ownerPhotonViewID));
                break;
            case EffectType.Frost:
                StartCoroutine(FrostBlast(position, direction, ownerPhotonViewID));
                break;
        }
    }

    public IEnumerator EnergyBlast(Vector3 position, Vector3 direction, int ownerPhotonID)
    {
        GameObject magicCircle = GameManager.Instance.Instantiate(effectPrefabs[EffectType.Energy].name, position + direction * 4, Quaternion.Euler(-90, 0, 0));
        magicCircle.transform.localScale = _scale;
        magicCircle.GetComponent<MagicBlast>().SetOwnerPhotonViewID(ownerPhotonID);
        yield return new WaitForSeconds(_skillCastDelay);
        magicCircle.GetComponent<MagicBlast>().Blast(EffectType.Energy, _damage, _blastScale);
        GameManager.Instance.Destroy(magicCircle);
    }

    public IEnumerator DarknessBlast(Vector3 position, Vector3 direction, int ownerPhotonID)
    {
        GameObject magicCircle = GameManager.Instance.Instantiate(effectPrefabs[EffectType.Darkness].name, position + direction * 4, Quaternion.Euler(-90, 0, 0));
        //Debug.Log(magicCircle.name);
        magicCircle.transform.localScale = _scale;
        magicCircle.GetComponent<MagicBlast>().SetOwnerPhotonViewID(ownerPhotonID);
        yield return new WaitForSeconds(_skillCastDelay);
        magicCircle.GetComponent<MagicBlast>().Blast(EffectType.Darkness, _damage, _blastScale);
        GameManager.Instance.Destroy(magicCircle);
    }

    private IEnumerator FrostBlast(Vector3 position, Vector3 direction, int ownerPhotonID)
    {
        GameObject magicCircle = GameManager.Instance.Instantiate(effectPrefabs[EffectType.Frost].name, position + direction * 4, Quaternion.Euler(-90, 0, 0));
        magicCircle.transform.localScale = _scale;
        magicCircle.GetComponent<MagicBlast>().SetOwnerPhotonViewID(ownerPhotonID);
        yield return new WaitForSeconds(_skillCastDelay);
        magicCircle.GetComponent<MagicBlast>().Blast(EffectType.Frost, _damage, _blastScale);
        GameManager.Instance.Destroy(magicCircle);
    }

    public Vector3 IncreaseSkillRange()
    {
         _blastScale = new Vector3(4f, 4f, 10f); // 블래스트 스플래시 스케일
        return _blastScale;
    }

    public float DecreaseSkillCastDelay()
    {
        _skillCastDelay = 0.5f;
        return _skillCastDelay;
    }

    public float SkillDamageUp(float damage)
    {
        _damage += damage;
        return _damage;
    }
}
