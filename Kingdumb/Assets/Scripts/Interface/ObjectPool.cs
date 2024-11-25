using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : SingleTon<ObjectPool>
{
    //여기에 "Instantiate()로 사용할"오브젝트풀을 관리하는 프리팹
    //Photon.Instantiate()는 하지 않는다!    
    private Dictionary<string, ObjectPool<GameObject>> poolDictionary; //실제로 사용되는 오브젝트 풀
    private Dictionary<string, GameObject> allPrefabs; //Resource 폴더내의 모든 프리팹
    private const string path = "";
    int poolSize = 10;
    int poolMaxSize = 30;

    protected override void Awake()
    {
        base.Awake();
        InitObjPool();
        InitAllPrefabs(); 
    }
    
    private void InitObjPool()
    {
        if(poolDictionary == null)
        {
            poolDictionary = new Dictionary<string, ObjectPool<GameObject>> ();
        }
    }

    private void InitAllPrefabs()
    {
        if(allPrefabs == null)
        {
            allPrefabs = new Dictionary<string, GameObject> ();
        }

        allPrefabs.Clear();

        GameObject[] allprefabsArr = Resources.LoadAll<GameObject>(path);

        foreach (var prefab in allprefabsArr)
        {
            //Debug.Log(prefab.name +" "+ allPrefabs.Count);
            allPrefabs.Add(prefab.name, prefab);
        }
    }

    private void SetObjPool(GameObject newObj)
    {
        poolDictionary[newObj.name] = new ObjectPool<GameObject>(
        createFunc: () => Instantiate(newObj),
        actionOnGet: obj => obj.SetActive(true),
        actionOnRelease: obj => obj.SetActive(false),
        actionOnDestroy: obj => Destroy(obj),
        collectionCheck: false,
        defaultCapacity: poolSize,
        maxSize: poolMaxSize
        );
    }

    

    public GameObject CreateObj(string key)
    {
        //Debug.Log("오브젝트풀에서 생성");

        GameObject obj = null;
        if (poolDictionary.TryGetValue(key, out var pool))
        {
            obj = pool.Get();
        }
        else
        {
            //프리팹이 없다면 프리팹을 탐색한다.
            obj = FindPrefab(key);            
        }

        return obj;
    }

    public GameObject CreateObj(string key, Vector3 pos, Quaternion rot)
    {
        GameObject obj = null;
        if (poolDictionary.TryGetValue(key, out var pool))
        {
            obj = pool.Get();            
        }
        else
        {
            obj = FindPrefab(key);
        }

        if (obj != null)
        {
            obj.name = key; // 이름을 키와 동일하게 설정하여 (Clone) 제거
            obj.transform.position = pos;
            obj.transform.rotation = rot;
        }

        return obj;
    }

    //부하 많으므로 왠만하면 리스트에 추가바람
    public GameObject FindPrefab(string name)
    {
        if(allPrefabs.ContainsKey(name))
        {
            SetObjPool(allPrefabs[name]);
            poolDictionary.TryGetValue(name, out var pool);
            return pool.Get();
        }
        else
        {
            //Debug.Log("프리팹 로드 실패, 존재하지 않는 프리팹입니다." + name);
            return null;
        }
    }

    public void DestroyObj(string key, GameObject obj)
    {
        if (poolDictionary.TryGetValue(key, out var pool))
        {
            pool.Release(obj);
        }
        else
        {
            //Debug.LogError($"No pool found for key: {key}");
            Destroy(obj); // 풀을 못 찾았을 때는 오브젝트를 제거
        }
    }

    public void ClearObjPool()
    {        
        foreach (var pool in poolDictionary)
        {
            var objectPool = pool.Value;
            objectPool.Clear();
            //while(objectPool.CountInactive > 0)
            //{
            //    GameObject obj = objectPool.Get();

            //    if(obj != null)
            //    {
            //        //obj.GetComponent<MonoBehaviour>().StopAllCoroutines();
            //        GameManager.Instance.Destroy(obj);
            //    }
            //}
        }

        poolDictionary.Clear();
    }
}
