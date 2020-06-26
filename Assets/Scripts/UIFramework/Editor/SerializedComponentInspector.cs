using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

[ExecuteInEditMode]
[CustomEditor(typeof(SerializedComponent))]
public class SerializedComponentInspector : Editor
{
    private SerializedComponent mTarget;
    static List<string> types = new List<string>();
    static HashSet<string> names = new HashSet<string>();
    static List<string> enums = new List<string>();
    static List<Assembly> assemblies = new List<Assembly>();
    private void Awake()
    {
        mTarget = target as SerializedComponent;   
    }
    private void OnEnable()
    {
        enums.Clear();
        var attribute = typeof(SerializedEnumAttribute);
        var assembly = attribute.Assembly;
        var types = assembly.GetTypes();
        for (int j = 0; j < types.Length; ++j)
        {
            Type type = types[j];
            if (type.IsDefined(attribute, true))
            {
                enums.Add(type.FullName);
            }
        }
        if(assemblies.Count == 0)
        {
            assemblies.Add(typeof(GameObject).Assembly);
            assemblies.Add(typeof(UnityEngine.UI.Button).Assembly);
            assemblies.Add(typeof(SerializedComponent).Assembly);

        }
    }
  
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (mTarget == null)
        {
            return;
        }
        int errorIndex = -1;
        for (int i = 0; i < mTarget.fields.Count;)
        {
            EditorGUILayout.BeginHorizontal();
            var field = mTarget.fields[i];
            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(20));
            field.name = EditorGUILayout.TextField(field.name, GUILayout.Width(100));
            var propertyType = (SerializedFieldType)EditorGUILayout.EnumPopup(field.type, GUILayout.Width(80));
            if (propertyType != SerializedFieldType.None && propertyType != SerializedFieldType.Object)
            {
                if (string.IsNullOrEmpty(field.name))
                {
                    field.name = GetFieldName(propertyType.ToString());
                }
            }
            field.type = propertyType;
            switch (field.type)
            {
                case SerializedFieldType.Boolean: field.SetBool(EditorGUILayout.Toggle(field.GetBool())); break;
                case SerializedFieldType.Integer: field.SetInt(EditorGUILayout.IntField(field.GetInt())); break;
                case SerializedFieldType.Float: field.SetFloat(EditorGUILayout.FloatField(field.GetFloat())); break;
                case SerializedFieldType.String: field.SetString(EditorGUILayout.TextField(field.GetString())); break;
                case SerializedFieldType.Vector2: field.SetVector2(EditorGUILayout.Vector2Field("", field.GetVector2())); break;
                case SerializedFieldType.Vector3: field.SetVector3(EditorGUILayout.Vector3Field("", field.GetVector3())); break;
                case SerializedFieldType.Vector4: field.SetVector4(EditorGUILayout.Vector4Field("", field.GetVector4())); break;
                case SerializedFieldType.Object:
                    {
                        types.Clear();
                        var gameObjectType = typeof(GameObject).FullName;
                        var objectValue = field.GetObject();

                        if (objectValue != null)
                        {
                            if (string.IsNullOrEmpty(field.name))
                            {
                                field.name = GetFieldName(objectValue.name);
                            }

                            types.Add(gameObjectType);
                            Component[] components = null;
                            if (objectValue.GetType() == typeof(GameObject))
                            {
                                var go = objectValue as GameObject;
                                components = go.GetComponents<Component>();
                                if (field.GetTypeName() == gameObjectType)
                                {
                                    field.SetObject(go);
                                }
                            }
                            else
                            {
                                var component = objectValue as Component;
                                if (component != null)
                                {
                                    components = component.GetComponents<Component>();
                                    if (field.GetTypeName() == gameObjectType)
                                    {
                                        field.SetObject( component.gameObject);
                                    }
                                }
                            }
                            if (components != null)
                            {
                                for (int j = 0; j < components.Length; ++j)
                                {
                                    var componentType = components[j].GetType().FullName;

                                    types.Add(componentType);
                                    if (field.GetTypeName() == componentType)
                                    {
                                        field.SetObject(components[j]);
                                    }

                                }
                            }
                        }
                        else
                        {
                            field.SetTypeName( SerializedFieldType.Object, gameObjectType);
                        }
                        if (string.IsNullOrEmpty(field.GetTypeName()))
                        {
                            field.SetTypeName(SerializedFieldType.Object,gameObjectType);
                        }
                        var type = GetType(field.GetTypeName());
                        field.SetObject( EditorGUILayout.ObjectField(field.objectValue, type, true));

                        int index = types.FindIndex(x => x == field.GetTypeName());
                        if (index >= 0)
                        {
                            index = EditorGUILayout.Popup(index, types.ToArray());

                            field.SetTypeName( SerializedFieldType.Object, types[index]);
                        }
                    }
                    break;
                case SerializedFieldType.Enum:
                    {

                        int index = enums.FindIndex(x => x == field.GetTypeName());
                        if (index == -1) index = 0;
                        if (enums.Count > 0)
                        {
                            int k = EditorGUILayout.Popup(index, enums.ToArray());
                            int value = field.GetEnum();
                            if (k != index)
                            {
                                value = 0;
                            }
                            index = k;
                            field.SetTypeName(SerializedFieldType.Enum, enums[index]);
                            field.SetEnum( value);
                        }
                        if (string.IsNullOrEmpty(field.GetTypeName()) == false)
                        {
                            var type = typeof(SerializedField).Assembly.GetType(field.GetTypeName());

                            Enum value = (Enum)Enum.ToObject(type, field.GetEnum());
                            field.SetTypeName(SerializedFieldType.Enum, field.GetTypeName());
                            field.SetEnum( Convert.ToInt32(EditorGUILayout.EnumPopup(value)));
                        }

                    }
                    break;
            }
            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                mTarget.fields.RemoveAt(i);
                continue;
            }
            if (i > 0)
            {
                if (GUILayout.Button("∧", GUILayout.Width(20)))
                {
                    var prev = mTarget.fields[i - 1];
                    mTarget.fields[i - 1] = mTarget.fields[i];
                    mTarget.fields[i] = prev;
                }
            }
            if (i < mTarget.fields.Count - 1)
            {
                if (GUILayout.Button("∨", GUILayout.Width(20)))
                {
                    var next = mTarget.fields[i + 1];
                    mTarget.fields[i + 1] = mTarget.fields[i];
                    mTarget.fields[i] = next;
                }
            }
            if (errorIndex == -1)
            {
                if (string.IsNullOrEmpty(field.name))
                {
                    errorIndex = i;
                }
            }
            EditorGUILayout.EndHorizontal();

            ++i;
        }
      
        bool error = false;

        if (errorIndex >= 0)
        {
            error = true;
            EditorGUILayout.HelpBox("index:"+errorIndex+"命名不能为空", MessageType.Error);
        }
        string errorName = GetRepeatName();
        if (string.IsNullOrEmpty(errorName) == false)
        {
            error = true;
            EditorGUILayout.HelpBox("有重复的命名:"+errorName, MessageType.Error);
        }
        if(error==false)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                mTarget.fields.Add(new SerializedField());
            }
            if (GUILayout.Button("Save"))
            {
                Save();
            }

            EditorGUILayout.EndHorizontal();
        }

    }

    private void Save()
    {
        GameObject go = PrefabUtility.GetNearestPrefabInstanceRoot(mTarget.gameObject);
        if (go != null)
        {
            string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
            if (string.IsNullOrEmpty(assetPath) == false)
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(go, assetPath, InteractionMode.AutomatedAction);
            }
        }
        else
        {
            GameObject prefab = mTarget.transform.root.gameObject;
            string assetPath = AssetDatabase.GetAssetPath(prefab);

            if (string.IsNullOrEmpty(assetPath) == false)
            {
                go = Instantiate(prefab);
                PrefabUtility.SaveAsPrefabAssetAndConnect(go, assetPath, InteractionMode.AutomatedAction);

                DestroyImmediate(go);
            }
            else
            {
                if (AssetDatabase.IsOpenForEdit(prefab))
                {
                    EditorUtility.SetDirty(prefab);
                    AssetDatabase.SaveAssets();
                }

            }
        }
    }

    string GetFieldName(string type)
    {
        string fieldName = type.ToString();
        int i = 1;
        while(true)
        {
            if(mTarget.fields.Find(x=>x.name== fieldName) == null)
            {
                break;
            }
            else
            {
                fieldName = type + i.ToString();
                i++;
            }
        }

        return fieldName;
    }
    string GetRepeatName()
    {
        names.Clear();
        for(int i = 0; i < mTarget.fields.Count; ++i)
        {
            var field = mTarget.fields[i];

            if (names.Contains(field.name))
            {
                return field.name;
            }
            else
            {
                names.Add(field.name);
            }
        }
        return null;
    }

    private static Type GetType(string name)
    {
        for(int i = 0; i <assemblies.Count;++i)
        {
            var type = assemblies[i].GetType(name);
            if(type!=null)
            {
                return type;
            }
        }
        return null;
    }
}

