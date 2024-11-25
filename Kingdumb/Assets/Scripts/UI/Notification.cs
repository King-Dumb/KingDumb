using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
    public TextMeshProUGUI content;

    // Button Group
    public GameObject buttons;
    public Button okBtn;
    private string okType;
    public Button cancelBtn;
    private TextMeshProUGUI okBtnText;
    private TextMeshProUGUI cancelBtnText;


    // single button
    public GameObject button;
    public Button btn;
    private TextMeshProUGUI btnText;

    public CursorController cursor;

    private void Awake()
    {
        OffPopup();

        // 버튼 이벤트 추가
        okBtn.onClick.AddListener(OnClickOkBtn);
        cancelBtn.onClick.AddListener(OnClickCancelBtn);
        btn.onClick.AddListener(OnClickBtn);

        // 버튼 텍스트 초기화
        okBtnText = okBtn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        cancelBtnText = cancelBtn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        btnText = btn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
    }

    public void SetButton(int num)
    {
        if (num == 1)
        {
            button.SetActive(true);
            buttons.SetActive(false);
        }
        else
        {
            button.SetActive(false);
            buttons.SetActive(true);
        }
    }

    public void SetContent(string newContent)
    {
        content.text = newContent;
    }

    public void OnPopup()
    {
        if (cursor != null && Cursor.lockState == CursorLockMode.Locked)
        {
            cursor.CursorUnLock();
        }

        gameObject.SetActive(true);
    }

    public void OffPopup()
    {
        if (cursor != null && Cursor.lockState == CursorLockMode.None)
        {
            cursor.CursorLock();
        }

        gameObject.SetActive(false);
    }

    public void SetOkType(string type)
    {
        okType = type;
    }

    private void OnClickOkBtn()
    {
        switch (okType)
        {
            case "Exit": // 게임 나가기
                PhotonManager.Inst.ExitGame();
                break;
            case "Quit": // 게임 종료
                PhotonManager.Inst.QuitGame();
                break;
        }
    }

    private void OnClickCancelBtn()
    {
        gameObject.SetActive(false);
    }

    private void OnClickBtn()
    {
        if (cursor != null && Cursor.lockState == CursorLockMode.None)
        {
            cursor.CursorLock();
        }

        gameObject.SetActive(false);
    }

    public void SetButtonText(string type, string text)
    {
        switch (type)
        {
            case "ok":
                okBtnText.text = text;
                break;
            case "cancel":
                cancelBtnText.text = text;
                break;
            case "btn":
                btnText.text = text;
                break;
        }
    }
}
