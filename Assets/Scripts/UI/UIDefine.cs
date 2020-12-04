
using System;
using UnityEngine;

public static class UIDefine
{
    public static WidgetContext UIBg                = new WidgetContext("UIBg", typeof(UIBg));
    public static WidgetContext UIFixed             = new WidgetContext("UIFixed", typeof(UIFixed));
    public static WidgetContext UIRight             = new WidgetContext("UIRight", typeof(UIRight));

    public static WindowContext UIMain              = new WindowContext("UIMain", typeof(UIMain), true, 0, true, UIFixed, UIRight);
    public static WindowContext UINormal            = new WindowContext("UINormal", typeof(UINormal), true, 0, true, new WidgetContext(UIBg, -1), UIFixed);
    public static WindowContext UIPop               = new WindowContext("UIPop",typeof(UIPop));
    public static WindowContext UIWidget            = new WindowContext("UIWidget", typeof(UIWidget), false, 1000);
    public static WindowContext UISerialized        = new WindowContext("UISerialized", typeof(UISerialized), false, 0, true, new WidgetContext(UIBg, -1));







#if UNITY_EDITOR
    [UnityEditor.MenuItem("GameObject/Open UI/UIMain")]
    public static void OpenUIMain()
    {
        OpenWindow(UIMain);
    }
    [UnityEditor.MenuItem("GameObject/Open UI/UINormal")]
    public static void OpenUINormal()
    {
        OpenWindow(UINormal);
    }
    [UnityEditor.MenuItem("GameObject/Open UI/UIPop")]
    public static void OpenUIPop()
    {
        OpenWindow(UIPop);
    }
    [UnityEditor.MenuItem("GameObject/Open UI/UIWidget")]
    public static void OpenUIWidget()
    {
        OpenWindow(UIWidget);
    }
    [UnityEditor.MenuItem("GameObject/Open UI/UISerialized")]
    public static void OpenUISerialized()
    {
        OpenWindow(UISerialized);
    }

    public static void OpenWindow(WindowContextBase context)
    {
        if(context== null || context.GetType().IsAbstract)
        {
            return;
        }
        WindowManager.Instance.Initialize();
        WindowManager.Instance.SetLoader(LoadInEditor);
        WindowManager.Instance.Open(context);
    }

    public static void LoadInEditor(string name, Action<GameObject> callback)
    {
        GameObject asset = Resources.Load<GameObject>(string.Format("UI/{0}", name));

        if (asset != null)
        {
            callback(asset);
        }
    }
#endif

}
