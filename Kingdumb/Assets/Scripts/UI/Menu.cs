using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Button exitBtn;
    public Button quitBtn;
    public Notification notification;

    private void Awake()
    {
        exitBtn.onClick.AddListener(() => OnClickBtn("Exit"));
        quitBtn.onClick.AddListener(() => OnClickBtn("Quit"));
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void ActiveMenu(bool isActive)
    {
        if (!isActive)
        {
            notification.OffPopup();
        }
        gameObject.SetActive(isActive);
    }

    public void OnClickBtn(string type)
    {
        switch (type)
        {
            case "Exit":
                notification.OnPopup();
                notification.SetContent("게임을 나가시겠습니까?");
                notification.SetOkType("Exit");
                break;
            case "Quit":
                notification.OnPopup();
                notification.SetContent("게임을 종료하시겠습니까?");
                notification.SetOkType("Quit");
                break;
        }
    }
}
