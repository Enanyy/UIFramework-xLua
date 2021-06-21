#define ENABLE_WINDOW_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using System.Reflection;

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

    public readonly WindowDefine mWindowDefine = new WindowDefine();

    private Dictionary<ulong, WindowObject> mWindowObjectDic = new Dictionary<ulong, WindowObject>();

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
        mWindowDefine.Load(xml);
    }
    

   

    public void Open(string name, Action<WindowObject> callback = null, params string[] widgets)
    {
        WindowContextBase context = mWindowDefine.GetWindowContextBase(name);
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
                widgetContexts[i] = mWindowDefine.GetWidgetContext(widgets[i]);
            }
        }
        Open(context, callback, widgetContexts);
    }
    public void Close(string name)
    {
        WindowContextBase context = mWindowDefine.GetWindowContextBase(name);
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
                contexts[i] = mWindowDefine.GetWindowContext(excepts[i]);
            }
        }
        CloseAll(contexts);
    }

    public void SetWidgetsActive(string context, bool active)
    {
        WindowContext windowContext = mWindowDefine.GetWindowContext(context);
        if (windowContext == null || mWindowObjectDic.ContainsKey(windowContext.id) == false)
        {
            return;
        }
        SetWidgetsActive(windowContext, active);
    }

    public void SetWidgetActive(string context, string widget, bool active)
    {
        WindowContext windowContext = mWindowDefine.GetWindowContext(context);
        if (windowContext == null || mWindowObjectDic.ContainsKey(windowContext.id) == false)
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
        mInitialized = false;
        mWindowDefine.Clear();
#if ENABLE_WINDOW_EDITOR
        mMenuList.Clear();
        mMenuList = null;
#endif
    }


    public void Open(WindowContextBase context, Action<WindowObject> callback = null, params WidgetContext[] widgets)
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

        var windowObject = GetObject(context);
        if(windowObject == null)
        {
            windowObject = new WindowObject(context);
            mWindowObjectDic.Add(context.id, windowObject);
        }
        if (windowObject.gameObject == null)
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
                    mWindowObjectDic.Remove(context.id);
                    context.Clear();
                    return;
                }


                context.status = WindowStatus.Done;

                windowObject.Destroy();
                
                GameObject go = null;
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
#endif
                    go = Instantiate(asset) as GameObject;
#if UNITY_EDITOR
                }
                else
                {
                     go = UnityEditor.PrefabUtility.InstantiatePrefab(asset) as GameObject;
                }
#endif

                go.transform.SetParent(transform);
                windowObject.Init(go, mCamera);

                Push(context as WindowContext);

                SetActive(context, true);

                callback?.Invoke(windowObject);


            });
        }
        else
        {
            context.status = WindowStatus.Done;

            Push(context as WindowContext);

            SetActive(context, true);
            SetTouch(true);

            callback?.Invoke(windowObject);
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
                using (var it = mWindowObjectDic.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var w = it.Current.Value.contextbase as WindowContext;
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



    private int SortWindow(WindowState a, WindowState b)
    {
        var windowObjectA = GetObject(a.context);
        var windowObjectB = GetObject(b.context);
   
        if (windowObjectA == null && windowObjectB == null)
        {
            return 0;
        }
        else if (windowObjectA != null && windowObjectB == null)
        {
            return 1;
        }
        else if (windowObjectA == null && windowObjectB != null)
        {
            return -1;
        }
        else
        {
            if (windowObjectA.sortingOrder > windowObjectB.sortingOrder)
            {
                return -1;
            }
            else if (windowObjectA.sortingOrder < windowObjectB.sortingOrder)
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
        var windowObject = GetObject(context);
        if(windowObject == null)
        {
            return;
        }

        if (active)
        {
            SetOrder(context);
        }
        else
        {
            windowObject.sortingOrder = 0;
        }

        if (context.active != active)
        {
            context.active = active;
            windowObject.active = active;
           
            windowObject.SetComponentActive(active);
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
                    windowObject.SetParent(parent);

                    parent.OnWidgetActive(widget);
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
        return context.active && mWindowObjectDic.ContainsKey(context.id);
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

        var windowObject = GetObject(widget);
        if (windowObject != null)
        {
            SetActive(widget, active);
        }
        else
        {
            Open(widget);
        }
    }

    public int GetMaxOrder()
    {
        int maxOrder = int.MinValue;
        using (var it = mWindowObjectDic.GetEnumerator())
        {
            while (it.MoveNext())
            {
                var v = it.Current.Value;

                var context = v.contextbase as WindowContext;
                if (context != null)
                {
                    if (context.fixedOrder == 0)
                    {
                        if (v.sortingOrder > maxOrder)
                        {
                            maxOrder = v.sortingOrder;
                        }
                    }
                }
            }
        }
        return maxOrder;
    }

    private void SetOrder(WindowContextBase context)
    {
        if (context == null) return;

        var windowObject = GetObject(context);
        if (windowObject == null)
        {
            return;
        }
        var widget = context as WidgetContext;
        if (widget != null)
        {
            var parent = GetObject(widget.parent);
            if (parent != null)
            {
                windowObject.sortingOrder = parent.sortingOrder + widget.sortingOrderOffset;
            }
        }

        else
        {
            var window = context as WindowContext;
            //固定层级
            if (window != null && window.fixedOrder != 0)
            {
                windowObject.sortingOrder = window.fixedOrder;
            }
            else
            {
                int maxOrder = GetMaxOrder();
                if (maxOrder == int.MinValue)
                {
                    maxOrder = 0;
                }
                else
                {
                    maxOrder += mOrderAddition;
                }
                windowObject.sortingOrder = maxOrder;
            }
        }
    }

    private WindowContextBase Get(ulong id)
    {
        mWindowObjectDic.TryGetValue(id, out WindowObject obj);
        if(obj!=null)
        {
            return obj.contextbase;
        }
        return null;
    }

    public WindowObject GetObject(WindowContextBase context)
    {
        if (context == null)
        {
            return null;
        }
        return GetObject(context.id);
    }

    public WindowObject GetObject(ulong id)
    {
        mWindowObjectDic.TryGetValue(id, out WindowObject windowObject);
        return windowObject;
    }

    public void CloseAll(bool destroy = true)
    {
        mTempList.Clear();
        mTempList.AddRange(mWindowObjectDic.Keys);
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
        using (var it = mWindowObjectDic.GetEnumerator())
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

            var w = Get(key);
            if (w != null)
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
            int index = mWindowStack.FindIndex((nav) => { return nav.windowState.context.id == context.id; });
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
                if (mWindowStack.FindIndex((nav) => { return nav.windowState.context.id == context.id; }) < 0)
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
            WindowObject windowObject = GetObject(context);

            if (windowObject != null)
            {
                windowObject.Destroy();
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

    private void Update()
    {
        mTempList.Clear();
        mTempList.AddRange(mWindowObjectDic.Keys);
        for (int i = 0; i < mTempList.Count; ++i)
        {
            var windowObject = GetObject(mTempList[i]);
            if(windowObject!= null && windowObject.active)
            {
                windowObject.Update();
            }
        }
    }
    private void LateUpdate()
    {
        mTempList.Clear();
        mTempList.AddRange(mWindowObjectDic.Keys);
        for (int i = 0; i < mTempList.Count; ++i)
        {
            var windowObject = GetObject(mTempList[i]);
            if (windowObject != null && windowObject.active)
            {
                windowObject.LateUpdate();
            }
        }
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

        DrawItems(count,mWindowDefine.contexts.GetEnumerator(), (current) =>
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
                    bool open = GetObject(val) != null;
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
    private Dictionary<string, WindowContextBase> contexts => WindowManager.Instance.mWindowDefine.contexts;

    [UnityEditor.MenuItem("GameObject/UI/Open")]
    private static void OpenDefineWindow()
    {
        var editor = GetWindow<WindowDefEditor>(WindowManager.WindowTitle);
        editor.Initialize();
    }

    private void Initialize()
    {
        var instance = WindowManager.Instance;
        if (instance.initialized == false || contexts.Count == 0)
        {

            instance.Initialize();
            instance.SetLoader(LoadInEditor);
            if(contexts.Count == 0)
            {
                var asset = Resources.Load<TextAsset>("UIDefine");
                instance.Load(asset.text);
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