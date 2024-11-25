using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestSkill : MonoBehaviour
{
    private int ownerPvId;
    private float duration; // 총 힐을 주는 시간
    private float pricePerSecond; // 초당 힐량
    private float startTime; // 영역 시작 시간
    private bool isAllPlayer = false; // 모든 플레이어에게 적용할지 여부
    private bool isSpeedUp = false; // 플레이어 이동 속도 증가할지 여부
    private float moveSpeedUP; // 플레이어 스킬 사용 시 증가 된 이동속도

    private bool canHealNexus = false;

    public GameObject healEffect; // 회복 이펙트
    private GameObject healEffectObject; // 생성한 회복 이펙트 오브젝트
    public GameObject damageEffect; // 피해 이펙트
    private GameObject damageEffectObject; // 생성한 피해 이펙트 오브젝트
    private Nexus nexus;

    // 각 오브젝트와 관련된 코루틴 참조를 저장할 Dictionary
    private Dictionary<Collider, Coroutine> damageCoroutines = new Dictionary<Collider, Coroutine>();
    private Dictionary<Collider, Coroutine> healCoroutines = new Dictionary<Collider, Coroutine>();

    void Start()
    {

    }

    void OnEnable()
    {
        startTime = Time.time; // 영역이 활성화된 시간 기록
        //GameManager.Instance.Destroy(gameObject, duration); // 설정 시간 뒤 영역 사라짐
        if (isAllPlayer)
        {
            SkillAllPlayer();
        }
    }

    void OnDisable()
    {
        // 이펙트삭제

    }

    private void OnTriggerEnter(Collider other)
    {
        //NEXUS HEAL
        // canHealNexus = true;
        // Debug.Log($"트리거 enter {canHealNexus}");
        float remainingTime = duration - (Time.time - startTime); // 남은 실행 시간 계산

        if (other.CompareTag("Monster")) // 몬스터 태그가 있는 오브젝트만 대상
        {
            // 피해 코루틴 시작하고 Dictionary에 추가
            Coroutine damageCoroutine = StartCoroutine(ApplyDamageOverTime(other));
            damageCoroutines[other] = damageCoroutine;

            // 체력 감소 이펙트 적용
            damageEffectObject = GameManager.Instance.Instantiate(damageEffect.name, other.transform.position, damageEffect.transform.rotation);
            //damageEffectObject.GetComponent<PriestSkillSplash>().SetDuration(remainingTime); // 실행 시간 뒤 이펙트 제거
            damageEffectObject.GetComponent<PriestSkillSplash>().SetObject(other.gameObject);
            GameManager.Instance.Destroy(damageEffectObject, remainingTime);
        }
        else if (other.CompareTag("Player"))
        {
            // 회복 코루틴 시작하고 Dictionary에 추가
            Coroutine healCoroutine = StartCoroutine(ApplyHealOverTime(other));
            healCoroutines[other] = healCoroutine;

            // 체력 회복 이펙트 적용
            healEffectObject = GameManager.Instance.Instantiate(healEffect.name, other.transform.position, healEffect.transform.rotation);
            //healEffectObject.GetComponent<PriestSkillSplash>().SetDuration(remainingTime); // 실행 시간 뒤 이펙트 제거
            healEffectObject.GetComponent<PriestSkillSplash>().SetObject(other.gameObject);
            GameManager.Instance.Destroy(healEffectObject, remainingTime);

            if (isSpeedUp)
            {
                // 플레이어 이동 속도 증가 후 원래대로 돌아오기
                StartCoroutine(MoveSpeedUpForSecond(remainingTime - 0.1f, other));
            }
        }
        else if (canHealNexus && other.CompareTag("Nexus"))
        {
            // NEXUS HEAL
            // Debug.Log("넥서스 힐 시도");
            // 잡고 있지 않은 경우에만 처리
            if (other.TryGetComponent<Nexus>(out nexus))
            {
                Coroutine healCoroutine = StartCoroutine(ApplyHealOverTimeNexus(nexus));
                healCoroutines[other] = healCoroutine;

                healEffectObject = GameManager.Instance.Instantiate(healEffect.name, other.transform.position, healEffect.transform.rotation);
                healEffectObject.GetComponent<PriestSkillSplash>().SetObject(other.gameObject);
                GameManager.Instance.Destroy(healEffectObject, remainingTime);
            }
        }
    }

    private IEnumerator MoveSpeedUpForSecond(float remainingTime, Collider other)
    {
        // Debug.Log($"버프 시작 : 지속시간 = {remainingTime}");
        // 다른 플레이어의 캐릭터 인포
        CharacterInfo otherCharacterInfo = other.GetComponent<CharacterInfo>();

        // 기준 이동속도 참고
        float otherBaseMoveSpeed = otherCharacterInfo.GetBaseMoveSpeed();
        // 새로운 이동속도 만들기
        float newMoveSpeed = otherBaseMoveSpeed + moveSpeedUP;

        ApplyMoveSpeed(other, newMoveSpeed);

        yield return new WaitForSeconds(remainingTime);

        // 이동속도 돌려놓기
        // Debug.Log("버프 지속시간 종료");
        otherBaseMoveSpeed = otherCharacterInfo.GetBaseMoveSpeed();
        ApplyMoveSpeed(other, otherBaseMoveSpeed);
    }

    void ApplyMoveSpeed(Collider other, float newMoveSpeed)
    {

        CharacterInfo otherCharacterInfo = other.GetComponent<CharacterInfo>();

        PlayerController otherPlayerController = other.GetComponent<PlayerController>();

        // 뛰고 있다면 이동속도 적용 x
        if (otherPlayerController.isRun)
        {

        }
        // 왕자를 들고 있거나 (궁수)차징 중이면 새로운 baseMoveSpeed의 반으로 설정
        else if (otherPlayerController.isNexusCaptured || otherPlayerController.isCharging)
        {
            otherCharacterInfo.SetMoveSpeed(newMoveSpeed / 2);
        }
        else
        {
            otherCharacterInfo.SetMoveSpeed(newMoveSpeed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            // 해당 몬스터의 피해 코루틴이 존재할 경우 중지
            if (damageCoroutines.TryGetValue(other, out Coroutine damageCoroutine))
            {
                StopCoroutine(damageCoroutine);
                damageCoroutines.Remove(other); // Dictionary에서 제거

                // 이펙트 제거
                GameManager.Instance.Destroy(damageEffectObject);
            }
        }
        else if (other.CompareTag("Player"))
        {
            // 해당 플레이어의 회복 코루틴이 존재할 경우 중지
            if (healCoroutines.TryGetValue(other, out Coroutine healCoroutine))
            {
                StopCoroutine(healCoroutine);
                healCoroutines.Remove(other); // Dictionary에서 제거

                // 이펙트 제거
                GameManager.Instance.Destroy(healEffectObject);
            }

            if (isSpeedUp)
            {
                // 플레이어 이동 속도 원래대로
                // Debug.Log("밖으로 나감 종료");
                float otherBaseMoveSpeed = other.GetComponent<CharacterInfo>().GetBaseMoveSpeed();
                ApplyMoveSpeed(other, otherBaseMoveSpeed);
            }
        }
        else if (other.CompareTag("Nexus"))
        {
            // 해당 플레이어의 회복 코루틴이 존재할 경우 중지
            if (healCoroutines.TryGetValue(other, out Coroutine healCoroutine))
            {
                StopCoroutine(healCoroutine);
                healCoroutines.Remove(other); // Dictionary에서 제거
                GameManager.Instance.Destroy(healEffectObject);
            }
        }
    }

    private IEnumerator ApplyHealOverTime(Collider player)
    {
        // 체력 회복 적용
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 매 초마다 회복 적용
            player.GetComponent<CharacterInfo>().RestoreHealth(pricePerSecond, player.transform.position, ownerPvId);
            elapsed += 1f;
            yield return new WaitForSeconds(1f); // 1초 기다린 후 다시 실행
        }
    }

    private IEnumerator ApplyHealOverTimeNexus(Nexus nexus)
    {
        // 체력 회복 적용
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 매 초마다 회복 적용
            nexus.RestoreHealth(pricePerSecond * 0.2f, nexus.transform.position, ownerPvId);
            Debug.Log("Nexus Heal");
            elapsed += 1f;
            yield return new WaitForSeconds(1f); // 1초 기다린 후 다시 실행
        }
    }

    private IEnumerator ApplyDamageOverTime(Collider monster)
    {
        // 체력 감소 적용
        IDamageable target = monster.GetComponent<IDamageable>();
        if (target != null)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                // Collider가 파괴된 경우 코루틴 종료
                if (monster == null)
                {
                    yield break;
                }

                // 매 초마다 피해 적용
                target.OnDamage(pricePerSecond, true, monster.transform.position, ownerPvId);
                elapsed += 1f;
                yield return new WaitForSeconds(1f); // 1초 기다린 후 다시 실행
            }
        }
    }

    public void SetOwnerPvId(int id)
    {
        ownerPvId = id;
    }

    public void SetDuration(float dur)
    {
        duration = dur;
    }

    public void SetPricePerSecond(float price)
    {
        pricePerSecond = price;
    }

    public void SetMoveSpeedUp(float amount)
    {
        moveSpeedUP = amount;
    }

    public void SetIsSpeedUp(bool isSpeed)
    {
        isSpeedUp = isSpeed;
    }

    public void SkillAllPlayer()
    {
        // "Player" 태그를 가진 모든 오브젝트 가져오기
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // 가져온 오브젝트들을 순회하며 처리
        foreach (GameObject player in players)
        {
            Collider collider = player.GetComponent<Collider>();
            StartCoroutine(ApplyHealOverTime(collider));
        }
    }

    public void SetAllPlayer(bool isAll)
    {
        isAllPlayer = isAll;
    }

    public void SetCanHealNexus(bool canHeal)
    {
        canHealNexus = canHeal;
    }
}