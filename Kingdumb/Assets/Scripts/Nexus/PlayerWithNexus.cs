using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWithNexus : MonoBehaviourPun
{
    public GameObject capturedNexus; // 잡힌 넥서스가 비활성화된 채로 시작해서 FindWithTag로 참조가 안됨
    public GameObject nexusInfoUI; // TODO: InGameUIManager 참조하는 형식으로 가져와야 함
    public GameObject playerWeapon;
    private GameObject nexus;
    private Nexus nexusScript;

    private Animator animator;
    private InputHandler inputHandler; // playerController 만으로도 인풋 통제 가능하다면 생략
    private PlayerController playerController;
    private CharacterInfo characterInfo; // 상속관계도 참조 가능

    public GameObject NexusMinimapObject;

    private bool _isNexusCaptured = false;
    public bool isNexusCaptured
    {
        get { return _isNexusCaptured; }
        set
        {
            _isNexusCaptured = value;

            // inputHandler.isNexusCaptured = _isNexusCaptured;
            playerController.isNexusCaptured = _isNexusCaptured;

            NexusMinimapObject.SetActive(_isNexusCaptured);

            if (!_isNexusCaptured)
            {
                nexusCatchRemainTime = nexusCatchCoolTime;
            }
        }
    }

    private float nexusCatchCoolTime = 3f;
    private float nexusCatchRemainTime;

    private float nexusCapturedDuration;
    private int nexusControlTimeDamageMultiplier; // 들고있는 시간 대비 배율을 적용한 데미지를 적용한다.

    private bool isNexusCatchable = true;
    private float catchableDistance = 9f; // 제곱값

    void Start()
    {
        //nexus = GameObject.FindWithTag("Nexus"); // 이 시점에 활성화 된 Nexus는 잡히지 않은 Nexus 하나여서 상관없음
        //nexusScript = nexus.GetComponent<Nexus>();

        //animator = GetComponent<Animator>();
        //inputHandler = GetComponent<InputHandler>();
        //characterInfo = GetComponent<CharacterInfo>();

        //nexusInfoUI = IngameUIManager.Inst.nexusInfo;
    }

    void OnEnable()
    {
        animator = GetComponent<Animator>();
        inputHandler = GetComponent<InputHandler>();
        characterInfo = GetComponent<CharacterInfo>();
        playerController = GetComponent<PlayerController>();

        InitializeNexus();
    }

    public void InitializeNexus()
    {
        //Debug.Log("Player With Nexus 컴포넌트 활성화");
        nexus = GameObject.FindWithTag("Nexus"); // 이 시점에 활성화 된 Nexus는 잡히지 않은 Nexus 하나여서 상관없음
        //Debug.Log("넥서스가 정상적으로 불러와졌는지 확인:" + nexus.name);
        nexusScript = nexus.GetComponent<Nexus>();
        //Debug.Log(nexusScript.name);

        nexusInfoUI = IngameUIManager.Inst.nexusInfo;

        nexusCatchRemainTime = nexusCatchCoolTime;
        nexusCapturedDuration = 0f;
        nexusControlTimeDamageMultiplier = 1;
    }

    void Update()
    {
        if (photonView != null && !photonView.IsMine)
        {
            return;
        }

        if (nexus == null)
        {
            //Debug.LogError("nexus 변수가 초기화되지 않았습니다.");
            InitializeNexus();
            return;
        }
        //if (!PhotonNetwork.IsMasterClient) return;
        // 넥서스 죽으면 return하는 코드 추가하려다가, 넥서스 죽으면 게임 끝이라 추가하지 않음

        CheckDistance();

        if (inputHandler.CatchNexus)
        {
            if (isNexusCatchable && nexusCatchRemainTime < 0)
            {
                GlobalSoundManager.Instance.PlayPrinceCarrySound();
                photonView.RPC("CatchNexus", RpcTarget.All);
                //CatchNexus();
                return;
            }

            if (isNexusCaptured)
            {
                GlobalSoundManager.Instance.StopEffectSourceClip();
                photonView.RPC("ReleaseNexus", RpcTarget.All);
                //ReleaseNexus();
                return;
            }
        }

        if (isNexusCaptured)
        {
            // 넥서스 주운 후 패널티 적용 구간
            nexusCapturedDuration += Time.deltaTime;

            // 3초에 한번씩 넥서스를 주운 플레이어(=자기 자신)의 OnDamage 호출
            if (nexusCapturedDuration > 3f)
            {
                nexusCapturedDuration = 0f;
                photonView.RPC("OnDamageBroadcast", RpcTarget.All, 5f * nexusControlTimeDamageMultiplier, true, transform.position, nexusScript.nexusPhotonViewID);
                nexusControlTimeDamageMultiplier++; // 배율 증가
            }
        }
        else
        {
            // 넥서스 떨군 후 쿨타임 감소 구간
            nexusCatchRemainTime -= Time.deltaTime;
            nexusControlTimeDamageMultiplier = 1; // 배율 초기화
        }

        // Debug.Log($"CapturedDuration : {nexusCapturedDuration}, nexusCatchRemainTime : {nexusCatchRemainTime}");
    }

    public void CheckDistance()
    {
        if (nexus.activeSelf)
        {
            float playerNexusDistance = (transform.position - nexus.transform.position).sqrMagnitude;

            if (playerNexusDistance <= catchableDistance && nexusCatchRemainTime < 0)
            {
                isNexusCatchable = true;
                nexusInfoUI.SetActive(true);
                return;
            }
        }

        isNexusCatchable = false;
        nexusInfoUI.SetActive(false);
    }

    [PunRPC]
    public void CatchNexus()
    {
        isNexusCaptured = true;
        // 플레이어 속도 느리게 하는 코드
        playerController.IsSlow(true);

        nexusCapturedDuration = 0f;

        capturedNexus.SetActive(true);
        nexus.SetActive(false);

        playerWeapon.SetActive(false);

        animator.SetBool("carryNexus", true);
    }

    [PunRPC]
    public void ReleaseNexus()
    {
        isNexusCaptured = false;
        // 플레이어 속도 원상복구하는 코드 - 넥서스 주우면 스킬 업 불가능 >> 그 사이 이속 바뀌지 않음
        playerController.IsSlow(false);

        nexus.SetActive(true);
        capturedNexus.SetActive(false);

        playerWeapon.SetActive(true);

        // 플레이어가 바라보고 있는 방향 + 0.5f 만큼 떨어진 곳에 넥서스 떨구기
        Vector3 releasePosition = transform.position + transform.forward * 0.5f;

        nexus.transform.position = releasePosition;

        animator.SetBool("carryNexus", false);
    }

    [PunRPC]
    public void OnDamageBroadcast(float damage, bool isMagic, Vector3 hitPoint, int sourceViewID)
    {
        characterInfo.OnDamage(damage, isMagic, hitPoint, sourceViewID);
    }
}
