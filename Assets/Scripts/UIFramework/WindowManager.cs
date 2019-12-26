﻿using UnityEngine;
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

public interface IUpdateable
{
  void Update();
}
public abstract class Window 
{
    public GameObject gameObject { get; private set; }
    private Canvas mCanvas;
    public Canvas canvas
    {
        get
        {
            if (mCanvas == null && gameObject!= null)
            {
                mCanvas =gameObject.GetComponent<Canvas>();
            }
            return mCanvas;
        }
    }

    public const int LAYER = 5;
    public const int LAYER_MODEL = 6;
    public const int LAYER_HIDE = 7;
    public WindowStatus status = WindowStatus.None;
    public bool active
    {
        get { return gameObject && gameObject.layer == LAYER; }
        set
        {
            if (active != value)
            {
                SetLayer(value ? LAYER : LAYER_HIDE);
                if(value)
                {
                    OnShow();
                }
                else
                {
                    canvas.sortingOrder = 0;
                    OnHide();
                }
            }
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
            if (mParent != null)
            {
                if (mParent.widgets == null)
                {
                    mParent.widgets = new Dictionary<Type, Window>();
                }
                Type type = GetType();
                if (mParent.widgets.ContainsKey(type) == false)
                {
                    mParent.widgets.Add(type, this);
                }
            }
        }
    }
    public WindowType type;
    public bool hideOther { get; protected set; }
    public int fixedOrder { get; protected set; } = 0;

    public List<Type> fixedWidgets { get; protected set; }
    public Dictionary<Type, Window> widgets { get; protected set; }

    /// <summary>
    /// 打开本界面隐藏了哪些界面？
    /// </summary>
    public List<Window> hideWindows { get; set; }

    public int widgetOrderAddition = 5;

    /// <summary>
    /// 关闭是是否Destroy
    /// </summary>
    public bool closeDestroy = true;
    public void Close()
    {
        WindowManager.Instance.Close(this, closeDestroy);
    }

    public T GetComponent<T>(string path = null) where T : Component
    {
        if (gameObject == null)
        {
            return null;
        }
        if (string.IsNullOrEmpty(path))
        {
            gameObject.TryGetComponent(out T component);
            return component;
        }
        else
        {
            gameObject.transform.Find(path).TryGetComponent(out T component);
            return component;
        }
    }

    private void SetWidgetActive(bool active)
    {
        if (fixedWidgets != null && active)
        {
            for (int i = 0; i < fixedWidgets.Count; ++i)
            {
                Type type = fixedWidgets[i];
                if (widgets == null)
                {
                    widgets = new Dictionary<Type, Window>();
                }
                if (widgets.ContainsKey(type) == false)
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

            widget.parent = this;

            if (widget.status == WindowStatus.LoadDone)
            {
                WindowManager.Instance.SetActive(widget, active);
            }
        }
    }

    public void RemoveFromParent()
    {
        if (mParent != null && mParent.widgets != null)
        {
            Type type = GetType();
            mParent.widgets.Remove(type);

        }
    }
    private void SetLayer(int layer)
    {
        if (gameObject == null || gameObject.layer == layer)
        {
            return;
        }
        gameObject.layer = layer;

        var transforms = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < transforms.Length; ++i)
        {
            transforms[i].gameObject.layer = layer;
        }
    }

    public virtual void OnLoad(GameObject go)
    {
        gameObject = go;
        status = WindowStatus.LoadDone;
    }
    public virtual void OnUnload()
    {
        gameObject = null;
        status = WindowStatus.None;
    }

    protected virtual void OnShow()
    {

    }
    protected virtual void OnHide()
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
    private Stack<Window> mWindowStackTemp = new Stack<Window>();
    private List<Type> mCloseList = new List<Type>();

    
    private Camera mCamera;
    private EventSystem mEventSystem;

    private int mOrderAddition = 50;

    private Action<string, Action<UnityEngine.Object>> mLoader;

    void Awake()
    {
        GameObject camera = new GameObject("Camera");
        camera.transform.SetParent(transform);
        camera.layer = Window.LAYER;
        mCamera = camera.AddComponent<Camera>();
        mCamera.clearFlags = CameraClearFlags.Depth;
        mCamera.depth = 10;
        mCamera.orthographic = false; //--使用透视投影，这样UI模型的3D立体感更强
        mCamera.orthographicSize = 10;
        mCamera.fieldOfView = 60;
        mCamera.cullingMask = 1 << Window.LAYER | 1 << Window.LAYER_MODEL;

        GameObject eventsystem = new GameObject("EventSystem");
        eventsystem.transform.SetParent(transform);
        eventsystem.layer = Window.LAYER;
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
    /// <summary>
    /// 设置加载预设函数
    /// </summary>
    /// <param name="loader"></param>
    public void SetLoader(Action<string, Action<UnityEngine.Object>> loader)
    {
        mLoader = loader;
    }

    public void Open<T>(Type parentType = null,Action<T> callback = null) where T:Window
    {
        Type type = typeof(T);
        Open(type, parentType, (window) => { callback?.Invoke(window as T);});
    }

    public void Open(Type type, Type parentType, Action<Window> callback = null)
    {
        if(mLoader == null)
        {
            Debug.LogError("Loader is null.");
            callback?.Invoke(null);
            return;
        }

        if (type == null)
        {
            callback?.Invoke(null);
            return;
        }

        SetTouch(false);
        Window t = Get(type);
        if(t == null)
        {
            t = (Window)Activator.CreateInstance(type);

            if (t == null)
            {
                Debug.LogError("Can't create window:" + type.Name);
                callback?.Invoke(null);
                return;
            }
            mWindowDic.Add(type, t);
        }
       

        Window parent = Get(parentType);

        if (parentType!=null && (parent == null || parent.status == WindowStatus.None))
        {
            //先加载父界面
            Open(parentType, null, (p)=> {

                Open(type, parentType, callback);
            });
        }
        else
        {
            t.parent = parent;

            if (t.gameObject == null)
            {
                if (t.status == WindowStatus.Loading)
                {
                    return;
                }
                t.status = WindowStatus.Loading;
                mLoader(type.Name, (asset) =>
                {
                    if (t.status == WindowStatus.None)
                    {
                        return;
                    }

                    GameObject go = Instantiate(asset) as GameObject;
                    go.transform.SetParent(transform);
                    go.SetActive(true);

                    go.TryGetComponent(out Canvas canvas);
                    if (canvas == null) canvas = go.AddComponent<Canvas>();
                    
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = mCamera;
                    canvas.sortingLayerName = "UI";

                    go.TryGetComponent(out CanvasScaler scaler);
                    if (scaler == null) scaler = go.AddComponent<CanvasScaler>();

                    scaler.scaleFactor = 1;
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.referencePixelsPerUnit = 100;

                    
                    if (t.hideOther )
                    {
                        HideOther(t);
                    }
                    if (t.type == WindowType.Normal)
                    {
                        if (mWindowStack.Count <= 0 || mWindowStack.Peek().GetType() != type)
                        {
                            mWindowStack.Push(t);
                        }
                    }

                    t.OnLoad(go);

                    SetActive(t, true);
                    SetTouch(true);

                    callback?.Invoke(t);


                });
            }
            else
            {
                if (t.hideOther)
                {
                    HideOther(t);
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

                callback?.Invoke(t);
            }
        }
    }

    private void HideOther(Window window)
    {
        if (window == null)
        {
            return;
        }

        var it = mWindowDic.GetEnumerator();
        while (it.MoveNext())
        {
            var w = it.Current.Value;
            if (w != window)
            {
                if (w.active)
                {
                    if (window.hideWindows == null)
                    {
                        window.hideWindows = new List<Window>();
                    }
                    window.hideWindows.Add(w);
                    SetActive(w, false, false);
                }
            }
        }
    }

    public void SetActive(Window window, bool active, bool destory = false)
    {
        if (window == null)
        {
            return;
        }

        if (destory)
        {
            window.active = false;

            DestroyWindow(window);
        }
        else
        {
            if (active)
            {
                SetOrder(window);
            }

            window.active = active;
        }
    }
    private void SetOrder(Window window)
    {
        if (window == null || window.canvas == null) return;

        if (window.type == WindowType.Widget && window.fixedOrder != 0)
        {
            window.canvas.sortingOrder = window.fixedOrder;
        }
        else
        {
            if (window.parent != null && window.parent.canvas != null)
            {
                window.canvas.sortingOrder = window.parent.canvas.sortingOrder + window.widgetOrderAddition;
            }
            else
            {

                int maxOrder = int.MinValue;
                var it = mWindowDic.GetEnumerator();
                while (it.MoveNext())
                {
                    var v = it.Current.Value;
                    if (v.canvas != null && v.fixedOrder == 0 && v.parent == null)
                    {
                        if (v.canvas.sortingOrder > maxOrder)
                        {
                            maxOrder = v.canvas.sortingOrder;
                        }
                    }
                }
                if (maxOrder == int.MinValue)
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
        if(type == null)
        {
            return null;
        }

        mWindowDic.TryGetValue(type, out Window t);
        return t;
    }
   
    public void CloseAll(bool destroy = true)
    {
        mCloseList.Clear();
        mCloseList.AddRange(mWindowDic.Keys);
        for(int i = 0; i < mCloseList.Count; ++i)
        {
            SetActive(Get(mCloseList[i]), false, destroy);
        }
        mCloseList.Clear();
        mWindowStack.Clear();
    }
    public void CloseAllAndOpen<T>(Type parentType = null, Action<T> callback = null, bool destroy = true) where T: Window
    {
        CloseAllAndOpen(typeof(T), parentType, (t)=> { callback?.Invoke(t as T); }, destroy);
    }
   
    public void CloseAllAndOpen(Type type, Type parentType = null, Action<Window> callback = null,bool destroy = true) 
    {
        Window window = Get(type);
        mCloseList.Clear();
        var it = mWindowDic.GetEnumerator();
        while(it.MoveNext())
        {
            if(it.Current.Key!= type)
            {
                if(window == null 
                    || window.widgets == null
                    || window.widgets.ContainsKey(it.Current.Key) ==false)
                {
                    mCloseList.Add(it.Current.Key);
                }
               
            }
        }
        for(int i = 0; i< mCloseList.Count; ++i)
        {
            Type key = mCloseList[i];
            if (mWindowDic.TryGetValue(key, out Window w))
            {
                SetActive(w, false, destroy);
            }
        }
        mWindowStack.Clear();

        Open(type, parentType, callback);
    }

    public void Close<T>(bool destroy = true) where T:Window
    {
        Close(Get<T>(),destroy);
    }
    public void Close(Window window,bool destroy = true)
    {
        if(window == null)
        {
            return;
        }
        if (window.type == WindowType.Normal)
        {
            Window previous = null;
            bool contains = false;
            bool find = false;
            mWindowStackTemp.Clear();
            while (mWindowStack.Count > 0)
            {
                var v = mWindowStack.Pop();
                if (v == window)
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

            SetActive(window, false, contains == false && destroy);

            while (mWindowStackTemp.Count > 0)
            {
                var v = mWindowStackTemp.Pop();
                if (v == previous)
                {
                    if (mWindowStack.Count > 0 && previous.hideOther == false)
                    {
                        SetActive(mWindowStack.Peek(), true);
                    }
                }

                mWindowStack.Push(v);
            }
            if(window.hideWindows!= null)
            {
                for(int i =0; i < window.hideWindows.Count; ++i)
                {
                    if(window.hideWindows[i].type == WindowType.Widget)
                    {
                        SetActive(window.hideWindows[i], true);
                    }
                }
                window.hideWindows.Clear();
            }

            if (previous != null)
            {
                SetActive(previous, true);
            }
        }
        else
        {
            SetActive(window, false, destroy);
        }
    }

    private void DestroyWindow(Window window)
    {
        if(window== null)
        {
            return;
        }
        mWindowDic.Remove(window.GetType());
        Destroy(window.gameObject);
        window.OnUnload();
    }

    private void Update()
    {
        var it = mWindowDic.GetEnumerator();
        while(it.MoveNext())
        {
            if (it.Current.Value.active)
            {
                var update = it.Current.Value as IUpdateable;
                if (update != null)
                {
                    update.Update();
                }
            }
        }
    }
}
