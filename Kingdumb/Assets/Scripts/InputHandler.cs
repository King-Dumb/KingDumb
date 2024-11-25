// using System.Collections;
// using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class InputHandler : MonoBehaviourPun
{
    public float HAxis { get; private set; }
    public float VAxis { get; private set; }
    public bool CatchNexus { get; private set; }
    public bool RunKey { get; private set; }
    public bool AttackKey { get; private set; }
    public bool ChargingKey { get; private set; }
    public bool ChargingKeyUp { get; private set; }
    public bool SkillKeyDown { get; private set; }
    public bool UltimateKeyDown { get; private set; }
    public bool TowerShopKeyDown { get; private set; }
    public bool SkillTreeKeyDown { get; private set; }
    public bool Emotion1KeyDown { get; private set; }
    public bool Emotion2KeyDown { get; private set; }
    public bool Emotion3KeyDown { get; private set; }
    public bool Emotion4KeyDown { get; private set; }

    void Update()
    {
        // 자신의 객체만 움직이도록 설정
        if (photonView != null && !photonView.IsMine)
        {
            return;
        }
        GetInput();
    }

    public void GetInput()
    {
        HAxis = Input.GetAxisRaw("Horizontal");
        VAxis = Input.GetAxisRaw("Vertical");
        CatchNexus = Input.GetKeyDown(KeyCode.Space); // 넥서스 잡기: 스페이스

        RunKey = Input.GetKey(KeyCode.LeftShift); // 달리기: 좌쉬프트
        AttackKey = Input.GetMouseButton(0); // 공격: 마우스 좌
        ChargingKey = Input.GetMouseButton(1); // 차징: 마우스 우 꾸욱
        ChargingKeyUp = Input.GetMouseButtonUp(1); // 차징 공격: 마우스 우클릭 떼기
        SkillKeyDown = Input.GetKeyDown(KeyCode.E); // 스킬: e
        UltimateKeyDown = Input.GetKeyDown(KeyCode.Q); // 궁: q
        TowerShopKeyDown = Input.GetKeyDown(KeyCode.F); // 상호작용: f
        SkillTreeKeyDown = Input.GetKeyDown(KeyCode.V); // 스킬트리: v

        Emotion1KeyDown = Input.GetKeyDown(KeyCode.Alpha1); // 감정표현1: 1
        Emotion2KeyDown = Input.GetKeyDown(KeyCode.Alpha2); // 감정표현2: 2
        Emotion3KeyDown = Input.GetKeyDown(KeyCode.Alpha3); // 감정표현3: 3
        Emotion4KeyDown = Input.GetKeyDown(KeyCode.Alpha4); // 감정표현4: 4
    }
}