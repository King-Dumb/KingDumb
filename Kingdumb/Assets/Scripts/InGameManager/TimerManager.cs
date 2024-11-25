using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviourPun
{
    public static TimerManager Inst;

    public bool isIncreasingTimer = false;
    public bool isDecreasingTimer = false;
    public float increasingElapsedTime;
    public float decreasingElapsedTime;
    public float increaseMin;
    public float increaseSec;
    public float decreaseMin;
    public float decreaseSec;
    public float decreaseStartTime = 30f; // 감소 타이머 시작 시간 (초 단위)

    public event Action OnIncreaseTimerSecondPassed; // 증가 타이머 1초마다 호출
    public event Action<bool> OnDecreaseTimerSecondPassed; // 감소 타이머 1초마다 호출
    void Awake()
    {
        //싱글톤 선언
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ResetIncreasingTimer();
        ResetDecreasingTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        UpdateIncreasingTimer();
        UpdateDecreasingTimer();
    }

    public void ResetIncreasingTimer()
    {
        increasingElapsedTime = 0f;
        increaseMin = GameManager.Instance.totalIncreaseMin;
        increaseSec = GameManager.Instance.totalIncreaseSec;
        photonView.RPC("UpdateIncreasingTimerRpc", RpcTarget.All, increaseMin, increaseSec);
    }

    public void ResetDecreasingTimer()
    {
        decreasingElapsedTime = 0f;
        decreaseMin = Mathf.Floor(decreaseStartTime / 60);
        decreaseSec = decreaseStartTime % 60;
    }

    void UpdateIncreasingTimer()
    {
        if (isIncreasingTimer)
        {
            increasingElapsedTime += Time.deltaTime;

            if (increasingElapsedTime >= 1f)
            {
                increasingElapsedTime = 0f;
                increaseSec += 1f;

                if (increaseSec >= 60f)
                {
                    increaseSec = 0f;
                    increaseMin += 1f;
                }
                photonView.RPC("UpdateIncreasingTimerRpc", RpcTarget.All, increaseMin, increaseSec); 
                //OnIncreaseTimerSecondPassed?.Invoke(); // 증가 타이머 1초마다 콜백 호출
            }
        }
    }

    [PunRPC]
    public void UpdateIncreasingTimerRpc(float _increaseMin, float _increaseSec)
    {
        //Debug.Log("UpdateIncreasingTimerRpc");
        increaseMin = _increaseMin;
        increaseSec = _increaseSec;
        OnIncreaseTimerSecondPassed?.Invoke(); // 증가 타이머 1초마다 콜백 호출
    }

    void UpdateDecreasingTimer()
    {
        if (isDecreasingTimer)
        {
            decreasingElapsedTime += Time.deltaTime;

            if (decreasingElapsedTime >= 1f)
            {
                decreasingElapsedTime = 0f;

                if (decreaseSec == 0)
                {
                    if (decreaseMin > 0)
                    {
                        decreaseMin -= 1f;
                        decreaseSec = 59f;
                    }
                    else
                    {
                        isDecreasingTimer = false; // 감소 타이머 종료
                        photonView.RPC("UpdateDecreasingTimerRpc", RpcTarget.All, decreaseMin, decreaseSec, true);
                        //OnDecreaseTimerSecondPassed?.Invoke(true); // 마지막 콜백 호출
                        return;
                    }
                }
                else
                {
                    decreaseSec -= 1f;
                }
                photonView.RPC("UpdateDecreasingTimerRpc", RpcTarget.All, decreaseMin, decreaseSec, false);
                //OnDecreaseTimerSecondPassed?.Invoke(false); // 감소 타이머 1초마다 콜백 호출
            }
        }
    }

    [PunRPC]
    public void UpdateDecreasingTimerRpc(float _decreaseMin, float _decreaseSec, bool isZero)
    {
        decreaseMin = _decreaseMin;
        decreaseSec = _decreaseSec;
        //Debug.Log($"UpdateDecreasingTimerRpc: {decreaseMin}Min, {decreaseSec}Sec");
        OnDecreaseTimerSecondPassed?.Invoke(isZero); // 감소 타이머 1초마다 콜백 호출
    }

}
