﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WindowComponent : MonoBehaviour
{
   
    public WindowContextBase contextbase;
    public WidgetContext widget
    {
        get { return contextbase as WidgetContext; }
    }
    public WindowContext context
    {
        get
        {
            if(contextbase.type == WindowType.Normal)
            {
                return contextbase as WindowContext;
            }
            else
            {
                return widget.parent;
            }
        }
    }
    public Dictionary<string, string> parameters;

    public void SetWidgetActive(string name,bool active)
    {
        WindowManager.Instance.SetWidgetActive(context.name, name, active);
    }
    public bool IsWidgetActive(string name)
    {
        WidgetContext widget = context.GetWidget(name);
        if (widget != null)
        {
            return widget.active;
        }
        return false;
    }

    public virtual void Close(bool self = false)
    {
        WindowContextBase windowContext = contextbase;

        if (self == false)
        {
            if (widget != null)
            {
                windowContext = widget.parent;
            }
        }

        WindowManager.Instance.Close(windowContext);
    }

   

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

    public string GetParam(string name)
    {
        string value = null;
        if(parameters!=null && !string.IsNullOrEmpty(name))
        {
            parameters.TryGetValue(name, out value);
        }
        return value;
    }

    public static void SetLayer(GameObject go, int layer)
    {
        if (go == null || go.layer == layer)
        {
            return;
        }
        go.layer = layer;

        var transforms = go.GetComponentsInChildren<Transform>();
        for (int i = 0; i < transforms.Length; ++i)
        {
            var child = transforms[i].gameObject;

            child.layer = layer;

        }
    }
    public virtual void OnInit() { }

    public virtual void OnShow() { }
    public virtual void OnHide() { }

    public virtual void OnAnimationEvent(string param) { }
   
    public virtual void OnWidgetShow(WidgetContext widget) 
    {
        //Debug.LogError(GetType().Name + " OnWidgetShow:" + widget.name);
    }
   
    public virtual void OnWidgetHide(WidgetContext widget) 
    {
        //Debug.LogError(GetType().Name + " OnWidgetHide:" + widget.name);
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
