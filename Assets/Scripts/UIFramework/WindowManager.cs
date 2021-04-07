#define ENABLE_WINDOW_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Xml;
using System.Reflection;
using System.Collections;


[AttributeUsage(AttributeTargets.Method)]
public class WindowMenuAttribute : Attribute
{
    public string Title { get; private set; }
    public WindowMenuAttribute(string title)
    {
        Title = title;
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
                mInstance = FindObjectOfType<WindowManager>();
                if (mInstance == null)
                {
                    GameObject go = new GameObject(typeof(WindowManager).Name);
                    mInstance = go.AddComponent<WindowManager>();
                }
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(mInstance.gameObject);
                }
            }
            return mInstance;
        }
    }
    private class WindowState
    {
        public WindowContext context;
        public Dictionary<ulong, bool> widgetsActive;//context的widgets激活状态
    }

    private class WindowNav
    {
        public WindowState windowState;
        public List<WindowState> hideWindowStates;
    }
    public readonly Dictionary<string, WindowContextBase> contexts = new Dictionary<string, WindowContextBase>();

    private Dictionary<ulong, WindowContextBase> mWindowContextDic = new Dictionary<ulong, WindowContextBase>();
    private Dictionary<ulong, GameObject> mWindowObjectDic = new Dictionary<ulong, GameObject>();

    private List<WindowNav> mWindowStack = new List<WindowNav>();
    private List<ulong> mTempList = new List<ulong>();


    private Camera mCamera;
    private EventSystem mEventSystem;

    /// <summary>
    /// 两个normal之间层级差
    /// </summary>
    private int mOrderAddition = 50;

    private Action<string, Action<UnityEngine.Object>> mLoader;

    private bool mInitialized = false;
    public bool initialized { get { return mInitialized; } }

    public Vector2 DesignResolution = new Vector2(1920, 1080);

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (mInitialized)
        {
            return;
        }
        mInitialized = true;
        mCamera = GetComponentInChildren<Camera>();
        if (mCamera == null)
        {
            GameObject camera = new GameObject("Camera");
            camera.transform.SetParent(transform);
            camera.layer = WindowContextBase.LAYER;
            mCamera = camera.AddComponent<Camera>();
            mCamera.clearFlags = CameraClearFlags.Depth;
            mCamera.depth = 10;
            mCamera.orthographic = true;
            mCamera.orthographicSize = 10;
            mCamera.cullingMask = 1 << WindowContextBase.LAYER;
        }
        mEventSystem = GetComponentInChildren<EventSystem>();
        if (mEventSystem == null)
        {
            GameObject eventsystem = new GameObject("EventSystem");
            eventsystem.transform.SetParent(transform);
            eventsystem.layer = WindowContextBase.LAYER;
            mEventSystem = eventsystem.AddComponent<EventSystem>();
            mEventSystem.sendNavigationEvents = true;
            mEventSystem.pixelDragThreshold = 5;

            eventsystem.AddComponent<StandaloneInputModule>();
        }
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


    public void Load(string xml)
    {
        if (string.IsNullOrEmpty(xml))
        {
            return;
        }

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);
        var widgetNode = doc.SelectSingleNode("/Root/WidgetContexts");

        var widgetIter = widgetNode.ChildNodes.GetEnumerator();
        while (widgetIter.MoveNext())
        {
            var child = widgetIter.Current as XmlElement;

            if (child.Name == "WidgetContext")
            {
                WidgetContext widget = new WidgetContext();
                widget.ParseXml(child);

                contexts.Add(widget.name, widget);
            }
        }

        var windowNode = doc.SelectSingleNode("/Root/WindowContexts");

        var windowIter = windowNode.ChildNodes.GetEnumerator();
        while (windowIter.MoveNext())
        {
            var child = windowIter.Current as XmlElement;

            if (child.Name == "WindowContext")
            {
                WindowContext context = new WindowContext();
                context.ParseXml(child, GetWidgetContext);
                contexts.Add(context.name, context);
            }
        }
    }

    public WindowContextBase GetWindowContextBase(string name)
    {
        WindowContextBase context;
        contexts.TryGetValue(name, out context);
        return context;
    }

    public WidgetContext GetWidgetContext(string name)
    {
        return GetWindowContextBase(name) as WidgetContext;
    }
    public WindowContext GetWindowContext(string name)
    {
        return GetWindowContextBase(name) as WindowContext;
    }

    public void Open(string name, Action<GameObject> callback = null, params string[] widgets)
    {
        WindowContextBase context = GetWindowContextBase(name);
        if (context == null)
        {
            return;
        }

        WidgetContext[] widgetContexts = null;
        if (widgets != null)
        {
            widgetContexts = new WidgetContext[widgets.Length];
            for (int i = 0; i < widgets.Length; ++i)
            {
                widgetContexts[i] = GetWidgetContext(widgets[i]);
            }
        }
        Open(context, callback, widgetContexts);
    }
    public void Close(string name)
    {
        WindowContextBase context = GetWindowContextBase(name);
        if (context == null)
        {
            return;
        }
        Close(context);
    }
    public void CloseAll(params string[] excepts)
    {
        WindowContext[] contexts = null;
        if (excepts != null)
        {
            contexts = new WindowContext[excepts.Length];
            for (int i = 0; i < excepts.Length; ++i)
            {
                contexts[i] = GetWindowContext(excepts[i]);
            }
        }
        CloseAll(contexts);
    }

    public void SetWidgetsActive(string context, bool active)
    {
        WindowContext windowContext = GetWindowContext(context);
        if (windowContext == null || mWindowContextDic.ContainsKey(windowContext.id) == false)
        {
            return;
        }
        SetWidgetsActive(windowContext, active);
    }

    public void SetWidgetActive(string context, string widget, bool active)
    {
        WindowContext windowContext = GetWindowContext(context);
        if (windowContext == null || mWindowContextDic.ContainsKey(windowContext.id) == false)
        {
            return;
        }
        WidgetContext widgetContext = windowContext.GetWidget(widget);
        if (widgetContext == null)
        {
            return;
        }

        SetWidgetActive(windowContext, widgetContext, active);
    }

    public void Clear()
    {
        CloseAll();
        contexts.Clear();
        mInitialized = false;
#if ENABLE_WINDOW_EDITOR
        mMenuList.Clear();
        mMenuList = null;
#endif
    }


    public void Open(WindowContextBase context, Action<GameObject> callback = null, params WidgetContext[] widgets)
    {
        if (mLoader == null)
        {
            Debug.Assert(mLoader != null, "Loader is null!!");
            callback?.Invoke(null);
            return;
        }

        if (context == null)
        {
            Debug.Assert(context != null, "Context is null");
            callback?.Invoke(null);
            return;
        }

        SetTouch(false);

        if (mWindowContextDic.ContainsKey(context.id) == false)
        {
            mWindowContextDic.Add(context.id, context);
        }


        if (widgets != null && widgets.Length > 0)
        {
            Debug.Assert(context.type == WindowType.Normal, string.Format("{0} type = {1}", context.name, context.type));
            var it = widgets.GetEnumerator();
            while (it.MoveNext())
            {
                WidgetContext widget = it.Current as WidgetContext;
                if (widget != null)
                {
                    widget.parent = context as WindowContext;
                }
            }
        }

        GameObject go = GetObject(context);
        if (go == null)
        {
            if (context.status == WindowStatus.Loading)
            {
                return;
            }
            context.status = WindowStatus.Loading;
            mLoader(context.path, (asset) =>
            {
                SetTouch(true);

                if (asset == null || context.status == WindowStatus.None)
                {
                    if (asset == null)
                    {
                        Debug.LogError("Can't load ui with path= " + context.path);
                    }
                    if (context.status == WindowStatus.None)
                    {
                        Debug.LogError("context.name=" + context.name + " status=" + context.path);
                    }
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
                go.name = System.IO.Path.GetFileNameWithoutExtension(context.path);
                if (mWindowObjectDic.ContainsKey(context.id))
                {
                    DestroyGameObject(mWindowObjectDic[context.id]);
                    mWindowObjectDic[context.id] = go;
                }
                else
                {
                    mWindowObjectDic.Add(context.id, go);
                }
                AddComponent(go, context);

                go.TryGetComponent(out Canvas canvas);
                if (canvas == null) canvas = go.AddComponent<Canvas>();
                go.TryGetComponent(out CanvasScaler scaler);

                go.transform.SetParent(transform);
                bool isScale = context.type == WindowType.Normal;
                if (isScale == false)
                {
                    var widget = context as WidgetContext;
                    if (widget.parent == null)
                    {
                        isScale = true;
                    }
                }

                if (isScale)
                {
                    if (scaler == null) scaler = go.AddComponent<CanvasScaler>();

                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = mCamera;
                    canvas.sortingLayerName = "UI";
                    canvas.overrideSorting = false;

                    scaler.scaleFactor = 1;
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.referenceResolution = DesignResolution;
                    scaler.referencePixelsPerUnit = 100;
                }
                else
                {

                    if (scaler != null)
                    {
                        DestroyImmediate(scaler);
                    }
                }
                go.SetActive(true);

                Push(context as WindowContext);

                SetActive(context, true);

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
            nav.windowState = GetWindowState(context);

            if (context.hideOther)
            {
                using (var it = mWindowContextDic.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var w = it.Current.Value as WindowContext;
                        if (w != null && w != context && w.active)
                        {
                            if (nav.hideWindowStates == null)
                            {
                                nav.hideWindowStates = new List<WindowState>();
                            }
                            WindowState windowState = GetWindowState(w);

                            nav.hideWindowStates.Add(windowState);
                            SetActive(w, false);
                        }
                    }
                }
                if (nav.hideWindowStates != null)
                {
                    nav.hideWindowStates.Sort(SortWindow);
                }
            }
            mWindowStack.Insert(0, nav);
        }
    }
    private WindowState GetWindowState(WindowContext context)
    {
        WindowState windowActive = new WindowState();
        windowActive.context = context;
        if (context.widgets != null && context.widgets.Count > 0)
        {
            windowActive.widgetsActive = new Dictionary<ulong, bool>();
            using (var iter = context.widgets.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    var widget = iter.Current.Value;
                    windowActive.widgetsActive.Add(widget.id, widget.active);
                }
            }
        }
        return windowActive;
    }

    private void AddComponent(GameObject go, WindowContextBase context)
    {
        if (Application.isPlaying == false)
        {
            return;
        }
        if (go == null)
        {
            return;
        }
        if (context.components != null)
        {
            using (var it = context.components.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var type = it.Current.Key;

                    WindowComponent component = go.GetComponent(type) as WindowComponent;
                    if (component == null)
                    {
                        component = go.AddComponent(type) as WindowComponent;
                    }
                    if (component != null)
                    {
                        component.contextbase = context;
                        component.parameters = it.Current.Value;
                    }
                }
            }

        }
        var components = go.GetComponentsInChildren<WindowComponent>();
        for (int i = 0; i < components.Length; ++i)
        {
            components[i].contextbase = context;
        }
        for (int i = 0; i < components.Length; ++i)
        {
            components[i].OnInit();
        }
    }
    private void SetComponentActive(GameObject go, bool active)
    {
        if (Application.isPlaying == false)
        {
            return;
        }

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

    private int SortWindow(WindowState a, WindowState b)
    {
        var canvasA = GetCanvas(a.context);
        var canvasB = GetCanvas(b.context);
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

    public void SetActive(WindowContextBase context, bool active, Dictionary<ulong, bool> widgetsActive = null)
    {
        if (context == null)
        {
            return;
        }
        var canvas = GetCanvas(context);

        if (active)
        {
            SetOrder(context);
        }
        else
        {
            if (canvas != null)
            {
                canvas.sortingOrder = 0;
            }
        }
        GameObject go = GetObject(context);

        if (context.active != active)
        {
            context.active = active;
            if (canvas != null)
            {
                canvas.enabled = context.active;
            }

            SetComponentActive(go, active);
        }
        if (context.type == WindowType.Normal)
        {
            SetWidgetsActive(context as WindowContext, active, widgetsActive);
        }
        else
        {
            var widget = context as WidgetContext;
            if (widget != null)
            {
                var parent = GetObject(widget.parent);
                if (parent != null)
                {
                    Transform bindNode = parent.transform;
                    if(!string.IsNullOrEmpty(widget.bindNode))
                    {
                        bindNode = parent.transform.Find(widget.bindNode);
                    }

                    if (bindNode != go.transform.parent)
                    {
                        go.transform.SetParent(bindNode);
                        go.transform.localScale = Vector3.one;
                        go.transform.localPosition = Vector3.zero;

                        RectTransform rect = go.transform as RectTransform;

                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.offsetMin = Vector2.zero;
                        rect.offsetMax = Vector2.zero;

                        if (canvas != null)
                        {
                            canvas.overrideSorting = true;
                        }
                    }

                    var components = parent.GetComponentsInChildren<WindowComponent>();
                    for (int i = 0; i < components.Length; ++i)
                    {
                        var com = components[i];

                        if (com.contextbase != null && com.contextbase.id == widget.id)
                        {
                            continue;
                        }
                        if (active)
                        {
                            com.OnWidgetShow(widget);
                        }
                        else
                        {
                            com.OnWidgetHide(widget);
                        }
                    }
                }
            }
        }
    }
    public bool IsActive(WindowContextBase context)
    {
        if (context == null)
        {
            return false;
        }
        return context.active && mWindowContextDic.ContainsKey(context.id);
    }

    public void SetWidgetsActive(WindowContext context, bool active, Dictionary<ulong, bool> widgetsActive = null)
    {
        if (context == null)
        {
            return;
        }
        if (context.fixedWidgets != null && active)
        {
            using (var it = context.fixedWidgets.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    bool defaultActive = true;
                    var param = context.GetWidgetParam(it.Current.Key);
                    if(param!=null)
                    {
                        defaultActive = param.defaultActive;
                    }
                    if(!Application.isPlaying)
                    {
                        defaultActive = true;
                    }

                    if (defaultActive)
                    {
                        if (Application.isPlaying && widgetsActive != null && widgetsActive.TryGetValue(it.Current.Value.id, out bool val))
                        {
                            if (val == active)
                            {
                                it.Current.Value.parent = context;
                            }
                        }
                        else
                        {
                            it.Current.Value.parent = context;
                        }
                    }
                }
            }
        }

        using (var it = context.widgets.GetEnumerator())
        {
            while (it.MoveNext())
            {
                var widget = it.Current.Value;
                if (Application.isPlaying && widgetsActive != null && widgetsActive.TryGetValue(widget.id, out bool val))
                {
                    if (val == active)
                    {
                        SetWidgetActive(context, widget, active);
                    }
                }
                else
                {
                    SetWidgetActive(context, widget, active);
                }
            }
        }
    }
    public void SetWidgetActive(WindowContext context, WidgetContext widget, bool active)
    {
        widget.parent = context;

        var go = GetObject(widget);
        if (go != null)
        {
            SetActive(widget, active);
        }
        else
        {
            Open(widget);
        }
    }


    private Canvas GetCanvas(WindowContextBase context)
    {
        GameObject go = GetObject(context);

        if (go != null)
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
                using (var it = mWindowContextDic.GetEnumerator())
                {
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
        mTempList.Clear();
        mTempList.AddRange(mWindowContextDic.Keys);
        for (int i = 0; i < mTempList.Count; ++i)
        {
            var context = Get(mTempList[i]);
            if (destroy)
            {
                DestroyWindow(context);
            }
            else
            {
                SetActive(context, false);
            }
        }
        mTempList.Clear();
        mWindowStack.Clear();
    }


    public void CloseAll(params WindowContext[] excepts)
    {
        mTempList.Clear();
        using (var it = mWindowContextDic.GetEnumerator())
        {
            while (it.MoveNext())
            {
                if (excepts == null)
                {
                    mTempList.Add(it.Current.Key);
                }
                else
                {
                    bool close = true;
                    for (int i = 0; i < excepts.Length; ++i)
                    {
                        var w = excepts[i];
                        if (w.id == it.Current.Key)
                        {
                            close = false;
                            break;
                        }
                        if (w.widgets.ContainsKey(it.Current.Key))
                        {
                            close = false;
                            break;
                        }
                    }
                    if (close)
                    {
                        mTempList.Add(it.Current.Key);
                    }
                }
            }
        }
        for (int i = 0; i < mTempList.Count; ++i)
        {
            ulong key = mTempList[i];
            for (int j = 0; j < mWindowStack.Count;)
            {
                var nav = mWindowStack[j];
                if (nav.windowState.context.id == key)
                {
                    mWindowStack.RemoveAt(j);
                    continue;
                }
                else
                {
                    if (nav.hideWindowStates != null)
                    {
                        for (int k = 0; k < nav.hideWindowStates.Count;)
                        {
                            if (nav.hideWindowStates[k].context.id == key)
                            {
                                nav.hideWindowStates.RemoveAt(k);
                                continue;
                            }
                            ++k;
                        }
                    }
                }

                ++j;

            }
            if (mWindowContextDic.TryGetValue(key, out WindowContextBase w))
            {
                DestroyWindow(w);
            }
        }
        mTempList.Clear();

        if (mWindowStack.Count > 0)
        {
            var current = mWindowStack[mWindowStack.Count - 1];
            SetActive(current.windowState.context, true, current.windowState.widgetsActive);
        }
    }

    public void Close(WindowContextBase context)
    {
        if (context == null)
        {
            return;
        }
        if (context.type == WindowType.Normal)
        {
            int index = mWindowStack.FindIndex((nav) => { return nav.windowState.context == context; });
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
                if (mWindowStack.FindIndex((nav) => { return nav.windowState.context == context; }) < 0)
                {
                    DestroyWindow(context);
                }
                else
                {
                    SetActive(context, false);
                }
                mTempList.Clear();
                if (current != null && current.hideWindowStates != null)
                {
                    for (int i = 0; i < current.hideWindowStates.Count; ++i)
                    {
                        SetActive(current.hideWindowStates[i].context, true, current.hideWindowStates[i].widgetsActive);
                        mTempList.Add(current.hideWindowStates[i].context.id);
                    }
                }

                if (previous != null)
                {
                    if (previous.windowState.context.hideOther == false && previousIndex < mWindowStack.Count)
                    {
                        var previousPrevious = mWindowStack[previousIndex];

                        if (mTempList.Contains(previousPrevious.windowState.context.id))
                        {
                            SetActive(previousPrevious.windowState.context, true, previousPrevious.windowState.widgetsActive);
                        }
                    }
                    if (mTempList.Contains(previous.windowState.context.id) == false)
                    {
                        SetActive(previous.windowState.context, true, previous.windowState.widgetsActive);
                    }
                }
                mTempList.Clear();
            }
            else
            {
                DestroyWindow(context);
            }
        }
        else
        {
            DestroyWindow(context);
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

        DestroyGameObject(go);

        mWindowObjectDic.Remove(context.id);

        var w = context as WindowContext;
        if (w != null)
        {
            using (var it = w.widgets.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    DestroyWindow(it.Current.Value);
                }
            }
        }

        context.Clear();
    }
    public static void DestroyGameObject(GameObject go)
    {
        if (go == null)
        {
            return;
        }
        DestroyImmediate(go);
    }

    #region Debug Draw
#if ENABLE_WINDOW_EDITOR
    private Vector2 mScroll;
    private string mKey;
    private bool mVisible;
    private Rect mWindowRect;
    public const string WindowTitle = "UI定义列表";
    readonly Rect TitleBarRect = new Rect(0, 0, 10000, 20);

    private const int margin = 0;

    private Dictionary<WindowMenuAttribute,Action> mMenuList = null;

    public void Draw(Rect rect)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("搜索", GUILayout.Width(30), GUILayout.Height(30));
        mKey = GUILayout.TextField(mKey, GUILayout.Width(200), GUILayout.Height(30));
        if (string.IsNullOrEmpty(mKey) == false)
        {
            mKey = mKey.Trim().ToLower();

            if (GUILayout.Button("Reset", GUILayout.Width(60), GUILayout.Height(30)))
            {
                mKey = null;
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        mScroll = GUILayout.BeginScrollView(mScroll, false, true);
        float width = 200f;
        int count = (int)(rect.width / width);

        DrawItems(count, contexts.GetEnumerator(), (current) =>
        {
            WindowContextBase val = current.Value;
            if (val != null)
            {
                bool visible = true;
                if (string.IsNullOrEmpty(mKey) == false)
                {
                    if (val.name.ToLower().Contains(mKey) == false)
                    {
                        visible = false;
                    }
                }
                if (visible)
                {
                    bool open = GetObject(val);
                    GUIStyle style = GUI.skin.button;
                    style.normal.textColor = open ? Color.yellow : Color.white;

                    if (GUILayout.Button(val.name, style, GUILayout.Width(width), GUILayout.Height(30)))
                    {
                        if (!open)
                        {
                            Open(val);
                        }
                        else
                        {
                            Close(val);
                        }
                    }
                }
            }
        });

        if(mMenuList == null)
        {
            mMenuList = new Dictionary<WindowMenuAttribute, Action>();
            var types = GetType().Assembly.GetTypes();
            foreach (var type in types)
            {
                MethodInfo[] methods = type.GetMethods( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                for (int i = 0; methods != null && i < methods.Length; ++i)
                {
                    var method = methods[i];

                    // only find static functions
                    if (method.IsStatic)
                    {
                        WindowMenuAttribute attribute = method.GetCustomAttribute(typeof(WindowMenuAttribute), true) as WindowMenuAttribute;
                        if(attribute!=null)
                        {
                            mMenuList.Add(attribute, (Action)Delegate.CreateDelegate(typeof(Action),  method));
                        }
                    }
                }
            }
        }

        if (mMenuList.Count > 0)
        {
            GUILayout.Space(10);
            DrawItems(count, mMenuList.GetEnumerator(), (current) => {
                GUIStyle style = GUI.skin.button;
                Action callback = current.Value;
                if (GUILayout.Button(current.Key.Title, style, GUILayout.Width(width), GUILayout.Height(30)))
                {
                    if(callback!=null)
                    {
                        callback();
                    }
                }
            });
        }


        GUILayout.EndScrollView();
    }

    private void DrawItems<T>(int count, IEnumerator<T> it, Action<T> func)
    {
        int i = 0;

        bool beginHorizontal = false;
        while (it.MoveNext())
        {
            if (i == 0)
            {
                GUILayout.BeginHorizontal();
                beginHorizontal = true;
            }

            if(func!=null)
            {
                func(it.Current);
            }

            i++;
            if (i == count)
            {
                if (beginHorizontal)
                {
                    GUILayout.EndHorizontal();
                    beginHorizontal = false;
                }
                i = 0;
            }

        }
        if (beginHorizontal)
        {
            GUILayout.EndHorizontal();
        }

    }

    void OnGUI()
    {
        if (mVisible)
        {
            mWindowRect = GUILayout.Window(123456, new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2)), DrawWindow, WindowTitle);
        }
        if (!mVisible)
        {
            DrawButton();
        }
    }

    void DrawButton()
    {
        int w = Screen.width / 6;
        GUILayout.BeginArea(new Rect(Screen.width  - w / 2, 0, w, Screen.height / 10));
        GUILayoutOption o1 = GUILayout.Height(40);
        GUILayoutOption o2 = GUILayout.Width(100);
        GUILayoutOption[] oo = { o1, o2 };
        if (GUILayout.Button(mVisible ? "Close" : "Show", oo)) mVisible = !mVisible;
        GUILayout.EndArea();
    }

    /// <summary>
    /// Displays a window that lists the recorded logs.
    /// </summary>
    /// <param name="windowID">Window ID.</param>
    void DrawWindow(int windowID)
    {
        Draw(mWindowRect);
        if (mVisible)
        {
            DrawButton();
        }
        // Allow the window to be dragged by its title bar.
        GUI.DragWindow(TitleBarRect);
    }
#endif
    #endregion
}
#region WindowDefEditor
#if UNITY_EDITOR && ENABLE_WINDOW_EDITOR
public class WindowDefEditor : UnityEditor.EditorWindow
{

    [UnityEditor.MenuItem("GameObject/UI/Open")]
    private static void OpenDefineWindow()
    {
        var editor = GetWindow<WindowDefEditor>(WindowManager.WindowTitle);
        editor.Initialize();
    }

    private void Initialize()
    {
        if (WindowManager.Instance.initialized == false || WindowManager.Instance.contexts.Count == 0)
        {

            WindowManager.Instance.Initialize();
            WindowManager.Instance.SetLoader(LoadInEditor);
            if(WindowManager.Instance.contexts.Count == 0)
            {
                var asset = Resources.Load<TextAsset>("UIDefine");
                WindowManager.Instance.Load(asset.text);
            }
        }
    }

    private void OnGUI()
    {
        WindowManager.Instance.Draw(position);
    }

  

    public static void LoadInEditor(string name, Action<GameObject> callback)
    {
        GameObject asset = Resources.Load<GameObject>(string.Format("UI/{0}", name));

        if (asset != null)
        {
            callback(asset);
        }
    }
   
    private void OnFocus()
    {
        Initialize();
    }
    private void OnDestroy()
    {
        WindowManager.Instance.Clear();
    }
}

#endif
#endregion