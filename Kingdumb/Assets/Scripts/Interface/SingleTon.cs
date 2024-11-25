using UnityEngine;

public class SingleTon<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T Inst;

    public static T Instance
    {
        get
        {
            if (Inst == null)
            {
                Inst = FindObjectOfType<T>();

                // if (Inst == null)
                // {
                //     Debug.LogError(typeof(T) + " is not found in the scene.");
                // }
            }
            return Inst;
        }
    }

    protected virtual void Awake()
    {
        if (Inst == null)
        {
            Inst = this as T;
            DontDestroyOnLoad(gameObject);  // 씬 변경에도 오브젝트 유지
        }
        else if (Inst != this)
        {
            Destroy(gameObject);  // 이미 인스턴스가 존재하면 중복 생성 방지
        }
    }
}
