using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[CustomEditor(typeof(SerializedField))]
public class SerializedFieldInspector : Editor
{
    private SerializedField mTarget;
    static List<string> types = new List<string>();
    static HashSet<string> names = new HashSet<string>();
    static List<string> enums = new List<string>();
    private void Awake()
    {
        mTarget = target as SerializedField;   
    }
    private void OnEnable()
    {
        enums.Clear();
        var attribute = typeof(PropertyEnumAttribute);
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
    }
    public override void OnInspectorGUI()
    {
        if(mTarget== null)
        {
            return;
        }
        int errorIndex = -1;
        for(int i = 0; i < mTarget.fields.Count;)
        {
            EditorGUILayout.BeginHorizontal();
            var field = mTarget.fields[i];
            field.name = EditorGUILayout.TextField(field.name);
            var propertyType = (PropertyType)EditorGUILayout.EnumPopup(field.type);
            if (propertyType!= PropertyType.None && propertyType!= PropertyType.Object)
            {
                if(string.IsNullOrEmpty(field.name))
                {
                    field.name = GetFieldName(propertyType.ToString());
                }
            }
            field.type = propertyType;
            switch(field.type)
            {
                case PropertyType.Boolean:field.boolValue = EditorGUILayout.Toggle(field.boolValue);break;
                case PropertyType.Integer:field.intValue = EditorGUILayout.IntField(field.intValue);break;
                case PropertyType.Float: field.floatValue = EditorGUILayout.FloatField(field.floatValue); break;
                case PropertyType.String: field.stringValue = EditorGUILayout.TextField(field.stringValue); break;
                case PropertyType.Vector2: field.vector2Value = EditorGUILayout.Vector2Field("", field.vector2Value); break;
                case PropertyType.Vector3: field.vector3Value = EditorGUILayout.Vector3Field("", field.vector3Value); break;
                case PropertyType.Vector4: field.vector4Value = EditorGUILayout.Vector4Field("", field.vector4Value); break;
                case PropertyType.Object:
                    {
                        if(field.objectValue== null)
                        {
                            field.objectValue = new ObjectField();
                        }
                        types.Clear();
                        var gameObjectType = typeof(GameObject).FullName;
                        if (field.objectValue.obj != null)
                        {
                            if(string.IsNullOrEmpty(field.name))
                            {
                                field.name = GetFieldName(field.objectValue.obj.name);
                            }

                            types.Add(gameObjectType);
                            Component[] components = null;
                            if (field.objectValue.obj.GetType() == typeof(GameObject))
                            {
                                var go = field.objectValue.obj as GameObject;
                                components = go.GetComponents<Component>();
                                if(field.objectValue.type == gameObjectType)
                                {
                                    field.objectValue.obj = go;
                                }
                            }
                            else
                            {
                                var component = field.objectValue.obj as Component;
                                if (component != null)
                                {
                                    components = component.GetComponents<Component>();
                                    if (field.objectValue.type == gameObjectType)
                                    {
                                        field.objectValue.obj = component.gameObject;
                                    }
                                }
                            }
                            if (components != null)
                            {
                                for (int j = 0; j < components.Length; ++j)
                                {
                                    var componentType = components[j].GetType().FullName;

                                    types.Add(componentType);
                                    if (field.objectValue.type == componentType)
                                    {
                                        field.objectValue.obj = components[j];
                                    }

                                }
                            }
                        }
                        else
                        {
                            field.objectValue.type = gameObjectType;
                        }
                        if( string.IsNullOrEmpty( field.objectValue.type))
                        {
                            field.objectValue.type = gameObjectType;
                        }
                        var type = typeof(GameObject).Assembly.GetType(field.objectValue.type);
                        if(type== null)
                        {
                            type = typeof(SerializedField).Assembly.GetType(field.objectValue.type);
                        }
                        field.objectValue.obj = EditorGUILayout.ObjectField(field.objectValue.obj, type, true);

                        int index = types.FindIndex(x => x == field.objectValue.type);
                        if (index >= 0)
                        {
                            index = EditorGUILayout.Popup(index, types.ToArray());

                            field.objectValue.type = types[index];
                        }
                    }
                    break;
                case PropertyType.Enum:
                    {
                        if(field.enumValue== null)
                        {
                            field.enumValue = new EnumField();
                        }

                        int index = enums.FindIndex(x => x == field.enumValue.type);
                        if (index == -1) index = 0;
                        if (enums.Count > 0)
                        {
                            int k = EditorGUILayout.Popup(index, enums.ToArray());
                            if(k!=index)
                            {
                                field.enumValue.value = 0;
                            }
                            index = k;

                            field.enumValue.type = enums[index];
                        }
                        if(string.IsNullOrEmpty(field.enumValue.type)==false)
                        {
                            var type = typeof(SerializedField).Assembly.GetType(field.enumValue.type);

                            Enum value = (Enum)Enum.ToObject(type, field.enumValue.value);
                           
                            field.enumValue.value = Convert.ToInt32(EditorGUILayout.EnumPopup(value)); 
                        }

                    }
                    break;
            }
            if(GUILayout.Button("×"))
            {
                mTarget.fields.RemoveAt(i);
                continue;
            }
            if (i > 0)
            {
                if (GUILayout.Button("∧"))
                {
                    var prev = mTarget.fields[i - 1];
                    mTarget.fields[i - 1] = mTarget.fields[i];
                    mTarget.fields[i] = prev;
                }
            }
            if (i < mTarget.fields.Count - 1)
            {
                if (GUILayout.Button("∨"))
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

        if(GUILayout.Button("Add Field"))
        {
            mTarget.fields.Add(new PropertyField());
        }
        if (errorIndex >=0)
        {
            EditorGUILayout.LabelField("Error index=" + errorIndex +",name can't be null!");
        }
        string errorName = GetRepeatName();
        if(string.IsNullOrEmpty(errorName)==false)
        {
            EditorGUILayout.LabelField("Error name=" + errorName + ",name can't repeat!");
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
}

