using System;
using System.Collections.Generic;
using System.Xml;

public enum WindowType
{
    Normal = 0,    //0、普通界面,会加入到导航栈,要fixedOrder等于0
    Widget = 1,    //1、小组件，比如飘字或者子界面
}

public enum WindowStatus
{
    None = 0,
    Loading = 1,     //正在加载中
    Done = 2,     //加载完成
}

public abstract class WindowContextBase
{
    public const int LAYER = 5;
    private static ulong ID = 0;

    /// <summary>
    /// 唯一ID
    /// </summary>
    public readonly ulong id;
    /// <summary>
    /// 唯一名字，通过名字打开
    /// </summary>
    public string name { get; protected set; }
    /// <summary>
    /// 预设路径
    /// </summary>
    public string path { get; protected set; }
    /// <summary>
    /// UI类型
    /// </summary>
    public abstract WindowType type { get; }


    /// <summary>
    /// 关闭是是否Destroy
    /// </summary>
    public bool closeDestroy { get; protected set; } = true;
    public Dictionary<Type,Dictionary<string, string>> components;

    public WindowStatus status = WindowStatus.None;

    private bool mActive;
    public bool active
    {
        get { return mActive; }
        set
        {
            if (mActive != value)
            {
                mActive = value;
            }
        }
    }


    public WindowContextBase()
    {
        id = ID++;
    }

    public virtual void CopyFrom(WindowContextBase context)
    {
        name = context.name;
        path = context.path;
        closeDestroy = context.closeDestroy;
        if(context.components!=null)
        {
            components = new Dictionary<Type, Dictionary<string, string>>();

            using (var it = context.components.GetEnumerator())
            {
                while(it.MoveNext())
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>(it.Current.Value);
                    components.Add(it.Current.Key, parameters);
                }
            }

        }
    }


    public virtual void Clear()
    {
        status = WindowStatus.None;
    }

    public void AddComponent(Type component, Dictionary<string, string> parameters)
    {
        if (components == null)
        {
            components = new Dictionary<Type, Dictionary<string, string>>();
        }
        if(components.ContainsKey(component))
        {
            components[component] = parameters;
        }
        else
        {
            components.Add(component, parameters);
        }
    }
    public Dictionary<string, string> GetParameters(Type component)
    {
        if(components!=null)
        {
            components.TryGetValue(component, out Dictionary<string, string> parameters);
            return parameters;
        }
        return null;
    }


    public virtual void ParseXml(XmlElement node)
    {
        name = node.GetAttribute("name");
        path = node.GetAttribute("path");
        closeDestroy = bool.Parse(node.GetAttribute("closeDestroy"));

        AddComponent(node);
    }

    public virtual void AddComponent(XmlElement node)
    {
        if(node== null)
        {
            return;
        }
        var it = node.ChildNodes.GetEnumerator();
        while (it.MoveNext())
        {
            var componentChild = it.Current as XmlElement;
            if (componentChild.Name == "Component")
            {
                Type type = Type.GetType(componentChild.GetAttribute("type"));
                Dictionary<string, string> parameters = GetParameters(type);
                var paramIter = componentChild.ChildNodes.GetEnumerator();
                while (paramIter.MoveNext())
                {
                    var paramChild = paramIter.Current as XmlElement;
                    if (paramChild.Name == "Param")
                    {
                        var attributeIter = paramChild.Attributes.GetEnumerator();
                        while (attributeIter.MoveNext())
                        {
                            var attribute = attributeIter.Current as XmlAttribute;
                            if (parameters == null)
                            {
                                parameters = new Dictionary<string, string>();
                            }
                            if (parameters.ContainsKey(attribute.Name) == false)
                            {
                                parameters.Add(attribute.Name, attribute.Value);
                            }else
                            {
                                parameters[attribute.Name]= attribute.Value;
                            }
                        }
                    }
                }
                AddComponent(type, parameters);
            }
        }
    }
}

public sealed class WindowContext : WindowContextBase
{
    /// <summary>
    /// 固定层级
    /// </summary>
    public int fixedOrder { get; private set; } = 0;
    /// <summary>
    ///打开时是否隐藏别的UI
    /// </summary>
    public bool hideOther { get; private set; } = true;

    public override WindowType type => WindowType.Normal;
    /// <summary>
    /// 固定的子UI
    /// </summary>
    public Dictionary< string, WidgetContext> fixedWidgets;
    public Dictionary<string, WidgetParam> widgetParams; 
   

    public readonly Dictionary<ulong, WidgetContext> widgets = new Dictionary<ulong, WidgetContext>();

    public WindowContext()
    {
        Clear();
    }

    public override void CopyFrom(WindowContextBase context)
    {
        base.CopyFrom(context);
        WindowContext windowContext = context as WindowContext;
        if(windowContext!=null)
        {
            fixedOrder = windowContext.fixedOrder;
            hideOther = windowContext.hideOther;

            if(windowContext.fixedWidgets!=null)
            {
                fixedWidgets = new Dictionary<string, WidgetContext>(windowContext.fixedWidgets);
            }
        }
    }

    public void AddFixedWidget(WidgetContext widget)
    {
        if (fixedWidgets == null)
        {
            fixedWidgets = new Dictionary<string, WidgetContext>();
        }
        if(fixedWidgets.ContainsKey(widget.name)==false)
        {
            fixedWidgets.Add(widget.name,widget);
        }
        else
        {
            fixedWidgets[widget.name] = widget;
        }
    }

    public WidgetContext GetFixedWidget(string name)
    {
        if(fixedWidgets == null)
        {
            return null;
        }
        fixedWidgets.TryGetValue(name, out WidgetContext widget);
        return widget;
    }

    public WidgetContext GetWidget(string name)
    {
        using (var it = widgets.GetEnumerator())
        {
            while (it.MoveNext())
            {
                if (it.Current.Value.name == name)
                {
                    return it.Current.Value;
                }
            }
        }
        return GetFixedWidget(name);
    }
    public List<WidgetContext> GetWidgets(int group)
    {
        List<WidgetContext> list = new List<WidgetContext>();
        using (var it = widgets.GetEnumerator())
        {
            while (it.MoveNext())
            {
                if (it.Current.Value.group == group)
                {
                    list.Add(it.Current.Value);
                }
            }
        }
        if (fixedWidgets != null)
        {
            using (var it = fixedWidgets.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    if (it.Current.Key == name && list.Contains(it.Current.Value)==false)
                    {
                        list.Add(it.Current.Value);
                    }
                }
            }
        }
       

        return list;
    }

    public void AddWidgetParam(string name, WidgetParam param)
    {
        if (widgetParams == null)
        {
            widgetParams = new Dictionary<string, WidgetParam>();
        }

        if (widgetParams.ContainsKey(name) == false)
        {
            widgetParams.Add(name, param);
        }
        else
        {
            widgetParams[name] = param;
        }

    }
    public WidgetParam GetWidgetParam(string name)
    {
        if(widgetParams!=null)
        {
            widgetParams.TryGetValue(name, out WidgetParam param);
            return param;
        }
        return null;
    }

    public void Foreach(Func<WidgetContext, bool> func)
    {
        if (func == null)
        {
            return;
        }
        bool result = false;
        using (var it = widgets.GetEnumerator())
        {
            while (it.MoveNext())
            {
                if (func(it.Current.Value))
                {
                    result = true;
                    break;
                }
            }
        }
        if (result == false)
        {
            if (fixedWidgets != null)
            {
                using (var it = fixedWidgets.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        if (func(it.Current.Value))
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
        }
    }


    public override void Clear()
    {
        base.Clear();
        active = false;
        if (widgets != null)
        {
            widgets.Clear();
        }
    }

    public override void ParseXml(XmlElement node)
    {
        base.ParseXml(node);
        var s = node.GetAttribute("fixedOrder");
        if(string.IsNullOrEmpty(s) == false)
        {
            fixedOrder = int.Parse(s);
        }

        s = node.GetAttribute("hideOther");
        if (string.IsNullOrEmpty(s) == false)
        {
            hideOther = bool.Parse(s);
        }
    }

    public void ParseXml(XmlElement node, Func<string, WidgetContext> func)
    {
        ParseXml(node);

        if (func != null)
        {
            var it = node.ChildNodes.GetEnumerator();
            while (it.MoveNext())
            {
                var child = it.Current as XmlElement;
                if (child.Name == "WidgetContext")
                {
                    string name = child.GetAttribute("ref");
                   
                    var param = new WidgetParam();
                    WidgetContext widget = func(name);
                    if (widget != null)
                    {
                        param.CopyFrom(widget.defaultParam);
                        bool clone = false;
                        string s = child.GetAttribute("clone");
                        if (string.IsNullOrEmpty(s) == false)
                        {
                            clone = bool.Parse(s);
                        }
                        if (!clone)
                        {
                            AddFixedWidget(widget);
                        }
                        else
                        {
                            WidgetContext cloneWidget = new WidgetContext();
                            cloneWidget.CopyFrom(widget);
                            cloneWidget.AddComponent(child);
                            AddFixedWidget(cloneWidget);
                        }
                    }
                    param.ParseXml(child);
                    AddWidgetParam(name, param);

                }
            }
        }
    }
}

public interface IWidgetContext
{
    public int sortingOrderOffset { get; }
    public int group { get; }
    public string bindNode { get;  }

    public bool defaultActive { get; }

    void ParseXml(XmlElement node);

}

public class WidgetParam : IWidgetContext
{
    public int sortingOrderOffset { get; private set; } = 0;

    public int group { get; private set; } = 0;

    public string bindNode { get; private set; }

    public bool defaultActive { get; private set; } = true;


    public void ParseXml(XmlElement node)
    {
        var s = node.GetAttribute("sortingOrderOffset");
        if (string.IsNullOrEmpty(s) == false)
        {
            sortingOrderOffset = int.Parse(s);
        }
        s = node.GetAttribute("group");
        if (string.IsNullOrEmpty(s) == false)
        {
            group = int.Parse(s);
        }

        bindNode = node.GetAttribute("bindNode");

        s = node.GetAttribute("defaultActive");
        if (string.IsNullOrEmpty(s) == false)
        {
            defaultActive = bool.Parse(s);
        }
    }

    public void CopyFrom(WidgetParam other)
    {
        sortingOrderOffset = other.sortingOrderOffset;
        group = other.group;
        bindNode = other.bindNode;
        defaultActive = other.defaultActive;
    }
}

public class WidgetContext : WindowContextBase,IWidgetContext
{
    /// <summary>
    ///相当于父界面的层级差
    /// </summary>
    public int sortingOrderOffset =>  param.sortingOrderOffset; 
    public int group =>  param.group;
    public string bindNode => param.bindNode;
    public bool defaultActive => param.defaultActive;

    public WidgetParam defaultParam = new WidgetParam();

    public override WindowType type => WindowType.Widget;
    public WidgetContext()
    {

    }

    public override void CopyFrom(WindowContextBase context)
    {
        base.CopyFrom(context);
        WidgetContext widget = context as WidgetContext;
        if(widget!=null)
        {
            defaultParam.CopyFrom(widget.defaultParam);
        }
    }
  

    private WindowContext mParent;
    public WindowContext parent
    {
        get { return mParent; }
        set
        {
            mParent = value;
            if (mParent != null)
            {
                if (mParent.widgets.ContainsKey(id) == false)
                {
                    mParent.widgets.Add(id, this);
                }
            }
        }
    }
    public WidgetParam param
    {
        get
        {
            if (parent != null)
            {
                var p = parent.GetWidgetParam(name);
                if (p != null)
                {
                    return p;
                }
            }
            return defaultParam;
        }
    }



    public override void ParseXml(XmlElement node)
    {
        base.ParseXml(node);
        defaultParam.ParseXml(node);
    }

    public override void Clear()
    {
        base.Clear();
        mParent = null;
    }
}
