using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherSkillTreeManager : MonoBehaviourPun, ISkillTree
{
    private Archer archer;

    private bool[] skillNode = new bool[16];

    void Awake()
    {
        archer = GetComponent<Archer>();
        //Debug.Log(archer);
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
                result = archer.AttackDamageUp(5f);
                break;
            case 2:
                // 평타 공격력 강화 5 (check)
                result = archer.AttackDamageUp(5f);
                break;
            case 3:
                // 평타 공격력 강화 10 (check)
                result = archer.AttackDamageUp(10f);
                break;
            case 4:
                // 기본 이동속도 증가 (check)
                result = archer.MoveSpeedUp(1f);
                break;
            case 5:
                // 공격 속도 증가 (check)
                result = archer.DecreaseAttackDuration(0.2f);
                break;
            case 6:
                // 차지 소모시간 감소 (check)
                result = archer.DecreaseChargingDuration(0.5f);
                break;
            case 7:
                // 평타 시 화살이 2발씩 나감 (check)
                archer.IncreaseAttackCount(1);
                break;
            case 8:
                // 트리플 샷 상태 시 공격력 증가 (check)
                result = archer.SkillDamageUp(0.5f);
                break;
            case 9:
                // 트리플 샷 상태 시 방어구 관통 (check)
                archer.MakeSkillMagic(true);
                break;
            case 10:
                // 차지샷 시 공격력이 50% 감소하지만 적을 관통 (check)
                archer.ChargeUpgrade(false);
                break;
            case 11:
                // 트리플 샷이 4발로 나감 (check)
                archer.IncreaseSkillAttackCount(1);
                break;
            case 12:
                // 궁극기 요구 게이지 감소 (check)
                result = archer.DecreaseUltimateDuration(5f);
                break;
            case 13:
                // 일반공격 시 일정확률로 넉백 (check)
                archer.AddKnockBack(true);
                break;
            case 14:
                // 궁극기로 처치한 몬스터 하나 당 게이지 반환 (check)
                archer.ApplyKillCooldownReduction();
                break;
            case 15:
                // 일반공격 시 화살이 주변의 적에게 1번 튕김 (check)
                archer.AttackTrackingActivate();
                break;
        }

        archer.SaveSkillNode(num);
        skillNode[num] = true;
        return result;
    }
}
