using Cinemachine;
using UnityEngine;

public class MoveTemp : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    void Update()
    {

        float HorizontalValue = CameraControl.Inst.POV.m_HorizontalAxis.Value;

        Vector3 playerRotation = transform.eulerAngles;

        playerRotation.y = HorizontalValue;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(playerRotation), Time.deltaTime * 30f);


        // 입력을 받아서 이동 방향 계산
        float moveHorizontal = Input.GetAxis("Horizontal"); // A, D 키 입력
        float moveVertical = Input.GetAxis("Vertical");     // W, S 키 입력

        // X, Z 축으로 이동 (플레이어의 로컬 좌표계에서)
        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical);

        // TransformDirection을 사용하여 현재 바라보는 방향으로 변환
        Vector3 moveDirection = transform.TransformDirection(movement);

        // 이동 처리 (프레임에 따라 속도를 조절)
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

    }


}
