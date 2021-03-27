using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Xml;

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
    private class WindowNavStatus
    {
        public WindowContext context;
        public Dictionary<ulong, bool> widgetsActive;//context的激活状态
    }

    private class WindowNav
    {
        public WindowNavStatus windowStatus;
        public List<WindowNavStatus> hideWindowsStatus;
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
    public void CloseAllAndOpen(string name, Action<GameObject> callback = null, params string[] widgets)
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
        CloseAllAndOpen(context, callback, widgetContexts);
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
        if(windowContext== null || mWindowContextDic.ContainsKey(windowContext.id) == false)
        {
            return;
        }
        WidgetContext widgetContext = windowContext.GetWidget(widget);
        if(widgetContext == null)
        {
            return;
        }

        SetWidgetActive(windowContext, widgetContext, active);
    }

    public void Clear()
    {
        CloseAll();
        contexts.Clear();
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
            nav.windowStatus = GetWindowNavStatus(context);

            if (context.hideOther)
            {
                using (var it = mWindowContextDic.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var w = it.Current.Value as WindowContext;
                        if (w != null && w != context && w.active)
                        {
                            if (nav.hideWindowsStatus == null)
                            {
                                nav.hideWindowsStatus = new List<WindowNavStatus>();
                            }
                            WindowNavStatus windowNavStatus = GetWindowNavStatus(w);

                            nav.hideWindowsStatus.Add(windowNavStatus);
                            SetActive(w, false);
                        }
                    }
                }
                if (nav.hideWindowsStatus != null)
                {
                    nav.hideWindowsStatus.Sort(SortWindow);
                }
            }
            mWindowStack.Insert(0, nav);
        }
    }
    private WindowNavStatus GetWindowNavStatus(WindowContext context)
    {
        WindowNavStatus windowActive = new WindowNavStatus();
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
        if (go == null)
        {
            return;
        }
        if (context.components != null)
        {
            for (int i = 0; i < context.components.Count; ++i)
            {
                var type = context.components[i];

                WindowComponent component = go.GetComponent(type) as WindowComponent;
                if (component == null)
                {
                    component = go.AddComponent(type) as WindowComponent;
                }
                if (component != null)
                {
                    component.contextbase = context;
                }
            }
        }
        var components = go.GetComponentsInChildren<WindowComponent>();
        for (int i = 0; i < components.Length; ++i)
        {
            components[i].contextbase = context;
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

    private int SortWindow(WindowNavStatus a, WindowNavStatus b)
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

    public void SetActive(WindowContextBase context, bool active,Dictionary<ulong,bool> widgetsActive = null)
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
            var canvas = GetCanvas(context);
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
    }
    public bool IsActive(WindowContextBase context)
    {
        if(context==null)
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
                    if (it.Current.Value)
                    {
                        if (widgetsActive != null && widgetsActive.TryGetValue(it.Current.Key.id, out bool val))
                        {
                            if(val == active)
                            {
                                it.Current.Key.parent = context;
                            }
                        }
                        else
                        {
                            it.Current.Key.parent = context;
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
                if (widgetsActive != null && widgetsActive.TryGetValue(widget.id, out bool val))
                {
                    if(val==active)
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
        if (context != null && mWindowObjectDic.TryGetValue(context.id, out GameObject go))
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


    public void CloseAllAndOpen(WindowContextBase context, Action<GameObject> callback = null, params WidgetContext[] widgets)
    {
        mTempList.Clear();
        using (var it = mWindowContextDic.GetEnumerator())
        {
            while (it.MoveNext())
            {
                if (context == null || it.Current.Key != context.id)
                {
                    if (context == null)
                    {
                        mTempList.Add(it.Current.Key);
                    }
                    else
                    {
                        var window = context as WindowContext;
                        if (window == null || window.widgets.ContainsKey(it.Current.Key) == false)
                        {
                            mTempList.Add(it.Current.Key);
                        }
                    }
                }
            }
        }
        for (int i = 0; i < mTempList.Count; ++i)
        {
            ulong key = mTempList[i];
            if (mWindowContextDic.TryGetValue(key, out WindowContextBase w))
            {
                DestroyWindow(w);
            }
        }
        mWindowStack.Clear();

        Open(context, callback, widgets);
    }

    public void Close(WindowContextBase context)
    {
        if (context == null)
        {
            return;
        }
        if (context.type == WindowType.Normal)
        {
            int index = mWindowStack.FindIndex((nav) => { return nav.windowStatus.context == context; });
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
                if (mWindowStack.FindIndex((nav) => { return nav.windowStatus.context == context; }) < 0)
                {
                    DestroyWindow(context);
                }
                else
                {
                    SetActive(context, false);
                }
                mTempList.Clear();
                if (current != null && current.hideWindowsStatus != null)
                {
                    for (int i = 0; i < current.hideWindowsStatus.Count; ++i)
                    {
                        SetActive(current.hideWindowsStatus[i].context, true, current.hideWindowsStatus[i].widgetsActive);
                        mTempList.Add(current.hideWindowsStatus[i].context.id);
                    }
                }

                if (previous != null)
                {
                    if (previous.windowStatus.context.hideOther == false && previousIndex < mWindowStack.Count)
                    {
                        var previousPrevious = mWindowStack[previousIndex];

                        if (mTempList.Contains(previousPrevious.windowStatus.context.id))
                        {
                            SetActive(previousPrevious.windowStatus.context, true, previousPrevious.windowStatus.widgetsActive);
                        }
                    }
                    if (mTempList.Contains(previous.windowStatus.context.id)==false)
                    {
                        SetActive(previous.windowStatus.context, true, previous.windowStatus.widgetsActive);
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
        if (Application.isPlaying)
        {
            Destroy(go);
        }
        else
        {
            DestroyImmediate(go);
        }

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
}
#region WindowDefEditor
#if UNITY_EDITOR 
public class WindowDefEditor : UnityEditor.EditorWindow
{

    [UnityEditor.MenuItem("GameObject/UI/Open")]
    private static void OpenDefineWindow()
    {
        var asset = Resources.Load<TextAsset>("UIDefine");

        GetWindow<WindowDefEditor>("UI定义列表");

        WindowManager.Instance.Initialize();
        WindowManager.Instance.SetLoader(LoadInEditor);
        WindowManager.Instance.Clear();
        WindowManager.Instance.Load(asset.text);
    }
    Vector2 mScroll;
    string mKey;
    private void OnGUI()
    {
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label(UnityEditor.EditorGUIUtility.TrTextContent("搜索"), GUILayout.Width(30), GUILayout.Height(30));
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
        int count = (int)(position.width / width);
        int i = 0;
        using (var it = WindowManager.Instance.contexts.GetEnumerator())
        {
            bool beginHorizontal = false;
            while (it.MoveNext())
            {
                if (i == 0)
                {
                    GUILayout.BeginHorizontal();
                    beginHorizontal = true;
                }
                WindowContextBase val = it.Current.Value;
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
                        bool open = WindowManager.Instance.GetObject(val);
                        GUIStyle style = GUI.skin.button;
                        style.normal.textColor = open ? Color.yellow : Color.white;

                        if (GUILayout.Button(UnityEditor.EditorGUIUtility.TrTextContent(val.name), style, GUILayout.Width(width), GUILayout.Height(30)))
                        {
                            OpenWindow(val);
                        }
                    }
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

        GUILayout.EndScrollView();
    }

    private void OpenWindow(WindowContextBase obj)
    {
        bool open = WindowManager.Instance.GetObject(obj) != null;

        if (!open)
        {
            WindowManager.Instance.Open(obj);
        }
        else
        {
            WindowManager.Instance.Close(obj);
        }
    }

    public static void LoadInEditor(string name, Action<GameObject> callback)
    {
        GameObject asset = Resources.Load<GameObject>(string.Format("UI/{0}", name));

        if (asset != null)
        {
            callback(asset);
        }
    }
}

#endif
#endregion