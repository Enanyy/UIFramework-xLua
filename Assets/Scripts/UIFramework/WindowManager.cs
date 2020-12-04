using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;

public class WindowManager : MonoBehaviour
{
    private static WindowManager mInstance;
    public static WindowManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = FindObjectOfType<WindowManager>();
                if (mInstance == null)
                {
                    GameObject go = new GameObject(typeof(WindowManager).Name);
                    mInstance = go.AddComponent<WindowManager>();
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return mInstance;
        }
    }

    private class WindowNav
    {
        public WindowContext window;
        public List<WindowContext> hideWindows;
    }

    private Dictionary<ulong, WindowContextBase> mWindowContextDic = new Dictionary<ulong, WindowContextBase>();
    private Dictionary<ulong, GameObject> mWindowObjectDic = new Dictionary<ulong, GameObject>();

    private List<WindowNav> mWindowStack = new List<WindowNav>();
    private List<ulong> mCloseList = new List<ulong>();


    private Camera mCamera;
    private EventSystem mEventSystem;

    /// <summary>
    /// 两个normal之间层级差
    /// </summary>
    private int mOrderAddition = 50;

    private Action<string, Action<UnityEngine.Object>> mLoader;

    private bool mInitialized = false;

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if(mInitialized)
        {
            return;
        }
        mInitialized = true;

        GameObject camera = new GameObject("Camera");
        camera.transform.SetParent(transform);
        camera.layer = WindowContextBase.LAYER;
        mCamera = camera.AddComponent<Camera>();
        mCamera.clearFlags = CameraClearFlags.Depth;
        mCamera.depth = 10;
        mCamera.orthographic = false; //--使用透视投影，这样UI模型的3D立体感更强
        mCamera.orthographicSize = 10;
        mCamera.fieldOfView = 60;
        mCamera.cullingMask = 1 << WindowContextBase.LAYER;

        GameObject eventsystem = new GameObject("EventSystem");
        eventsystem.transform.SetParent(transform);
        eventsystem.layer = WindowContextBase.LAYER;
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



    public void Open(WindowContextBase context, WidgetContext widget = null, Action<GameObject> callback = null)
    {
        if (mLoader == null)
        {
            Debug.Assert(mLoader != null);
            callback?.Invoke(null);
            return;
        }

        if (context == null)
        {
            Debug.Assert(context != null);
            callback?.Invoke(null);
            return;
        }

        SetTouch(false);

        if (mWindowContextDic.ContainsKey(context.id) == false)
        {
            mWindowContextDic.Add(context.id, context);
        }


        if (widget != null)
        {
            Debug.Assert(context.type == WindowType.Normal);
            widget.parent = context as WindowContext;
        }

        GameObject go = GetObject(context);
        if (go == null)
        {
            if (context.status == WindowStatus.Loading)
            {
                return;
            }
            context.status = WindowStatus.Loading;
            mLoader(context.name, (asset) =>
            {
                if (asset == null || context.status == WindowStatus.None)
                {
                    mWindowContextDic.Remove(context.id);
                    context.Clear();
                    return;
                }


                context.status = WindowStatus.Done;
                if (Application.isPlaying)
                {
                    go = Instantiate(asset) as GameObject;
                }
                else
                {
#if UNITY_EDITOR
                    go = UnityEditor.PrefabUtility.InstantiatePrefab(asset) as GameObject;
#else
                    return;
#endif
                }
                if (mWindowObjectDic.ContainsKey(context.id))
                {
                    Destroy(mWindowObjectDic[context.id]);
                    mWindowObjectDic[context.id] = go;
                }
                else
                {
                    mWindowObjectDic.Add(context.id, go);
                }
                AddComponent(go, context);
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

                Push(context as WindowContext);

                SetActive(context, true);
                SetTouch(true);

                callback?.Invoke(go);


            });
        }
        else
        {
            context.status = WindowStatus.Done;

            Push(context as WindowContext);

            SetActive(context, true);
            SetTouch(true);

            callback?.Invoke(go);
        }
    }

    private void Push(WindowContext context)
    {
        if (context == null)
        {
            return;
        }

        if (context.fixedOrder == 0)
        {
            WindowNav nav = new WindowNav();
            nav.window = context;

            if (context.hideOther)
            {
                var it = mWindowContextDic.GetEnumerator();
                while (it.MoveNext())
                {
                    var w = it.Current.Value as WindowContext;
                    if (w != null && w != context && w.active)
                    {
                        if (nav.hideWindows == null)
                        {
                            nav.hideWindows = new List<WindowContext>();
                        }
                        nav.hideWindows.Add(w);
                        SetActive(w, false);
                    }
                }

                if (nav.hideWindows != null)
                {
                    nav.hideWindows.Sort(SortWindow);
                }
            }
            mWindowStack.Insert(0, nav);
        }
    }

    private void AddComponent(GameObject go, WindowContextBase context)
    {
        if (go == null)
        {
            return;
        }
        if (context.component != null && go.GetComponent(context.component) == null)
        {
            var component = go.AddComponent(context.component) as WindowComponent;
            if (component != null)
            {
                component.context = context;
            }
        }
        var components = go.GetComponentsInChildren<WindowComponent>();
        for (int i = 0; i < components.Length; ++i)
        {
            components[i].context = context;
        }
    }
    private void SetComponentActive(GameObject go, bool active)
    {
        if (go == null)
        {
            return;
        }
        var components = go.GetComponentsInChildren<WindowComponent>();
        for (int i = 0; i < components.Length; ++i)
        {
            if (active)
            {
                components[i].OnShow();
            }
            else
            {
                components[i].OnHide();
            }
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

    public void SetActive(WindowContextBase context, bool active)
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
            if (canvas != null)
            {
                canvas.sortingOrder = 0;
            }
        }
        GameObject go = GetObject(context);

        if (context.active != active)
        {
            context.active = active;


            SetLayer(go, context.layer);


            SetComponentActive(go, active);
        }
        SetWidgetActive(context as WindowContext, active);
    }
    private void SetWidgetActive(WindowContext context, bool active)
    {
        if (context == null)
        {
            return;
        }
        if (context.fixedWidgets != null && active)
        {
            for (int i = 0; i < context.fixedWidgets.Count; ++i)
            {
                var widget = context.fixedWidgets[i];
                widget.parent = context;
            }
        }

        var it = context.widgets.GetEnumerator();
        while (it.MoveNext())
        {
            var widget = it.Current.Value;

            var go = GetObject(widget);
            if (go != null)
            {
                SetActive(widget, active);
            }
            else
            {
                Open(widget, null);
            }
        }
    }
    private Canvas GetCanvas(WindowContextBase context)
    {
        if (mWindowObjectDic.TryGetValue(context.id, out GameObject go))
        {
            return go.GetComponent<Canvas>();
        }
        return null;
    }

    private void SetOrder(WindowContextBase context)
    {
        if (context == null) return;

        var canvas = GetCanvas(context);
        if (canvas == null)
        {
            return;
        }
        var widget = context as WidgetContext;
        if (widget != null)
        {
            var parent = GetCanvas(widget.parent);
            if (parent != null)
            {
                canvas.sortingOrder = parent.sortingOrder + widget.sortingOrderOffset;
            }
        }

        else
        {
            var window = context as WindowContext;
            //固定层级
            if (window != null && window.fixedOrder != 0)
            {
                canvas.sortingOrder = window.fixedOrder;
            }
            else
            {
                int maxOrder = int.MinValue;
                var it = mWindowContextDic.GetEnumerator();
                while (it.MoveNext())
                {
                    var v = it.Current.Value as WindowContext;
                    if (v != null)
                    {
                        var c = GetCanvas(v);
                        if (c != null && v.fixedOrder == 0)
                        {
                            if (c.sortingOrder > maxOrder)
                            {
                                maxOrder = c.sortingOrder;
                            }
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

    private WindowContextBase Get(ulong id)
    {
        mWindowContextDic.TryGetValue(id, out WindowContextBase context);
        return context;
    }

    public GameObject GetObject(WindowContextBase context)
    {
        if (context == null)
        {
            return null;
        }
        return GetObject(context.id);
    }

    public GameObject GetObject(ulong id)
    {
        mWindowObjectDic.TryGetValue(id, out GameObject go);
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


    public void CloseAllAndOpen(WindowContextBase context, WidgetContext widget = null, Action<GameObject> callback = null)
    {
        mCloseList.Clear();
        var it = mWindowContextDic.GetEnumerator();
        while (it.MoveNext())
        {
            if (context == null || it.Current.Key != context.id)
            {
                if (context == null)
                {
                    mCloseList.Add(it.Current.Key);
                }
                else
                {
                    var window = context as WindowContext;
                    if (window == null || window.widgets.ContainsKey(it.Current.Key) == false)
                    {
                        mCloseList.Add(it.Current.Key);
                    }
                }
            }
        }
        for (int i = 0; i < mCloseList.Count; ++i)
        {
            ulong key = mCloseList[i];
            if (mWindowContextDic.TryGetValue(key, out WindowContextBase w))
            {

                DestroyWindow(w);

            }
        }
        mWindowStack.Clear();

        Open(context, widget, callback);
    }

    public void Close(WindowContextBase window)
    {
        if (window == null)
        {
            return;
        }
        if (window.type == WindowType.Normal)
        {
            int index = mWindowStack.FindIndex((nav) => { return nav.window == window; });
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
                if (mWindowStack.FindIndex((nav) => { return nav.window == window; }) < 0)
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
                    if (previous.window.hideOther == false && previousIndex < mWindowStack.Count)
                    {
                        var previousPrevious = mWindowStack[previousIndex];

                        SetActive(previousPrevious.window, true);
                    }
                    SetActive(previous.window, true);
                }
            }
            else
            {
                DestroyWindow(window);
            }
        }
        else
        {
            DestroyWindow(window);
        }
    }

    private void DestroyWindow(WindowContextBase context)
    {
        if (context == null)
        {
            return;
        }
        SetActive(context, false);

        if (context.closeDestroy)
        {
            DestroyWindowObject(context);
        }
    }

    private void DestroyWindowObject(WindowContextBase context)
    {
        mWindowContextDic.Remove(context.id);

        GameObject go = GetObject(context);

        Destroy(go);

        mWindowObjectDic.Remove(context.id);

        var w = context as WindowContext;
        if (w != null)
        {
            var it = w.widgets.GetEnumerator();
            while (it.MoveNext())
            {
                DestroyWindow(it.Current.Value);
            }
        }

        context.Clear();
    }



    public static void SetLayer(GameObject go, int layer)
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
