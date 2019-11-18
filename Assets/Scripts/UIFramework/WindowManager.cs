using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public enum WindowType  
{
    Normal  = 0,    //0、普通界面,会加入到导航栈
    Widget = 1,     //1、小组件，比如飘字或者子界面
}

public enum WindowStatus{
    None            = 0, 
    Loading         = 1,     //正在加载中
    LoadDone        = 2,     //加载完成
}

public abstract class Window 
{
    public GameObject gameObject;

    public Canvas canvas;
    public WindowStatus status;
    public bool active;
    public Window parent;
    public WindowType type;
    public bool hidePrevious { get; protected set; }
    public int fixedOrder { get; protected set; } = 0;

    public List<Type> childs { get; private set; }
    public Dictionary<Type, Window> widgets { get; protected set; } = new Dictionary<Type, Window>();

    public int widgetOrderAddition = 5;

    public void Close()
    {

    }
}
public class WindowManager : MonoBehaviour
{
    private static WindowManager mInstance;
    public static WindowManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                GameObject go = new GameObject(typeof(WindowManager).Name);
                mInstance = go.AddComponent<WindowManager>();
                DontDestroyOnLoad(go);
            }
            return mInstance;
        }
    }
    private Dictionary<Type, Window> mWindowDic = new Dictionary<Type, Window>();
    private Stack<Window> mWindowStack = new Stack<Window>();

    private Camera mCamera;
    private EventSystem mEventSystem;

    private int mOrderAddition = 50;
    private int mLayer = 5;             // --显示层LayerMask.NameToLayer("UI")
    private int mLayerModel = 6;        // --UI模型层
    private int mLayerHide = 7;         // --隐藏层
    void Awake()
    {
        GameObject camera = new GameObject("Camera");
        camera.transform.SetParent(transform);
        camera.layer = mLayer;
        mCamera = camera.AddComponent<Camera>();
        mCamera.clearFlags = UnityEngine.CameraClearFlags.Depth;
        mCamera.depth = 10;
        mCamera.orthographic = false; //--使用透视投影，这样UI模型的3D立体感更强
        mCamera.orthographicSize = 10;
        mCamera.fieldOfView = 60;
        mCamera.cullingMask = 2 ^ mLayer + 2 ^ mLayerModel;

        GameObject eventsystem = new GameObject("EventSystem");
        eventsystem.transform.SetParent(transform);
        eventsystem.layer = mLayer;
        mEventSystem = eventsystem.AddComponent<EventSystem>();
        mEventSystem.sendNavigationEvents = true;
        mEventSystem.pixelDragThreshold = 5;

        eventsystem.AddComponent<StandaloneInputModule>();
    }
    public void SetTouch(bool touchable)
    {
        if (mEventSystem)
            mEventSystem.enabled = touchable;
    } 

    public void Open<T>(Window parent,Action<T> callback) where T:Window
    {
        T t = Get<T>();
        if(t == null)
        {

        }
        else
        {

        }
    }

    public T Get<T>() where T:Window
    {
        mWindowDic.TryGetValue(typeof(T), out Window t);
        return t as T;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
