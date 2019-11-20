using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

public abstract class Window:MonoBehaviour
{
    public Canvas canvas;

    public WindowStatus status { get { return WindowManager.Instance.GetStatus(GetType()); } }
    public bool active
    {
        get { return gameObject.activeInHierarchy; }
        set
        {
            gameObject.SetActive(value);
            SetWidgetActive(value);
        }
    }
    private Window mParent;
    public Window parent
    {
        get { return mParent; }
        set
        {
            mParent = value;
            if(mParent!= null)
            {
                if(mParent.widgets == null)
                {
                    mParent.widgets = new Dictionary<Type, Window>();
                }
                Type type = GetType();
                if(mParent.widgets.ContainsKey(type) == false)
                {
                    mParent.widgets.Add(type, this);
                }
            }
        }
    }
    public WindowType type;
    public bool hidePrevious { get; protected set; }
    public int fixedOrder { get; protected set; } = 0;

    public List<Type> fixedWidgets { get; private set; }
    public Dictionary<Type, Window> widgets { get; protected set; }

    public int widgetOrderAddition = 5;

    public void Close()
    {

    }

    public void SetWidgetActive(bool active)
    {
       
        if(fixedWidgets!= null && active)
        {
            for(int i = 0; i < fixedWidgets.Count; ++i)
            {
                Type type = fixedWidgets[i];
                if(widgets.ContainsKey(type) == false)
                {
                    WindowManager.Instance.Open(type, GetType(), null);
                }
            }
        }
        if (widgets == null)
        {
            return;
        }
        var it = widgets.GetEnumerator();
        while (it.MoveNext())
        {
            var widget = it.Current.Value;
           
            if (widget.status == WindowStatus.LoadDone)
            {
                WindowManager.Instance.SetActive(widget, active);
            }
        }
    }

    public void RemoveFromParent()
    {
        if(mParent!= null && mParent.widgets!= null)
        {
            Type type = GetType();
            mParent.widgets.Remove(type);

        }
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
    private Stack<Window> mWindowStackTemp = new Stack<Window>();

    private Dictionary<Type, WindowStatus> mWindowStatus = new Dictionary<Type, WindowStatus>();

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

    public void Open<T>(Type parentType = null,Action<T> callback = null) where T:Window
    {
        Type type = typeof(T);
        Open(type, parentType, (window) => { callback?.Invoke(window as T);});
    }

    public void Open(Type type, Type parentType = null, Action<Window> callback = null)
    {
        if(type == null)
        {
            callback?.Invoke(null);
            return;
        }
        Window t = Get(type);
        if (t == null)
        {
            WindowStatus status = GetStatus(type);
            if (status == WindowStatus.Loading)
            {
                return;
            }
            SetStatus(type, WindowStatus.Loading);
            Main.LoadUI(type.Name, (asset) =>
            {
                status = GetStatus(type);
                if (status == WindowStatus.None)
                {
                    return;
                }
                SetStatus(type, WindowStatus.LoadDone);

                GameObject go = Instantiate(asset) as GameObject;
                go.transform.SetParent(transform);
                go.SetActive(true);

                t = go.GetComponent(type) as Window;
                if (t == null) t = go.AddComponent(type) as Window;

                t.canvas = go.GetComponent<Canvas>();
                t.canvas.renderMode = RenderMode.ScreenSpaceCamera;
                t.canvas.worldCamera = mCamera;
                t.canvas.sortingLayerName = "UI";

                var scaler = go.GetComponent<CanvasScaler>();
                if (scaler == null) scaler = go.AddComponent<CanvasScaler>();

                scaler.scaleFactor = 1;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.referencePixelsPerUnit = 100;

                SetLayer(go, mLayer);

                if (t.hidePrevious && mWindowStack.Count > 0)
                {
                    var v = mWindowStack.Peek();
                    SetActive(v, false);
                }
                if(t.type == WindowType.Normal )
                {
                    if (mWindowStack.Count <= 0 || mWindowStack.Peek().GetType() != type)
                    {
                        mWindowStack.Push(t);
                    }
                }
                
                SetActive(t, true);
                SetTouch(true);

                if(parentType!= null)
                {
                    Window parent = Get(parentType);
                    if(parent!= null)
                    {
                        t.parent = parent;
                    }
                }

                callback?.Invoke(t);

            });
        }
        else
        {
            if (t.hidePrevious && mWindowStack.Count > 0)
            {
                var v = mWindowStack.Peek();
                if (v != t)
                {
                    SetActive(v, false);
                }
            }
            if (t.type == WindowType.Normal)
            {
                if (mWindowStack.Count <= 0 || mWindowStack.Peek().GetType() != type)
                {
                    mWindowStack.Push(t);
                }
            }
            SetActive(t, true);
            SetTouch(true);

            if (parentType != null)
            {
                Window parent = Get(parentType);
                if (parent != null)
                {
                    t.parent = parent;
                }
            }

            callback?.Invoke(t);
        }
    }

   
    void SetLayer(GameObject go, int layer)
    {
        if(go == null || go.layer == layer)
        {
            return;
        }
        go.layer = layer;

        var transforms = go.GetComponentsInChildren<Transform>();
        for(int i = 0; i < transforms.Length; ++i)
        {
            transforms[i].gameObject.layer = layer;
        }
    }

    public void SetActive(Window window,bool active)
    {
        if(window == null)
        {
            return;
        }

        if(active)
        {
            SetOrder(window);
            if(window.active== false)
            {
                SetLayer(window.gameObject, mLayer);
                window.active = true;
            }
            else
            {
                window.SetWidgetActive(true);
            }
        }
        else
        {
            if(window.active)
            {
                SetLayer(window.gameObject, mLayerHide);
                window.active = false;
            }
            else
            {
                window.SetWidgetActive(false);
            }
        }
    }
    private void SetOrder(Window window)
    {
        if (window == null || window.canvas == null) return;


        if(window.type == WindowType.Widget && window.fixedOrder != 0)
        {
            window.canvas.sortingOrder = window.fixedOrder;
        }
        else
        {
            if(window.parent!= null)
            {
                window.canvas.sortingOrder = window.parent.canvas.sortingOrder + window.widgetOrderAddition;
            }
            else
            {

                int maxOrder = int.MinValue;
                var it = mWindowDic.GetEnumerator();
                while(it.MoveNext())
                {
                    var v = it.Current.Value;
                    if(v.canvas!= null && v.fixedOrder == 0 && v.parent == null)
                    {
                        if(v.canvas.sortingOrder > maxOrder)
                        {
                            maxOrder = v.canvas.sortingOrder;
                        }
                    }
                }
                if(maxOrder == int.MinValue)
                {
                    maxOrder = 0;
                }
                else
                {
                    maxOrder += mOrderAddition;
                }
                window.canvas.sortingOrder = maxOrder;
            }
        }
    }

    public T Get<T>() where T:Window
    {  
        return Get(typeof(T)) as T;
    }
    public Window Get(Type type)
    {
        mWindowDic.TryGetValue(type, out Window t);
        return t;
    }
    public WindowStatus GetStatus<T>()
    {
       return  GetStatus(typeof(T));
    }
    public WindowStatus GetStatus(Type type)
    {
        WindowStatus status = WindowStatus.None;
        mWindowStatus.TryGetValue(type, out status);
        return status;
    }
    private void SetStatus<T>( WindowStatus status)
    {
        SetStatus(typeof(T), status);
    }

    private void SetStatus(Type type, WindowStatus status)
    {
        if (mWindowStatus.ContainsKey(type) == false)
        {
            mWindowStatus.Add(type, status);
        }
        else
        {
            mWindowStatus[type] = status;
        }
    }

    public void CloseAll()
    {
        var it = mWindowDic.GetEnumerator();
        while(it.MoveNext())
        {
            Destroy(it.Current.Value.gameObject);
        }
        mWindowDic.Clear();
        mWindowStack.Clear();
    }

    public void Close<T>() where T:Window
    {
        Close(Get<T>());
    }
    public void Close(Window window)
    {
        if(window == null)
        {
            return;
        }
        if(window.type == WindowType.Normal)
        {
            Window previous = null;
            bool contains = false;
            bool find = false;
            mWindowStackTemp.Clear();
            while(mWindowStack.Count > 0)
            {
                var v = mWindowStack.Pop();
                if(v == window)
                {
                    if (find == false)
                    {
                        find = true;
                        if (mWindowStack.Count > 0)
                        {
                            previous = mWindowStack.Peek();
                        }
                    }
                    else
                    {
                        contains = true;
                        mWindowStackTemp.Push(v);
                    }
                }
                else
                {
                    mWindowStackTemp.Push(v);
                }
            }
            while(mWindowStackTemp.Count > 0)
            {
                var v = mWindowStackTemp.Pop();
                mWindowStack.Push(v);
                if (v == previous)
                {
                    if (window.hidePrevious == false && v.active == false)
                    {
                        SetActive(v, true);
                    }
                }
                else
                {
                    SetActive(v, true);
                }
            }

            if(contains)
            {
                SetActive(window, false);
            }
            else
            {
                Destroy(window.gameObject);
                mWindowDic.Remove(window.GetType());
            }

        }
        else
        {
            mWindowDic.Remove(window.GetType());
            Destroy(window.gameObject);
        }

    }
   
}
