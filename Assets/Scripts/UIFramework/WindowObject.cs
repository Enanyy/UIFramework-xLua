using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowObject
{
    public readonly WindowContextBase contextbase;

    public GameObject gameObject { get; private set; }
    public Canvas canvas { get; private set; }
    public CanvasScaler canvasScaler { get; private set; }

    public bool active
    {
        get { return canvas != null && canvas.enabled; }
        set
        {
            if (canvas != null)
            {
                canvas.enabled = value;
            }
        }
    }

    public int sortingOrder
    {
        get
        {
            if (canvas != null)
            {
                return canvas.sortingOrder;
            }
            return 0;
        }
        set
        {
            if (canvas != null)
            {
                canvas.sortingOrder = value;
            }
        }
    }

    private List<WindowComponent> components = new List<WindowComponent>();
    private List<IComponentUpdate> updateComponents;
    private List<IComponentLateUpdate> lateUpdateComponents;
    private List<IComponentFixedUpdate> fixedUpdateComponents;

    public static readonly Vector2 DesignResolution = new Vector2(1920, 1080);

    private bool initialize = false;

    public WindowObject(WindowContextBase windowContextBase)
    {
        Debug.Assert(windowContextBase != null, "windowContextBase is NULL!!!");
        contextbase = windowContextBase;
    }
    public void Init(GameObject go, Camera camera)
    {
        Debug.Assert(go != null, "go is NULL!!!");
        Debug.Assert(initialize == false, "initialize !=false!!!");

        gameObject = go;

        go.name = System.IO.Path.GetFileNameWithoutExtension(contextbase.path);

        go.TryGetComponent(out Canvas c);
        if (c == null) c = go.AddComponent<Canvas>();
        canvas = c;

        go.TryGetComponent(out CanvasScaler scaler);
        bool isScale = contextbase.type == WindowType.Normal;
        if (isScale == false)
        {
            var widget = contextbase as WidgetContext;
            if (widget.parent == null)
            {
                isScale = true;
            }
        }

        if (isScale)
        {
            if (scaler == null) scaler = go.AddComponent<CanvasScaler>();

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.sortingLayerName = "UI";
            canvas.overrideSorting = false;
            canvas.worldCamera = camera;

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
                Object.DestroyImmediate(scaler);
            }
        }
        canvasScaler = scaler;
        go.SetActive(true);

        InitComponent();

        initialize = true;
    }

    // Update is called once per frame
    public void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false)
        {
            return;
        }
#endif
        if (updateComponents == null)
        {
            return;
        }
        for (int i = 0; i < updateComponents.Count;)
        {
            var component = updateComponents[i];
            if (component == null)
            {
                continue;
            }
            component.OnUpdate();
            ++i;
        }
    }

    public void LateUpdate()
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false)
        {
            return;
        }
#endif
        if (lateUpdateComponents == null)
        {
            return;
        }
        for (int i = 0; i < lateUpdateComponents.Count;)
        {
            var component = lateUpdateComponents[i];
            if (component == null)
            {
                continue;
            }
            component.OnLateUpdate();
            ++i;
        }
    }

    public void FixedUpdate()
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false)
        {
            return;
        }
#endif
        if (fixedUpdateComponents == null)
        {
            return;
        }
        for (int i = 0; i < fixedUpdateComponents.Count;)
        {
            var component = fixedUpdateComponents[i];
            if (component == null)
            {
                continue;
            }
            component.OnFixedUpdate();
            ++i;
        }
    }

    private void InitComponent()
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false)
        {
            return;
        }
#endif
        if (contextbase == null || gameObject == null)
        {
            return;
        }
        if (contextbase.components != null)
        {
            using (var it = contextbase.components.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var type = it.Current.Key;

                    WindowComponent component = gameObject.GetComponent(type) as WindowComponent;
                    if (component == null)
                    {
                        component = gameObject.AddComponent(type) as WindowComponent;
                    }
                }
            }
        }
        RegisterComponent(gameObject);
    }

    public void RegisterComponent(GameObject go)
    {
        if (go == null || contextbase == null)
        {
            return;
        }
        var windowComponents = go.GetComponentsInChildren<WindowComponent>();
        for (int i = 0; i < windowComponents.Length; ++i)
        {
            var component = windowComponents[i];

            component.windowObject = this;
            var updateComponent = component as IComponentUpdate;
            if (updateComponent != null)
            {
                if (updateComponents == null)
                {
                    updateComponents = new List<IComponentUpdate>();
                }
                updateComponents.Add(updateComponent);
            }
            var lateUpdateComponent = component as IComponentLateUpdate;
            if (lateUpdateComponent != null)
            {
                if (lateUpdateComponents == null)
                {
                    lateUpdateComponents = new List<IComponentLateUpdate>();
                }
                lateUpdateComponents.Add(lateUpdateComponent);
            }
            var fixedUpdateComponent = component as IComponentFixedUpdate;
            if (fixedUpdateComponents != null)
            {
                if (fixedUpdateComponents == null)
                {
                    fixedUpdateComponents = new List<IComponentFixedUpdate>();
                }
                fixedUpdateComponents.Add(fixedUpdateComponent);
            }
        }
        var active = contextbase.active;
        for (int i = 0; i < windowComponents.Length; ++i)
        {
            var component = windowComponents[i];
            component.OnInit();
            if (initialize)
            {
                if (active)
                {
                    component.OnShow();
                }
                else
                {
                    component.OnHide();
                }
            }
        }
        components.AddRange(windowComponents);
    }

    public void SetComponentActive(bool active)
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false)
        {
            return;
        }
#endif
        for (int i = 0; i < components.Count;)
        {
            var component = components[i];
            if (component == null)
            {
                continue;
            }
            if (active)
            {
                component.OnShow();
            }
            else
            {
                component.OnHide();
            }
            ++i;
        }
    }

    public void OnWidgetActive(WidgetContext widget)
    {
        if (widget == null)
        {
            return;
        }
#if UNITY_EDITOR
        if (Application.isPlaying == false)
        {
            return;
        }
#endif
        bool active = widget.active;
        for (int i = 0; i < components.Count;)
        {
            var component = components[i];
            if (component == null)
            {
                continue;
            }
            if (contextbase.id == widget.id)
            {
                continue;
            }
            if (active)
            {
                component.OnWidgetShow(widget);
            }
            else
            {
                component.OnWidgetHide(widget);
            }
            ++i;
        }
    }

    public void SetParent(WindowObject parent)
    {
        if (parent == null || parent.gameObject == null)
        {
            return;
        }
        if (contextbase == null)
        {
            return;
        }
        var widget = contextbase as WidgetContext;
        if (widget == null)
        {
            return;
        }
        if (gameObject == null)
        {
            return;
        }

        Transform bindNode = parent.gameObject.transform;
        if (!string.IsNullOrEmpty(widget.bindNode))
        {
            bindNode = parent.gameObject.transform.Find(widget.bindNode);
        }

        if (bindNode != gameObject.transform.parent)
        {
            gameObject.transform.SetParent(bindNode);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.localPosition = Vector3.zero;

            RectTransform rect = gameObject.transform as RectTransform;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            if (canvas != null)
            {
                canvas.overrideSorting = true;
            }
        }
    }

    public void Destroy()
    {
        if (gameObject != null)
        {
            Object.DestroyImmediate(gameObject);
        }
        gameObject = null;
        canvas = null;
        components.Clear();
        if (updateComponents != null)
        {
            updateComponents.Clear();
        }
        if (lateUpdateComponents != null)
        {
            lateUpdateComponents.Clear();
        }
        if (fixedUpdateComponents != null)
        {
            fixedUpdateComponents.Clear();
        }
        initialize = false;
    }
}

