using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DrawGizmo : MonoBehaviour
{
    private Collider col;

    private void OnDrawGizmos()
    {
        col = GetComponent<Collider>();

        if (col == null) return;

        Gizmos.color = Color.magenta; // 반투명 빨간색으로 표시

        if (col is BoxCollider box)
        {
            Gizmos.matrix = Matrix4x4.TRS(box.transform.position, box.transform.rotation, box.transform.lossyScale);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawWireSphere(sphere.transform.position + sphere.center, sphere.radius * sphere.transform.lossyScale.x);
        }
        else if (col is CapsuleCollider capsule)
        {
            Gizmos.DrawWireSphere(capsule.transform.position + capsule.center, capsule.radius * capsule.transform.lossyScale.x);
        }
    }
}
