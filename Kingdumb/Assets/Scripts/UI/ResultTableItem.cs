using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultTableItem : MonoBehaviour
{
    [SerializeField] private Image _classTypeImage;
    [SerializeField] private TextMeshProUGUI _nickname;
    [SerializeField] private TextMeshProUGUI _dealtDamage;
    [SerializeField] private TextMeshProUGUI _heal;
    [SerializeField] private TextMeshProUGUI _takenDamage;

    public void Initialize(int classCode, string nickname, float dealtDamage, float healAmount, float takenDamage)
    {
        string spritePath = "";
        switch (classCode)
        {
            case 0: // 전사
                spritePath = "Sprites/Warrior_Head";
                break;
            case 1: // 궁수
                spritePath = "Sprites/Archer_Head";
                break;
            case 2: // 마법사
                spritePath = "Sprites/Mage_Head";
                break;
            case 3: // 사제
                spritePath = "Sprites/Priest_Head";
                break;
        }
        _classTypeImage.sprite = Resources.Load<Sprite>(spritePath);
        _nickname.text = nickname;
        _dealtDamage.text = dealtDamage > 0 ? dealtDamage.ToString("F0"):"-"; // 정수자리로 반올림
        _heal.text = healAmount > 0 ? healAmount.ToString("F0"):"-";
        _takenDamage.text = takenDamage > 0 ? takenDamage.ToString("F0"):"-";
    }
}
