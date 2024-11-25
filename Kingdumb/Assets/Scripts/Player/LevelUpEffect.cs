using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpEffect : MonoBehaviour
{
    public TextMeshProUGUI LevelNumText;
    public float fadeDuration = 1.0f; // 각 이미지가 투명해지는 데 걸리는 시간

    public CanvasGroup canvasGroup;
    private float elapsedTime = 0f; // 현재 이미지의 페이드 경과 시간
    

    public void SetLevelNumText(int level)
    {
        LevelNumText.text = level.ToString();
        elapsedTime = 0;
        canvasGroup.alpha = 1;
    }

      void Update()
    {
        if (elapsedTime <= fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Alpha 값을 계산
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            canvasGroup.alpha = alpha;
        }
    }
}
