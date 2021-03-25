using UnityEngine;
using System.Collections;

public class MainCS : MonoBehaviour
{
    public static MainCS Instance;

    private void Awake()
    {
        Instance = this;
    }
    // Use this for initialization
    void Start()
    {
        var asset = Resources.Load<TextAsset>("UIDefine");


        WindowManager.Instance.SetLoader(LoadUI);
        WindowManager.Instance.Load(asset.text);

        WindowManager.Instance.Open("UIMain");
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void LoadUI(string name, System.Action<Object> callback)
    {
        Instance.StartCoroutine(LoadUIAsync(name, callback));
    }
    private static IEnumerator LoadUIAsync(string name, System.Action<Object> callback)
    {
        ResourceRequest request = Resources.LoadAsync<Object>(string.Format("UI/{0}", name));
        yield return request;
        if (request.asset != null)
        {
            callback(request.asset);
        }
    }
}
