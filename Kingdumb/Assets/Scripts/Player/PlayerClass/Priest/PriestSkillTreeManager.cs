using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PriestSkillTreeManager : MonoBehaviourPun, ISkillTree
{
    private Priest priest;

    private bool[] skillNode = new bool[16];

    void Awake()
    {
        priest = GetComponent<Priest>();
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
                // 평타 공격력 3 증가 -> O
                result = priest.AttackDamageUp(3f);
                break;
            case 2:
                // 평타 공격력 6 증가 -> O
                result = priest.AttackDamageUp(6f);
                break;
            case 3:
                // 평타 공격력 10 증가 -> O
                result = priest.AttackDamageUp(10f);
                break;
            case 4:
                // 부활 시간 5초 감소 -> O
                result = priest.ReviveTimeDown(2f);
                break;
            case 5:
                // 최대 체력 50 증가 -> O
                result = priest.MaxHpUp(50f);
                break;
            case 6:
                // 기본 이동 속도 3 증가 -> O
                result = priest.SpeedUp(1f); // 수정
                break;
            case 7:
                // 자가 회복 초당 2 -> O
                StartCoroutine(priest.RecoverHp(2f));
                break;
            case 8:
                // 스킬 지속 시간 1초 증가 -> O
                priest.SkillTimeUp(1f);
                break;
            case 9:
                // 스킬 지속동안 이동속도 5f 증가 -> 스킬 초당 힐/딜량 3증가 -> O
                priest.SkillPriceUp(3f);
                break;
            case 10:
                // 처치 시 공격력 0.2 증가 -> O
                priest.ApplyAttackDamageUp(0.2f);
                break;
            case 11:
                // 넥서스 힐 가능 -> O
                priest.ApplyCanHealNexus();
                break;
            case 12:
                // 소환수 이동속도 10 증가 -> O
                priest.GolemSpeedUp(10f);
                break;
            case 13:
                // 스킬 쿨타임 2초 감소 -> O
                priest.SkillDurationDown(2f);
                break;
            case 14:
                // 소환수 지속시간 5초 증가 -> O
                priest.GolemDurationUp(5f);
                break;
            case 15:
                // 스킬 사용시 거리에 상관없이 모든 플레이어에게 적용 -> O
                priest.SkillAllPlayer();
                break;
        }

        priest.SaveSkillNode(num);
        skillNode[num] = true;
        return result;
    }
}
