using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

// ESC 눌렀을 때 커서 활성화해주는 클래스
public class CursorController : MonoBehaviour
{

    private bool _isCursorLocked;
    public bool isCursorLocked
    {
        get { return _isCursorLocked; }
        set
        {
            _isCursorLocked = value;

            if (_isCursorLocked)
            {
                CursorLock();
            }
            else
            {
                CursorUnLock();
            }
        }
    }

    private void Start()
    {
        isCursorLocked = true;
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     isCursorLocked = !isCursorLocked;
        // }
    }

    public void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked; // 커서를 화면 중앙에 고정
        Cursor.visible = false; // 커서를 숨김
        CameraControl.Inst.UnlockCamera();
    }

    public void CursorUnLock()
    {
        Cursor.lockState = CursorLockMode.None;  // 커서 잠금 해제
        Cursor.visible = true;  // 커서 다시 보이게 설정
        CameraControl.Inst.LockCamera();
    }

}
