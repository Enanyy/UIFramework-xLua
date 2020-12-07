
using System;
using System.Reflection;
using UnityEngine;

public static class UIDefine
{
    public static WidgetContext UIBg          = new WidgetContext("UIBg", typeof(UIBg));
    public static WidgetContext UIFixed       = new WidgetContext("UIFixed", typeof(UIFixed));
    public static WidgetContext UIRight       = new WidgetContext("UIRight", typeof(UIRight));

    public static WindowContext UIMain        = new WindowContext("UIMain", typeof(UIMain), true, 0, true, new WidgetContext(UIBg, -1), UIFixed, UIRight);
    public static WindowContext UINormal      = new WindowContext("UINormal", typeof(UINormal), false, 0, true, new WidgetContext(UIBg, -1), UIFixed);
    public static WindowContext UIPop               = new WindowContext("UIPop",typeof(UIPop));
    public static WindowContext UIWidget            = new WindowContext("UIWidget", typeof(UIWidget), false, 1000);
    public static WindowContext UISerialized        = new WindowContext("UISerialized", typeof(UISerialized), false, 0, true, new WidgetContext(UIBg, -1));

}

#if UNITY_EDITOR

public class UIDefineWindow : UnityEditor.EditorWindow
{

    [UnityEditor.MenuItem("GameObject/UI/Open")]
    private static void OpenDefineWindow()
    {
        GetWindow<UIDefineWindow>("UIDefineWindow");

        WindowManager.Instance.Initialize();
        WindowManager.Instance.SetLoader(LoadInEditor);
    }
    Vector2 mScroll;
    private void OnGUI()
    {
        mScroll = GUILayout.BeginScrollView(mScroll,false, true);
        float width = 200f;
        int count = (int)(position.width / width);
        Type type = typeof(UIDefine);

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

        for (int i = 0; i < fields.Length; i+= count)
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j < count; j++)
            {
                int index = i + j;
                if (index < fields.Length)
                {
                    var val = fields[index].GetValue(null) as WindowContextBase;
                    if (val != null)
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
            }
            GUILayout.EndHorizontal();
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

