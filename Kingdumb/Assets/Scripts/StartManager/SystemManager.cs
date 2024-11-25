using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//시스템 관련 매니저
public class SystemManager : SingleTon<SystemManager>
{
    private const string jsonPath = "JSON/";

    //// Start is called before the first frame update
    //void Start()
    //{
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
     

    //Json 파일 파싱
    public T LoadJson<T>(string name) where T : class
    {
        TextAsset json = Resources.Load<TextAsset>(jsonPath + name);       

        if(json == null)
        {
            return null;
        }

        return JsonUtility.FromJson<T>(json.text);
    }
}
