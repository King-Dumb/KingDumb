using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Timer : MonoBehaviour
{

    public void ActiveIncreseTimer(bool enable)
    {
        TimerManager.Inst.isIncreasingTimer = enable;   
        //isIncreasingTimer = enable;
    }

    public void ActiveDecreasingTimer(bool enable)
    {
        TimerManager.Inst.isDecreasingTimer = enable;
        //isDecreasingTimer = enable;
    }

    public string GetIncreasingTimerText()
    {
        return TimerManager.Inst.increaseMin.ToString("00") + " : " + TimerManager.Inst.increaseSec.ToString("00");
        //return increaseMin.ToString("00") + " : " + increaseSec.ToString("00");
    }

    public string GetDecreasingTimerText()
    {
        return TimerManager.Inst.decreaseMin.ToString("00") + " : " + TimerManager.Inst.decreaseSec.ToString("00");
        //return decreaseMin.ToString("00") + " : " + decreaseSec.ToString("00");
    }

    public void SetDecreseStartTime(float startTime)
    {
        TimerManager.Inst.decreaseStartTime = startTime;
        Debug.Log("SetDecreaseStartTime:" + TimerManager.Inst.decreaseStartTime);
        //decreaseStartTime = startTime;
    }
}
