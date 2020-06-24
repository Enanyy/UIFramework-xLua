using System;
using System.Collections.Generic;
using UnityEngine;


public class WindowComponent : MonoBehaviour
{
   
    public WindowContext context;

   

    public T GetComponent<T>(string path) where T :Component
    {
        if(string.IsNullOrEmpty(path))
        {
            TryGetComponent(out T component);
            return component;
        }
        else
        {
            Transform child = transform.Find(path);
            if(child)
            {
                child.TryGetComponent(out T component);
                return component;
            }
        }
        return null;
    }

    public virtual void OnShow() { }
    public virtual void OnHide() { }
}

#region Test Enum

[SerializedEnum]
public enum MyEnum1
{
    TestValue1,
    TestValue2,
}
[SerializedEnum]
public enum MyEnum2
{
    TestValue1,
    TestValue2,
    TestValue3,
    TestValue4,
    TestValue5,
}
#endregion
