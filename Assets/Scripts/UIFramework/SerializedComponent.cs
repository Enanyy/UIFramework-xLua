using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum SerializedFieldType
{
    None = -1,
    Boolean = 0,
    Integer = 1,
    Float = 2,
    Enum = 3,
    String = 4,
    Object = 5,
    Vector2 = 6,
    Vector3 = 7,
    Vector4 = 8,
}


[AttributeUsage(AttributeTargets.Enum)]
public class SerializedEnumAttribute : Attribute
{
}
[Serializable]
public class SerializedField
{
    public string name;
    public SerializedFieldType type = SerializedFieldType.None;

    public float[] values;
    public string stringValue;
    public UnityEngine.Object objectValue;

    public void SetInt(int value)
    {
        if (values == null || values.Length != 1)
        {
            values = new float[1];
        }
        values[0] = value;
        objectValue = null;
        stringValue = null;

        type = SerializedFieldType.Integer;
    }
    public int GetInt()
    {
        if (type == SerializedFieldType.Integer)
        {
            if (values != null && values.Length == 1)
            {
                return (int)values[0];
            }
        }
        return 0;
    }
    public void SetFloat(float value)
    {
        if (values == null || values.Length != 1)
        {
            values = new float[1];
        }
        values[0] = value;

        objectValue = null;
        stringValue = null;

        type = SerializedFieldType.Float;
    }

    public float GetFloat()
    {
        if (type == SerializedFieldType.Float)
        {
            if (values != null && values.Length == 1)
            {
                return values[0];
            }
        }
        return 0;
    }

    public void SetBool(bool value)
    {
        if (values == null || values.Length != 1)
        {
            values = new float[1];
        }
        values[0] = value ? 1 : -1;

        objectValue = null;
        stringValue = null;

        type = SerializedFieldType.Boolean;
    }
    public bool GetBool()
    {
        if (type == SerializedFieldType.Boolean)
        {
            if (values != null && values.Length == 1)
            {
                return values[0] > 0;
            }
        }
        return false;
    }
    public void SetString(string value)
    {
        stringValue = value;

        values = null;
        objectValue = null;

        type = SerializedFieldType.String;
    }
    public string GetString()
    {
        if (type == SerializedFieldType.String)
        {
            return stringValue;
        }
        return null;
    }

    public void SetVector2(Vector2 value)
    {
        if (values == null || values.Length != 2)
        {
            values = new float[2];
        }
        values[0] = value.x;
        values[1] = value.y;

        objectValue = null;
        stringValue = null;

        type = SerializedFieldType.Vector2;

    }
    public Vector2 GetVector2()
    {
        if (type == SerializedFieldType.Vector2)
        {
            if (values != null && values.Length == 2)
            {
                return new Vector2(values[0], values[1]);
            }
        }
        return Vector2.zero;

    }
    public void SetVector3(Vector3 value)
    {
        if (values == null || values.Length != 3)
        {
            values = new float[3];
        }
        values[0] = value.x;
        values[1] = value.y;
        values[2] = value.z;

        objectValue = null;
        stringValue = null;

        type = SerializedFieldType.Vector3;
    }
    public Vector3 GetVector3()
    {
        if (type == SerializedFieldType.Vector3)
        {
            if (values != null && values.Length == 3)
            {
                return new Vector3(values[0], values[1], values[2]);
            }
        }
        return Vector3.zero;

    }

    public void SetVector4(Vector4 value)
    {
        if (values == null || values.Length != 4)
        {
            values = new float[4];
        }
        values[0] = value.x;
        values[1] = value.y;
        values[2] = value.z;
        values[3] = value.w;

        objectValue = null;
        stringValue = null;

        type = SerializedFieldType.Vector4;
    }
    public Vector4 GetVector4()
    {
        if (type == SerializedFieldType.Vector4)
        {
            if (values != null && values.Length == 4)
            {
                return new Vector4(values[0], values[1], values[2], values[3]);
            }
        }
        return Vector4.zero;

    }


    public void SetEnum(string type, int value)
    {
        if (values == null || values.Length != 1)
        {
            values = new float[1];
        }
        values[0] = value;
        stringValue = type;

        objectValue = null;

        this.type = SerializedFieldType.Enum;
    }
    public int GetEnum()
    {
        if (type == SerializedFieldType.Enum)
        {
            if (values != null && values.Length == 1)
            {
                return (int)values[0];
            }
        }
        return 0;
    }
    public string GetEnumType()
    {
        if (type == SerializedFieldType.Enum)
        {
            return stringValue;
        }
        return null;
    }
    public void SetObject(string type, UnityEngine.Object obj)
    {
        objectValue = obj;
        stringValue = type;

        values = null;

        this.type = SerializedFieldType.Object;
    }
    public UnityEngine.Object GetObject()
    {
        if (type == SerializedFieldType.Object)
        {
            return objectValue;
        }
        return null;
    }

    public void SetObjectType(string type)
    {
        values = null;

        this.type = SerializedFieldType.Object;
        stringValue = type;
    }

    public string GetObjectType()
    {
        if (type == SerializedFieldType.Object)
        {
            return stringValue;
        }
        return null;
    }

}
public class SerializedComponent : MonoBehaviour
{
    [HideInInspector]
    public List<SerializedField> fields = new List<SerializedField>();

    private Dictionary<string, int> name2IndexDic = new Dictionary<string, int>();

    public SerializedField this[string name]
    {
        get
        {
            if(name2IndexDic.Count == 0)
            {
                for (int i = 0; i < fields.Count; ++i)
                {
                    name2IndexDic.Add(fields[i].name, i);
                }
            }
            name2IndexDic.TryGetValue(name, out int index);
            if (index >= 0)
            {
                return fields[index];
            }
            return null;
        }
    }

    public Component GetSerializedComponent(string name)
    {
        var field = this[name];
        if(field!=null)
        {
            return field.GetObject() as Component;
        }
        return null;
    }

    public T GetSerializedComponent<T>(string name) where T:Component
    {
        var field = this[name];
        if (field != null)
        {
            return field.GetObject() as T;
        }
        return null;
    }

    public GameObject GetSerializedGameObject(string name)
    {
        var field = this[name];
        if (field != null)
        {
            return field.GetObject() as GameObject;
        }
        return null;
    }

    public void AddClick(string name,UnityAction call)
    {
        Button button = GetSerializedComponent<Button>(name);
        if(button!=null)
        {
            button.onClick.AddListener(call);
        }
    }
    public void SetText(string name,string text)
    {
        Text button = GetSerializedComponent<Text>(name);
        if (button != null)
        {
            button.text = text;
        }
    }
}