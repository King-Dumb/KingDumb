using UnityEngine;

public interface IArrow
{
    void SetDamage(float damage);
    void SetDirection(Vector3 dir);
    void SetOwnerPhotonViewID(int id);
}