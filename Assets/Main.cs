using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

public class Main : MonoBehaviour
{
    static Main Instance;
    LuaEnv luaenv;
    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        luaenv = new LuaEnv();
        luaenv.DoString("require 'Main'");
    }



    // Update is called once per frame
    void Update()
    {
        luaenv.Tick();
     
    }
    private void OnDestroy()
    {
        
    }

    public static void LoadUI(string name,LuaFunction callback)
    {
        Instance.StartCoroutine(LoadUIAsync(name, callback));
    }

    private static IEnumerator LoadUIAsync(string name, LuaFunction callback)
    {
        ResourceRequest request = Resources.LoadAsync<Object>(string.Format("UI/{0}", name));
        yield return request;
        if(request.asset!= null)
        {
            callback.Action(request.asset);
        }
    }
}
