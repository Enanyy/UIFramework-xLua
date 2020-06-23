using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public enum PropertyType
{
    None = -1,
    Boolean = 0,
    Integer = 1,
    Float = 2,
    String = 3,
    Object = 5,
    Vector2 = 6,
    Vector3 = 7,
    Vector4 = 8,
    Enum = 9,
}
[Serializable]
public class ObjectField
{
    public string type;
    public UnityEngine.Object obj;
}
[Serializable]
public class EnumField
{
    public string type;
    public int value;
}

[AttributeUsage(AttributeTargets.Enum)]
public class PropertyEnumAttribute : Attribute
{
}
[Serializable]
public class PropertyField
{
    public string name;
    public PropertyType type = PropertyType.None;

    public bool boolValue;
    public int intValue;
    public float floatValue;
    public string stringValue;
    public ObjectField objectValue;
    public EnumField enumValue;
    public Vector2 vector2Value;
    public Vector3 vector3Value;
    public Vector4 vector4Value;

}

public class SerializedField : MonoBehaviour
{
    [HideInInspector]
    public List<PropertyField> fields= new List<PropertyField>();

    private Dictionary<string, int> name2IndexDic = new Dictionary<string, int>();
    private void Awake()
    {
        for(int i = 0;i < fields.Count; ++i)
        {
            var field = fields[i];
            name2IndexDic.Add(field.name, i);
        }
    }

    public int GetInt(string name,int defaultValue = 0)
    {
        if(name2IndexDic.TryGetValue(name, out int index))
        {
            var field = fields[index];
            if(field.type == PropertyType.Integer)
            {
                return field.intValue;
            }
            else
            {
                Debug.LogError(string.Format("{0} type is {1}", name, field.type));
            }
        }
        return defaultValue;
    }
    public bool GetBool(string name, bool defaultValue = false)
    {
        if (name2IndexDic.TryGetValue(name, out int index))
        {
            var field = fields[index];
            if (field.type == PropertyType.Boolean)
            {
                return field.boolValue;
            }
            else
            {
                Debug.LogError(string.Format("{0} type is {1}", name, field.type));
            }
        }
        return defaultValue;
    }
    public float GetFloat(string name, float defaultValue = 0)
    {
        if (name2IndexDic.TryGetValue(name, out int index))
        {
            var field = fields[index];
            if (field.type == PropertyType.Float)
            {
                return field.floatValue;
            }
            else
            {
                Debug.LogError(string.Format("{0} type is {1}", name, field.type));
            }
        }
        return defaultValue;
    }

    public string GetString(string name, string defaultValue = "")
    {
        if (name2IndexDic.TryGetValue(name, out int index))
        {
            var field = fields[index];
            if (field.type == PropertyType.String)
            {
                return field.stringValue;
            }
            else
            {
                Debug.LogError(string.Format("{0} type is {1}", name, field.type));
            }
        }
        return defaultValue;
    }

    public Vector2 GetVector2(string name )
    {
        if (name2IndexDic.TryGetValue(name, out int index))
        {
            var field = fields[index];
            if (field.type == PropertyType.Vector2)
            {
                return field.vector2Value;
            }
            else
            {
                Debug.LogError(string.Format("{0} type is {1}", name, field.type));
            }
        }
        return Vector2.zero;
    }

    public Vector2 GetVector3(string name)
    {
        if (name2IndexDic.TryGetValue(name, out int index))
        {
            var field = fields[index];
            if (field.type == PropertyType.Vector3)
            {
                return field.vector2Value;
            }
            else
            {
                Debug.LogError(string.Format("{0} type is {1}", name, field.type));
            }
        }
        return Vector3.zero;
    }
    public Vector4 GetVector4(string name)
    {
        if (name2IndexDic.TryGetValue(name, out int index))
        {
            var field = fields[index];
            if (field.type == PropertyType.Vector4)
            {
                return field.vector2Value;
            }
            else
            {
                Debug.LogError(string.Format("{0} type is {1}", name, field.type));
            }
        }
        return Vector4.zero;
    }
    public int GetEnum(string name)
    {
        if (name2IndexDic.TryGetValue(name, out int index))
        {
            var field = fields[index];
            if (field.type == PropertyType.Enum)
            {
                if (field.enumValue != null)
                {
                    return field.enumValue.value;
                }
                else
                {
                    Debug.LogError(string.Format("{0} enumValue is null", name));
                }
            }
            else
            {
                Debug.LogError(string.Format("{0} type is {1}", name, field.type));
            }
        }
        return 0;
    }


    public GameObject GetObject(string name)
    {
        if (name2IndexDic.TryGetValue(name, out int index))
        {
            var field = fields[index];
            if (field.type == PropertyType.Object)
            {
                if (field.objectValue != null)
                {
                    if (field.objectValue.type == typeof(GameObject).FullName)
                    {
                        return field.objectValue.obj as GameObject;
                    }
                    else
                    {
                        Debug.LogError(string.Format("{0} objectValue type is {1}", name, field.objectValue.type));
                    }
                }
                else
                {
                    Debug.LogError(string.Format("{0} objectValue is null", name));
                }
            }
            else
            {
                Debug.LogError(string.Format("{0} type is {1}", name, field.type));
            }
        }
        return null;
    }

    public T GetComponent<T>(string name) where T:Component
    {
        if (name2IndexDic.TryGetValue(name, out int index))
        {
            var field = fields[index];
            if (field.type == PropertyType.Object)
            {
                if (field.objectValue != null)
                {
                    if (field.objectValue.type == typeof(T).FullName)
                    {
                        return field.objectValue.obj as T;
                    }
                    else
                    {
                        Debug.LogError(string.Format("{0} objectValue type is {1}", name, field.objectValue.type));
                    }
                }
                else
                {
                    Debug.LogError(string.Format("{0} objectValue is null", name));
                }
            }
            else
            {
                Debug.LogError(string.Format("{0} type is {1}", name, field.type));
            }
        }
        return null;
    }

    public Component GetComponent(string name, Type type) 
    {
        if (name2IndexDic.TryGetValue(name, out int index))
        {
            var field = fields[index];
            if (field.type == PropertyType.Object)
            {
                if (field.objectValue.type == type.FullName)
                {
                    return field.objectValue.obj as Component;
                }
                else
                {
                    Debug.LogError(string.Format("{0} objectValue type is {1}", name, field.objectValue.type));
                }
            }
            else
            {
                Debug.LogError(string.Format("{0} type is {1}", name, field.type));
            }
        }
        return null;
    }

}

#region Test Enum

[PropertyEnum]
public enum MyEnum1
{
    MyEnum1,
    MyEnum2,
}
[PropertyEnum]
public enum MyEnum2
{
    MyEnum1,
    MyEnum2,
    MyEnum3,
    MyEnum4,
    MyEnum5,
}
#endregion
