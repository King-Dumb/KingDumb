using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//Resources/Photon에 들어있는 프리팹만 적용
public class PhotonObjectPool : IPunPrefabPool
{
    private const string path = "PhotonPool/"; //포톤 객체는 반드시 Resources/Photon 폴더에 넣기

    private Dictionary<string, Queue<GameObject>> _objPools = new Dictionary<string, Queue<GameObject>>();

    public GameObject Instantiate(string prefabId, Vector3 pos, Quaternion rot)
    {
        string prefabName = ParsePath(prefabId); //경로 제외한 파싱
        prefabName = ParseCloneName(prefabName); //(Clone) 네임 제거 파싱
        //Debug.Log("포톤 네트워크 오브젝트풀 생성 " + prefabName);

        GameObject obj = null;

        if (_objPools.ContainsKey(prefabName) && _objPools[prefabName].Count > 0)
        {
            //Debug.Log("큐에서 생성 " + prefabId);
            while (_objPools[prefabName].Count > 0)
            {
                obj = _objPools[prefabName].Dequeue();

                if (obj == null)
                    continue;

                obj.transform.position = pos;
                obj.transform.rotation = rot;
                obj.SetActive(false);
                break;
            }
        }
        if (obj == null)
        {
            //Debug.Log("Instantiate "+ prefabId);
            obj = GameObject.Instantiate(Resources.Load<GameObject>(path+prefabId), pos, rot);
            obj.name = prefabName;
            obj.SetActive(false);
        }

        return obj;
    }

    public void Destroy(GameObject obj)
    {
        //Debug.Log("오브젝트 풀 사용 완료" + obj.name);
        //오브젝트를 전부 사용하고 나면 비활성화 한 뒤 다시 풀에 집어넣는다.

        obj.SetActive(false);

        string objName = ParseCloneName(obj.name);

        if (!_objPools.ContainsKey(objName))
        {
            _objPools[objName] = new Queue<GameObject>();

        }
        _objPools[objName].Enqueue(obj);
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
}
