using UnityEngine;
using Cinemachine;


public class CanvasWithWorldSpace : MonoBehaviour
{

    public Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }
    private void Update()
    {
        Vector3 direction = (transform.position - mainCamera.transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
