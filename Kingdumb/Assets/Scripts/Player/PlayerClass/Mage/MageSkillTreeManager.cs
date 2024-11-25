using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageSkillTreeManager : MonoBehaviourPun, ISkillTree
{
    private Mage mage;

    private bool[] skillNode = new bool[16];

    void Awake()
    {
        mage = GetComponent<Mage>();
        //Debug.Log(mage);
    }
    public object activateNode(int num)
    {
        object result = null;

        if (skillNode[num])
        {
            Debug.Log("중복해서 찍으려는 시도는 반환");
            return null;
        }

        switch (num)
        {
            case 1:
                // 평타 공격력 강화 5 (check)
                result = mage.AttackDamageUp(5f);
                break;
            case 2:
                // 평타 공격력 강화 5 (check)
                result = mage.AttackDamageUp(5f);
                break;
            case 3:
                // 평타 공격력 강화 10 (check)
                result = mage.AttackDamageUp(10f);
                break;
            case 4:
                // 평타 스플래시 데미지가 증가한다. (check)
                result = mage.AttackSplashDamageUp(5f);
                break;
            case 5:
                // 평타 투사체 속도가 빨라진다. (check)
                result = mage.AttackProjectileSpeedUp(3f);
                break;
            case 6:
                // 최대 체력이 감소하지만 적 처치 시 스킬 쿨타임이 0.2초 감소한다. (check)
                mage.ApplyKillCooldownReduction();
                break;
            case 7:
                // 평타가 적을 추적한다. (check)
                result = mage.AttackTrackingActivate();
                break;
            case 8:
                // 스킬 범위가 살짝 증가한다. (check)
                result = mage.IncreaseSkillRange();
                break;
            case 9:
                // 스킬 쿨타임이 감소된다. (check)
                mage.DecreaseSkillCooldown();
                break;
            case 10:
                // 빙결 속성을 갖는다. (암흑 속성 찍을 시 못찍음) (check)
                mage.ChangeSkillType(EffectType.Frost);
                break;
            case 11:
                // 스킬 선딜레이가 감소된다 (스킬 쿨타임이 아닌 스킬 시전 시간이 감소) (check)
                mage.DecreaseSkillCastDelay();
                break;
            case 12:
                // 궁극기 재사용 대기시간 감소
                break;
            case 13:
                // 부활 시간이 감소합니다. (check)
                mage.DecreaseReviveTime();
                break;
            case 14:
                // 궁극기의 데미지가 2배가 된다. (check)
                mage.UltimateDamageUp();
                break;
            case 15:
                // 최대 체력이 감소하지만 공격력, 범위 등이 증폭된다.(빙결 스텟 찍을 시 할당 불가) (check)
                mage.ChangeSkillType(EffectType.Darkness);
                break;
        }
        
        mage.SaveSkillNode(num);
        skillNode[num] = true;
        return result;
    }
}
