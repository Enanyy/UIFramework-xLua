using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Collections;

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

    private class WindowNav
    {
        public WindowContext window;
        public List<WindowContext> hideWindows;
    }

    private Dictionary<string, WindowContext> mWindowContextDic = new Dictionary<string, WindowContext>();
    private Dictionary<string, GameObject> mWindowObjectDic = new Dictionary<string, GameObject>();

    private List<WindowNav> mWindowStack = new List<WindowNav>();
    private List<string> mCloseList = new List<string>();

    
    private Camera mCamera;
    private EventSystem mEventSystem;

    private int mOrderAddition = 50;

    private Action<string, Action<UnityEngine.Object>> mLoader;

    void Awake()
    {
        GameObject camera = new GameObject("Camera");
        camera.transform.SetParent(transform);
        camera.layer = WindowContext.LAYER;
        mCamera = camera.AddComponent<Camera>();
        mCamera.clearFlags = CameraClearFlags.Depth;
        mCamera.depth = 10;
        mCamera.orthographic = false; //--使用透视投影，这样UI模型的3D立体感更强
        mCamera.orthographicSize = 10;
        mCamera.fieldOfView = 60;
        mCamera.cullingMask = 1 << WindowContext.LAYER ;

        GameObject eventsystem = new GameObject("EventSystem");
        eventsystem.transform.SetParent(transform);
        eventsystem.layer = WindowContext.LAYER;
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

  

    public void Open(WindowContext context, WindowContext parent = null, Action<GameObject> callback = null)
    {
        if(mLoader == null)
        {
            Debug.LogError("Loader is null.");
            callback?.Invoke(null);
            return;
        }

        if (context == null)
        {
            callback?.Invoke(null);
            return;
        }

        SetTouch(false);
       
        if(mWindowContextDic.ContainsKey(context.name)==false)
        {
            mWindowContextDic.Add(context.name, context);
        }
       
        if (parent!=null && parent.status == WindowStatus.None)
        {
            //先加载父界面
            Open(parent, null, (p)=> {

                Open(context, parent, callback);
            });
        }
        else
        {
            context.parent = parent;

            GameObject go = null;
            mWindowObjectDic.TryGetValue(context.name, out go);
            if(go == null)
            {
                if (context.status == WindowStatus.Loading)
                {
                    return;
                }
                context.status = WindowStatus.Loading;
                mLoader(context.name, (asset) =>
                {
                    if ( asset == null || context.status == WindowStatus.None)
                    {
                        mWindowContextDic.Remove(context.name);
                        context.Clear();
                        return;
                    }

                    
                    context.status = WindowStatus.Done;

                    go = Instantiate(asset) as GameObject;
                    mWindowObjectDic.Add(context.name, go);
                    if(go.GetComponent(context.component)== null)
                    {
                        go.AddComponent(context.component);
                    }
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
            
                    Push(context);
                   
                    SetActive(context, true);
                    SetTouch(true);

                    callback?.Invoke(go);


                });
            }
            else
            {
                context.status = WindowStatus.Done;

                Push(context);

                SetActive(context, true);
                SetTouch(true);

                callback?.Invoke(go);
            }
        }
    }

    private void Push(WindowContext window)
    {
        if(window== null)
        {
            return;
        }

        if (window.type == WindowType.Normal)
        { 
            WindowNav nav = new WindowNav();
            nav.window = window;

            if (window.hideOther)
            {
                var it = mWindowContextDic.GetEnumerator();
                while (it.MoveNext())
                {
                    var w = it.Current.Value;
                    if (w != window && w.active && w.parent == null)
                    {
                        if (nav.hideWindows == null)
                        {
                            nav.hideWindows = new List<WindowContext>();
                        }
                        nav.hideWindows.Add(w);
                        SetActive(w, false);
                    }
                }

                if(nav.hideWindows!= null)
                {
                    nav.hideWindows.Sort(SortWindow);
                }
            }
            mWindowStack.Insert(0,nav);
        }
    }

    private int SortWindow(WindowContext a, WindowContext b)
    {
        var canvasA = GetCanvas(a);
        var canvasB = GetCanvas(b);
        if (canvasA == null && canvasB == null)
        {
            return 0;
        }
        else if (canvasA != null && canvasB == null)
        {
            return 1;
        }
        else if (canvasA == null && canvasB != null)
        {
            return -1;
        }
        else
        {
            if (canvasA.sortingOrder > canvasB.sortingOrder)
            {
                return -1;
            }
            else if (canvasA.sortingOrder < canvasB.sortingOrder)
            {
                return 1;
            }
        }
        return 0;
    }

    public void SetActive(WindowContext context, bool active)
    {
        if (context == null)
        {
            return;
        }
        if (active)
        {
            SetOrder(context);
        }
        else
        {
            var canvas = GetCanvas(context);
            if(canvas!=null)
            {
                canvas.sortingOrder = 0;
            }
        }

        context.active = active;

        GameObject go = GetObject(context);

        SetLayer(go, active ? WindowContext.LAYER : WindowContext.LAYER_HIDE);

        SetWidgetActive(context, active);

    }
    private void SetWidgetActive(WindowContext context, bool active)
    {
        if (context.fixedWidgets != null && active)
        {
            for (int i = 0; i < context.fixedWidgets.Count; ++i)
            {
                var widget = context.fixedWidgets[i];

                if (context.widgets.ContainsKey(widget.name) == false)
                {
                    Open(widget, context, null);
                }
            }
        }

        var it = context.widgets.GetEnumerator();
        while (it.MoveNext())
        {
            var widget = it.Current.Value;

            widget.parent = context;

            if (widget.status == WindowStatus.Done)
            {
                SetActive(widget, active);
            }
        }
    }
    private Canvas GetCanvas(WindowContext context)
    {
        if(mWindowObjectDic.TryGetValue(context.name, out GameObject go))
        {
            return go.GetComponent<Canvas>();
        }
        return null;
    }

    private void SetOrder(WindowContext window)
    {
        if (window == null) return;

        var canvas = GetCanvas(window);
        if(canvas == null)
        {
            return;
        }

        if (window.type == WindowType.Widget && window.fixedOrder != 0)
        {
            canvas.sortingOrder = window.fixedOrder;
        }
        else
        {
            if (window.parent != null)
            {
                var parent = GetCanvas(window.parent);
                if (parent != null)
                {
                    canvas.sortingOrder = parent.sortingOrder + window.widgetOrderAddition;
                }
            }
            else
            {

                int maxOrder = int.MinValue;
                var it = mWindowContextDic.GetEnumerator();
                while (it.MoveNext())
                {
                    var v = it.Current.Value;
                    var c = GetCanvas(v);
                    if (c != null && v.fixedOrder == 0 && v.parent == null)
                    {
                        if (c.sortingOrder > maxOrder)
                        {
                            maxOrder = c.sortingOrder;
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
                canvas.sortingOrder = maxOrder;
            }
        }
    }

    public WindowContext Get(string name)
    {
        mWindowContextDic.TryGetValue(name, out WindowContext context);
        return context;
    }

    public GameObject GetObject(WindowContext context)
    {
        if(context == null)
        {
            return null;
        }
        mWindowObjectDic.TryGetValue(context.name, out GameObject go);
        return go;
    }
   
    public void CloseAll(bool destroy = true)
    {
        mCloseList.Clear();
        mCloseList.AddRange(mWindowContextDic.Keys);
        for (int i = 0; i < mCloseList.Count; ++i)
        {
            var context = Get(mCloseList[i]);
            if (destroy)
            {
                DestroyWindow(context);
            }
            else
            {
                SetActive(context, false);
            }
        }
        mCloseList.Clear();
        mWindowStack.Clear();
    }
 
   
    public void CloseAllAndOpen(WindowContext context, WindowContext parent = null, Action<GameObject> callback = null,bool destroy = true) 
    {
    
        mCloseList.Clear();
        var it = mWindowContextDic.GetEnumerator();
        while(it.MoveNext())
        {
            if( context == null || it.Current.Key!= context.name)
            {
                if(context == null || context.widgets.ContainsKey(it.Current.Key) ==false)
                {
                    mCloseList.Add(it.Current.Key);
                }
               
            }
        }
        for(int i = 0; i< mCloseList.Count; ++i)
        {
            string key = mCloseList[i];
            if (mWindowContextDic.TryGetValue(key, out WindowContext w))
            {
                if (destroy)
                {
                    DestroyWindow(w);
                }
                else
                {
                    SetActive(w, false);
                }
            }
        }
        mWindowStack.Clear();

        Open(context, parent, callback);
    }

    public void Close(WindowContext window,bool destroy = true)
    {
        if(window == null)
        {
            return;
        }
        if (window.type == WindowType.Normal)
        {
            int index =  mWindowStack.FindIndex((nav) => { return nav.window == window; });
            if (index >= 0 && index < mWindowStack.Count)
            {
                WindowNav current = mWindowStack[index];

                WindowNav previous = null;
                int previousIndex = index + 1;
                if (previousIndex < mWindowStack.Count)
                {
                    previous = mWindowStack[previousIndex];
                }

                mWindowStack.RemoveAt(index);
                if (mWindowStack.FindIndex((nav) => { return nav.window == window; }) < 0 && destroy)
                {
                    DestroyWindow(window);
                }
                else
                {
                    SetActive(window, false);
                }
                if (current != null && current.hideWindows != null)
                {
                    for (int i = 0; i < current.hideWindows.Count; ++i)
                    {
                        SetActive(current.hideWindows[i], true);
                    }
                }

                if (previous != null)
                {
                    if(previous.window.hideOther == false && previousIndex < mWindowStack.Count)
                    {
                        var previousPrevious = mWindowStack[previousIndex];
                        
                        SetActive(previousPrevious.window, true);
                    }
                    SetActive(previous.window, true);
                }
            }
            else
            {
                if (destroy)
                {
                    DestroyWindow(window);
                }
                else
                {
                    SetActive(window, false);
                }
            }          
        }
        else
        {
            if (destroy)
            {
                DestroyWindow(window);
            }
            else
            {
                SetActive(window, false);
            }
        }
    }

    private void DestroyWindow(WindowContext context)
    {
        if(context== null)
        {
            return;
        }
        SetActive(context, false);

        mWindowContextDic.Remove(context.name);

        GameObject go = GetObject(context);
   
        Destroy(go);
        
        mWindowObjectDic.Remove(context.name);
        context.Clear();
    }

  

    public static void SetLayer( GameObject go, int layer)
    {
        if (go == null || go.layer == layer)
        {
            return;
        }
        go.layer = layer;

        var transforms = go.GetComponentsInChildren<Transform>();
        for (int i = 0; i < transforms.Length; ++i)
        {
            var child = transforms[i].gameObject;
            if (child.layer != WindowContext.LAYER_MODEL)
            {
                child.layer = layer;
            }
        }
    }
}
