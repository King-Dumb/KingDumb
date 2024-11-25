using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorSkillTreeManager : MonoBehaviourPun, ISkillTree
{
    private Warrior warrior;

    private bool[] skillNode = new bool[16];

    private void Awake()
    {
        warrior = GetComponent<Warrior>();
        //Debug.Log(warrior);
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
                result = warrior.AttackDamageUp(5f);
                break;
            case 2:
                // 평타 공격력 강화 10 (check)
                result = warrior.AttackDamageUp(10f);
                break;
            case 3:
                // 평타 공격력 강화 15 (check)
                result = warrior.AttackDamageUp(15f);
                break;
            case 4:
                // 방어력 10 증가 (check)
                result = warrior.DefencePowerUp(10f);
                break;
            case 5:
                // 체력 20% 증가 (check)
                result = warrior.IncreaseMaxHP(0.2f);
                break;
            case 6:
                // 달리기 시 이동속도 증가 (check)
                warrior.IncreaseRunningSpeed(2f);
                break;
            case 7:
                // 적 처치 시 영구적으로 방어력 0.2 증가 (check)
                result = warrior.IncreaseDefencePowerOnMonsterKill(0.2f);
                break;
            case 8:
                // 부메랑 공격의 크기 증가 (check)
                result = warrior.IncreaseThrowingSwordScale(3f);
                break;
            case 9:
                // 부메랑 공격의 사거리 증가 (check)
                result = warrior.IncreaseThrowingSwordDistance(10f);
                break;
            case 10:
                // 평타공격 속도가 50% 느려지지만 평타 공격력이 50% 증가 (check)
                warrior.SlowStrongDefaultAttack(0.5f, 0.5f);
                break;
            case 11:
                // 스킬 공격의 쿨타임 감소 (check)
                warrior.DecreaseSkillCoolTime(1f);
                break;
            case 12:
                // 궁극기에 공중 몬스터 즉사 (check)
                warrior.EnableFlyingMonsterInstantDeath(true);
                break;
            case 13:
                // 궁극기 게이지 요구량 감소 (check)
                warrior.DecreaseUltimateDuration(2f);
                break;
            case 14:
                // 궁극기가 4갈래로 나감 -> 궁극기 최대 피해 몬스터 5마리에서 10마리로 변경 (check)
                // 문제 파악 RPC의 파라미터를 설정하지 않았음 
                warrior.IncreaseMaxAttackableMonster(5);
                break;
            case 15:
                // 적 처치 시 스킬 공격의 쿨타임 1초 감소 (check)
                warrior.DecreaseSkillCoolTimeOnMonsterKill(1f);
                break;
        }

        warrior.SaveSkillNode(num);
        skillNode[num] = true;
        return result;
    }
}
