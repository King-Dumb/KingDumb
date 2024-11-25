using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraControl
{
    private static CameraControl _instance;
    public static CameraControl Inst
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CameraControl();
            }
            return _instance;
        }
    }

    private CinemachineVirtualCamera _virtualCamera;
    public CinemachineVirtualCamera virtualCamera
    {
        get
        {
            if (_virtualCamera == null)
            {
                _virtualCamera = GetActiveVirtualCamera();
            }

            if (_virtualCamera == null)
            {
                CinemachineVirtualCamera newCamera = Resources.Load<CinemachineVirtualCamera>("PlayerCamera");
                _virtualCamera = Object.Instantiate(newCamera);
                _virtualCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = 5f;
                _virtualCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = 5f;
            }


            return _virtualCamera;
        }
    }

    private CinemachinePOV _pov;
    public CinemachinePOV POV
    {
        get
        {
            if (_pov == null)
            {
                _pov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            }
            return _pov;
        }
    }

    public CinemachineVirtualCamera GetActiveVirtualCamera()
    {
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        return brain.ActiveVirtualCamera as CinemachineVirtualCamera;
    }

    public void DisableCameraMoving()
    {
        POV.m_HorizontalAxis.m_InputAxisName = "";
        POV.m_VerticalAxis.m_InputAxisName = "";
    }

    public void EnableCameraMoving()
    {
        POV.m_HorizontalAxis.m_InputAxisName = "Mouse X";
        POV.m_VerticalAxis.m_InputAxisName = "Mouse Y";
    }

    // 카메라를 고정된 위치에 고정시키기 (회전 방지)
    public void LockCamera()
    {
        //Debug.Log("카메라 막기");
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null)
        {
            brain.enabled = false;
            //Debug.Log("브레인 비활성화");
        }

        Camera.main.transform.rotation = Quaternion.Euler(Camera.main.transform.eulerAngles);

        // 입력을 비활성화하여 마우스나 키보드 입력이 카메라 회전에 영향을 미치지 않도록
        DisableCameraMoving();
    }

    // 카메라 회전 가능하도록 설정  
    public void UnlockCamera()
    {
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null)
        {
            brain.enabled = true;
            //Debug.Log("브레인 활성화");
        }

        EnableCameraMoving();
    }
}
