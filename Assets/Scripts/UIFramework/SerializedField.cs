using System;
using System.Collections.Generic;
using UnityEngine;

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
