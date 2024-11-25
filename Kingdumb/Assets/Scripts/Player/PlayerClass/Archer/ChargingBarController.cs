using UnityEngine;
using UnityEngine.UI;

public class ChargingBarController : MonoBehaviour
{
    // Slider 컴포넌트와 차징 시작 시간을 저장할 변수
    public Slider chargingSlider;
    private float _maxChargingTime; // 최대 차징 시간

    void Start()
    {
        chargingSlider.gameObject.SetActive(false); // 초기에 안보이도록
    }

    // 최대 차징 시간 설정
    public void SetMaxChargeTime(float maxChargingTime)
    {
        _maxChargingTime = maxChargingTime;
    }

    // 차징 시작 메서드
    public void StartCharging()
    {
        chargingSlider.gameObject.SetActive(true); // 차징바 활성화
        chargingSlider.maxValue = _maxChargingTime; // 최대값 설정
    }

    // 차징 중지 메서드
    public void StopCharging()
    {
        chargingSlider.value = 0; // 값 초기화
        chargingSlider.gameObject.SetActive(false); // 차징바 비활성화
    }

    public void UpdateChargeBar(float chargeDuration)
    {
        // 차징바 값은 현재 차징 시간 그대로 반영
        chargingSlider.value = Mathf.Clamp(chargeDuration, 0, _maxChargingTime);
    }
}
