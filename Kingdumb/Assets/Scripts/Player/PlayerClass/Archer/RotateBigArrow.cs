using UnityEngine;

public class RotateBigArrow : MonoBehaviour
{
    public float rotationSpeed = 150f;

    void Update()
    {
        transform.rotation *= Quaternion.Euler(0, 0, rotationSpeed * Time.deltaTime);
    }
}
