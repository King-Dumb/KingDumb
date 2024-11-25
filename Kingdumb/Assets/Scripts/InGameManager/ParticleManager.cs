using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//자주 사용되거나 공통으로 사용되는 파티클을 관리하는 매니저
public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Inst;
    private string particlePath = "Particle/";

    //파티클을 관리하는 오브젝트풀
    private Dictionary<string, Queue<GameObject>> particles = new Dictionary<string, Queue<GameObject>>();

    void Start()
    {
        //싱글톤 선언
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //반드시 생성할 때 Resources/Particle/ 폴더에 존재하는 파티클이어야함
    public GameObject Instantiate(string prefabId)
    {        
        return CreateParticle(prefabId, Vector3.zero, Quaternion.identity);
    }

    public GameObject Instantiate(string prefabId, Vector3 pos, Quaternion rot)
    {
        return CreateParticle(prefabId, pos, rot);
    }

    private GameObject CreateParticle(string prefabId, Vector3 pos, Quaternion rot)
    {
        //Debug.Log(prefabId + " 생성 시도");
        string prefabName = ParsePath(prefabId); //경로 제외한 파싱
        prefabName = ParseCloneName(prefabName); //(Clone) 네임 제거 파싱

        GameObject obj = null;

        if (particles.ContainsKey(prefabName) && particles[prefabName].Count > 0)
        {
            while(particles[prefabName].Count > 0)
            {
                obj = particles[prefabName].Dequeue();

                if(obj.activeSelf)
                {
                    continue;
                }
                else
                {
                    obj.transform.position = pos;
                    obj.transform.rotation = rot;
                    break;
                }
            }
        }

        if(obj == null)
        {
            obj = Instantiate(Resources.Load<GameObject>(prefabId), pos, rot);
            obj.name = prefabName;
        }

        return obj;
    }

    private string ParsePath(string str)
    {
        string result = null;
        int lastIdx = str.LastIndexOf('/');

        if (lastIdx == -1 || lastIdx == str.Length - 1)
            return str;

        result = str.Substring(lastIdx + 1);


        return result;
    }

    private string ParseCloneName(string str)
    {
        return str.Replace("(Clone)", "").Trim();
    }

    public void Destroy(GameObject obj)
    {
        DestroyParticle(obj);
    }

    public void Destroy(GameObject obj, float delay)
    {
        StartCoroutine(SetTime(obj, delay));
    }

    private void DestroyParticle(GameObject obj)
    {
        string name = ParseCloneName(obj.name);        
        
        if(!particles.ContainsKey(name))
        {
            particles[name] = new Queue<GameObject>();
        }

        obj.GetComponent<ParticleSystem>().Stop();
        particles[name].Enqueue(obj);
    }

    IEnumerator SetTime(GameObject obj , float delay)
    {
        yield return new WaitForSeconds(delay);
        DestroyParticle(obj);
    }
    //레벨업 이펙트
}
