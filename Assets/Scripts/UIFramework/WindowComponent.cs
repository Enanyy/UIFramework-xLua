using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WindowComponent : MonoBehaviour
{
   
    public WindowContextBase context;

   

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

    public virtual void OnAnimationEvent(string param)
    {

    }
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
