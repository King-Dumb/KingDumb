using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterUI : MonoBehaviour
{

    [SerializeField] private Transform _canvas;

    [SerializeField] private Slider _hpBar;

    [SerializeField] private GameObject _damageTextPrefab;

    [SerializeField] private ParticleSystem slowEffect;

    private void Update()
    {   
        lookAtMainCamera();
    }

    private void lookAtMainCamera()
    {
        _canvas.LookAt(Camera.main.transform);
        _canvas.Rotate(0, 180, 0);
    }

    public void UpdateHpBar(float value)
    {
        if (value > 0 && value < 1) {
            _hpBar.gameObject.SetActive(true);
            _hpBar.value = value;
        }
        else {
            _hpBar.gameObject.SetActive(false);
        }
    }

    public void DamagePopup(float damageAmount)
    {

        GameObject damageTextInstance = GameManager.Instance.Instantiate(_damageTextPrefab.name);
        damageTextInstance.name = _damageTextPrefab.name;
        damageTextInstance.transform.SetParent(_canvas, false);
        RectTransform rectTransform = damageTextInstance.GetComponent<RectTransform>();

        //damageTextInstance.transform.position = position;

        // 모델에 텍스트가 가려져서 임시로 위치 살짝 옮김 
        //Vector3 newPosition = damageTextInstance.transform.position;
        //newPosition.y += 1;
        //damageTextInstance.transform.position = newPosition;
        damageTextInstance.GetComponent<TextMeshProUGUI>().SetText(damageAmount.ToString());
        damageTextInstance.transform.localScale = new Vector3(2f, 2f, 2f);
    }

    public void UpdateSlowEffect(int debuffCount)
    {
        //Debug.Log(debuffCount);
        if (debuffCount > 0)
        {
            //Debug.Log("슬로우 이펙트 재생");
            slowEffect.Play();
        }
        else
        {
            //Debug.Log("슬로우 이펙트 종료");
            slowEffect.Stop();
        }
    }
}