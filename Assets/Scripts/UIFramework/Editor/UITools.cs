using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
 * 
 * 根据UI的prefab中的物件命名自动生成初始化代码
 * 命名规则 name@type.variableName
 * 解释：物体名称（可有可无）@要获取该物体上的对象类型.自动生成代码的变量名
 * 如：Icon@sprite.mItemIcon
 * 生成初始化代码 UISprite mItemIcon = transform.Find("XXXX/Icon").GetComponent<UISprite>();
 * XXXX是该物体在prefab中的路径
*/

public class Variable
{
    public readonly string Name;
    public readonly string Type;
    public readonly string Path;

    public Variable(string varName, string varType, string varPath)
    {
        Name = varName;

        Path = varPath;
        if (varType.Contains("."))
        {
            Type = varType.Substring(varType.LastIndexOf('.')+1);
        }
        else
        {
            Type = varType;
        }
    }
}

public static class UITools
{
    static string lua = @"local M = {{
    mName = '{classname}'
}}
{classname} = M
setmetatable(M, UIBase)

--整个生命周期只调用一次
function M:OnLoad()
--BINDING_CODE_BEGIN
{binding}--BINDING_CODE_END
end

--整个生命周期只调用一次
function M:OnUnload()
--UNBINDING_CODE_BEGIN
{unbinding}--UNBINDING_CODE_END
end


--整个生命周期可能调用多次
function M:OnShow()


end

--整个生命周期可能调用多次
function M:OnHide()


end

return M";

    static string cshape = @"using UnityEngine;
using UnityEngine.UI;

public class {classname} : Window
{
    public {classname}()
    {
    }
//BINDING_DEFINITION_BEGIN
{definition}//BINDING_DEFINITION_END
   
    private void Awake()
    {
//BINDING_CODE_BEGIN
{binding}//BINDING_CODE_END
    } 
}";

    [MenuItem("Assets/Generate UI Lua Code", true)]
    [MenuItem("Assets/Generate UI CS Code", true)]
    public static bool IsPrefab()
    {
        return (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(GameObject));
    }


    [MenuItem("Assets/Generate UI Lua Code")]
    public static void GenerateLuaCode()
    {
        GameObject ui = Selection.activeGameObject;
        if (!ui)
        {
            Debug.Log("请选择一个UI Prefab");
            return;
        }

        Dictionary<string, Variable> variableDir = ParseUIPrefab(ui);

        if (variableDir == null)
            return;

        StringBuilder bindCode = new StringBuilder();
        StringBuilder unBindCode = new StringBuilder();
        foreach (Variable variable in variableDir.Values)
        {
            bindCode.Append(string.Format("\tself.{0} = self.gameObject.transform:Find('{1}'):GetComponent(typeof({2}))\n", variable.Name, variable.Path, variable.Type));
            unBindCode.Append(string.Format("\tself.{0} = nil\n", variable.Name));
        }


        string fullpath = GetLuaFileName(ui);

        string code = "";
        if (File.Exists(fullpath))
        {
            string content = File.ReadAllText(fullpath);

            int startIndex = content.IndexOf("--BINDING_CODE_BEGIN");
            int endIndex = content.IndexOf("--BINDING_CODE_END");
            string part1 = content.Substring(0, startIndex + "--BINDING_CODE_BEGIN".Length + 1);
            string part2 = content.Substring(endIndex);
            content = part1 + bindCode.ToString() + part2;

            startIndex = content.IndexOf("--UNBINDING_CODE_BEGIN");
            endIndex = content.IndexOf("--UNBINDING_CODE_END");
            part1 = content.Substring(0, startIndex + "--UNBINDING_CODE_BEGIN".Length + 1);
            part2 = content.Substring(endIndex);

            code = part1 + unBindCode.ToString() + part2;

            File.Delete(fullpath);
        }
        else
        {
            code = lua.Replace("{classname}", ui.name)
                    .Replace("{binding}", bindCode.ToString())
                    .Replace("{unbinding}", unBindCode.ToString());
        }


        try
        {  
            FileStream fs = new FileStream(fullpath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(code);
            sw.Close();
        }
        catch (System.Exception e)
        {
            throw e;
        }

        Debug.Log("Done!!");
        AssetDatabase.Refresh();
    }


    [MenuItem("Assets/Generate UI CS Code")]
    public static void GenerateCSCode()
    {
        GameObject ui = Selection.activeGameObject;
        if (!ui)
        {
            Debug.Log("请选择一个UI Prefab");
            return;
        }

        Dictionary<string, Variable> variableDir = ParseUIPrefab(ui);

        if (variableDir == null)
            return;

        StringBuilder definitionCode = new StringBuilder();
        StringBuilder bindCode = new StringBuilder();
        foreach (Variable variable in variableDir.Values)
        {
            definitionCode.AppendFormat("\tprivate {0} {1};\n", variable.Type, variable.Name );
            bindCode.AppendFormat("\t\t{0} = transform.Find(\"{1}\").GetComponent<{2}>();\n", variable.Name, variable.Path, variable.Type);
        }

        string fullpath = GetCSFileName(ui);

        string code = "";
        if (File.Exists(fullpath))
        {
            string content = File.ReadAllText(fullpath);

            int startIndex = content.IndexOf("//BINDING_DEFINITION_BEGIN");
            int endIndex = content.IndexOf("//BINDING_DEFINITION_END");
            string part1 = content.Substring(0, startIndex + "//BINDING_DEFINITION_BEGIN".Length + 1);
            string part2 = content.Substring(endIndex);
            content = part1 + definitionCode.ToString() + part2;

            startIndex = content.IndexOf("//BINDING_CODE_BEGIN");
            endIndex = content.IndexOf("//BINDING_CODE_END");
            part1 = content.Substring(0, startIndex + "//BINDING_CODE_BEGIN".Length + 1);
            part2 = content.Substring(endIndex);

            code = part1 + bindCode.ToString() + part2;

            File.Delete(fullpath);
        }
        else
        {
            code = cshape.Replace("{classname}", ui.name)
                    .Replace("{definition}", definitionCode.ToString())
                    .Replace("{binding}", bindCode.ToString());
        }


        try
        {
            FileStream fs = new FileStream(fullpath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(code);
            sw.Close();
        }
        catch (System.Exception e)
        {
            throw e;
        }

        Debug.Log("Done!!");
        AssetDatabase.Refresh();
    }


    static Dictionary<string, Variable> ParseUIPrefab(GameObject ui)
    {
        Dictionary<string, Variable> variableDir = new Dictionary<string, Variable>();

        if (ui)
        {
            Transform[] childs = ui.GetComponentsInChildren<Transform>(true);

            foreach (var t in childs)
            {
                Transform child = t;
                if (child == ui.transform)
                    continue;

                string name = child.name;
                if (!name.Contains("@"))
                    continue;

                int index = name.IndexOf('@');
                if (index >= name.Length - 1)
                    continue;

                string variableNameAndType = name.Substring(index + 1);

                string variableName, type, path;

                if (variableNameAndType.Contains("."))
                {
                    string[] nameAndTypes = variableNameAndType.Split('.');
                    if (nameAndTypes.Length != 2)
                    {
                        variableDir.Clear();
                        Debug.LogError(string.Format("命名错误：{0}", name));
                        return variableDir;
                    }

                    type = nameAndTypes[0];
                    variableName = nameAndTypes[1];

                }
                else
                {
                    type = "Transform";
                    variableName = variableNameAndType;
                }

                System.Type variableType = GetType(type);
                if (variableType == null)
                {
                    variableDir.Clear();
                    Debug.LogError(string.Format("命名错误,没定义该类型：{0}", name));
                    return variableDir;
                }

                if (!child.GetComponent(variableType))
                {
                    variableDir.Clear();
                    Debug.LogError(string.Format("给定的物体{0}没有{1}类型的组件", name, variableType));
                    return variableDir;
                }

                path = name;

                while (child.parent && child.parent != ui.transform)
                {
                    path = string.Format("{0}/{1}", child.parent.name, path);
                    child = child.parent;
                }

                if (!ui.transform.Find(path))
                {
                    Debug.LogError(string.Format("根据路径 path = {0}未能找到物体！", path));
                    variableDir.Clear();

                    return variableDir;
                }

                if (!ui.transform.Find(path).GetComponent(variableType))
                {
                    Debug.LogError(string.Format("根据路径 path = {0}未能找到物体上的{1}组件！", path, variableType.ToString()));
                    variableDir.Clear();
                    return variableDir;
                }

                if (variableDir.ContainsKey(variableName))
                {
                    Debug.LogError(string.Format("重复变量名：{0}, path = {1}", variableName, path));
                    variableDir.Clear();
                    return variableDir;
                }
                else
                {
                    if (!string.IsNullOrEmpty(variableName) && !string.IsNullOrEmpty(variableType.ToString()) && !string.IsNullOrEmpty(path))
                    {
                        variableDir.Add(variableName, new Variable(variableName, variableType.ToString(), path));
                    }
                    else
                    {
                        variableDir.Clear();
                        Debug.LogError(string.Format("命名错误：{0}", name));
                        return variableDir;
                    }
                }
            }
        }

        return variableDir;
    }


    static System.Type GetType(string typeName)
    {
        switch (typeName)
        {
            case "Gameobject": return typeof(RectTransform);
            case "Transform": return typeof(RectTransform);
            case "Canvas": return typeof(Canvas);
            case "Text": return typeof(Text);
            case "Image": return typeof(Image);
            case "Button": return typeof(Button);
            case "Toggle": return typeof(Toggle);
            case "ToggleGroup": return typeof(ToggleGroup);
            case "RawImage": return typeof(RawImage);
            case "Slider": return typeof(Slider);
            case "Scrollbar": return typeof(Scrollbar);
            case "Dropdown": return typeof(Dropdown);
            case "InputField": return typeof(InputField);
            case "ScrollRect": return typeof(ScrollRect);
            case "VerticalScrollView": return typeof(VerticalScrollView);
            case "HorizontalScrollView": return typeof(HorizontalScrollView);
            case "Tab":return typeof(Tab);
            case "ProgressBar": return typeof(ProgressBar);
            default:
                {         
                    return null;
                }
        }
    }

    static string GetLuaFileName(GameObject ui)
    {
        string dir = Application.dataPath + "/Resources/Lua/UI/";

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        return dir + ui.name + ".lua.txt";
    }
    static string GetCSFileName(GameObject ui)
    {
        string dir = Application.dataPath + "/Scripts/UI/";

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        return dir + ui.name + ".cs";
    }
}