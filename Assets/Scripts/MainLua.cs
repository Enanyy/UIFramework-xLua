using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using XLua;

public class MainLua : MonoBehaviour
{
    static MainLua Instance;
    LuaEnv luaenv;
    private void Awake()
    {
        Instance = this;
    }
    Dictionary<string, string> luaFiles = new Dictionary<string, string>();

    LuaFunction update;
    // Start is called before the first frame update
    void Start()
    {
        string[] files = Directory.GetFiles(Application.dataPath, "*.lua.txt",SearchOption.AllDirectories);
        for(int i = 0; i < files.Length; ++i)
        {
            string file = Path.GetFileNameWithoutExtension(files[i]);
            string name = file.Substring(0,file.IndexOf('.'));
            luaFiles.Add(name, files[i]);
        }

        luaenv = new LuaEnv();
        luaenv.AddLoader(LuaFileLoader);
        luaenv.DoString("require 'Main'");

        update = luaenv.Global.Get<LuaFunction>("update");
    }

    byte[] LuaFileLoader(ref string name)
    {
        if(luaFiles.TryGetValue(name, out string file))
        {
            byte[] bytes = File.ReadAllBytes(file);

            return bytes;
        }
        else
        {
            Debug.LogError("Can't find lua file:" + name);
        }
        return null;
    }


    // Update is called once per frame
    void Update()
    {
        luaenv.Tick();

        if(update!= null)
        {
            update.Action(Time.deltaTime);
        }
     
    }
    private void OnDestroy()
    {
        
    }

    public static void LoadUI(string name,LuaFunction callback)
    {
        Instance.StartCoroutine(LoadUIAsync(name, (asset)=> 
        {
            if(callback!=null)
            {
                callback.Action(asset);
            }
        }));
    }
    public static void LoadUI(string name, System.Action<Object> callback)
    {
        Instance.StartCoroutine(LoadUIAsync(name, callback));
    }
    private static IEnumerator LoadUIAsync(string name, System.Action<Object> callback)
    {
        ResourceRequest request = Resources.LoadAsync<Object>(string.Format("UI/{0}", name));
        yield return request;
        if(request.asset!= null)
        {
            callback(request.asset);
        }
    }
}
