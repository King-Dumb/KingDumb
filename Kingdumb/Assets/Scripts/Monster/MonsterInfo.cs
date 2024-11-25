using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterInfo", menuName = "ScriptableObjects/MonsterInfo", order = 1)]
public class MonsterInfo : ScriptableObject
{
    public string monsterName; // 몬스터 이름
    public float hp; // 초기 체력
    public float attackDamage; // 기본 공격력
    public int defencePower; // 방어력
    public float moveSpeed = 3f; // 이동속도
    public float rotationSpeed = 480f; // 회전 속도
    public float attackDuration = 3f; // 공격 속도
    public float attackRange;
    public float outOfAttackRange;

    [Tooltip("다음 값 이내의 거리에서 플레이어에게 어그로가 끌릴 수 있음")]
    public float detectionRange = 10f;
    [Tooltip("다음 값 이상의 거리에서 플레이어에게 어그로가 풀림")]
    public float outOfDetectionRange = 11f;

    [Tooltip("몬스터의 체력이 다음 값 이하일 때 플레이어에게 어그로가 끌릴 수 있음")]
    public float aggroTriggerHp;
    public float aggroDuration = 10f;

    public int expReward = 1;
    public int goldReward = 25;
    public int gaugeReward = 1;

}
