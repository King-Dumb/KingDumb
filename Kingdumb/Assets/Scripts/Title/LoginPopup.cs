using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class LoginPopup : MonoBehaviour
{
    public TMP_InputField inputedNickname;
    public GameObject messageErrorPopup;
    public TextMeshProUGUI validation;
    public Button loginButton;

    //public DebugManager debugManager;

    private void Awake()
    {
        gameObject.SetActive(false);
        messageErrorPopup.SetActive(false);
        loginButton.onClick.AddListener(OnClickLoginButton);
        validation.text = "";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnClickLoginButton();
        }
    }

    // 닉네임 유효성 검사 후 로비 팝업
    public void OnClickLoginButton()
    {
        // 클릭음 재생
        GlobalSoundManager.Instance.PlayImpactSound();

        ValidateAndFilterInput(GetNickname());

        if (!string.IsNullOrEmpty(GetNickname()))
        {
            // 닉네임 저장
            GameConfig.UserNickName = GetNickname();
            PhotonManager.SetNickname(GetNickname());
            //debugManager.SetInfoText("Nickname : " + GetNickname()); // Debug Nickname

            // 색상 저장
            string randomColor = $"#{Random.Range(0x100000, 0xFFFFFF):X6}";
            //Debug.Log(randomColor);
            while (randomColor.Equals("#00ff00")) // Info 색이랑 같으면 새로 생성
            {
                //Debug.Log("This is info color.");
                randomColor = $"#{Random.Range(0x100000, 0xFFFFFF):X6}";
            }
            GameConfig.UserColor = randomColor;

            // 로그인 팝업 비활성화
            OffPopup();

            // 로비 씬 이동
            PhotonNetwork.JoinLobby();
            // SceneManager.LoadScene("Lobby");
        }

    }

    // 입력의 유효성을 검사하고 필터링하는 메서드
    private void ValidateAndFilterInput(string input)
    {
        if (input.Length == 0)
        {
            messageErrorPopup.SetActive(true);
            SetValidation("닉네임을 입력하세요.");
            return;
        }

        string filteredInput = "";
        int length = 0;

        foreach (char c in input)
        {
            // 한글 또는 영문인지 체크
            if (GameConfig.IsKorean(c) || GameConfig.IsEnglish(c) || GameConfig.IsNumber(c))
            {
                length += 1;

                // 길이가 10자를 넘지 않으면 입력 유지
                if (length <= 10)
                {
                    filteredInput += c;
                }
                else
                {
                    messageErrorPopup.SetActive(true);
                    SetValidation("10글자 내로 입력하세요.");
                    SetNickname("");
                    break;
                }
            }
            else
            {
                messageErrorPopup.SetActive(true);
                SetValidation("한글, 영문, 숫자만 입력하세요.");
                SetNickname("");
            }
        }
    }

    public void OnPopup()
    {
        gameObject.SetActive(true);
    }

    public void OffPopup()
    {
        gameObject.SetActive(false);
    }

    public void SetNickname(string newName)
    {
        inputedNickname.text = newName;
    }

    public string GetNickname()
    {
        return inputedNickname.text;
    }

    public void SetValidation(string text)
    {
        validation.text = text;
    }
}
