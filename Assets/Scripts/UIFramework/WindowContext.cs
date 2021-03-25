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
    public List<Type> components;

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
            components = new List<Type>(context.components);
        }
    }


    public virtual void Clear()
    {
        status = WindowStatus.None;
    }

    public void AddComponent(Type component)
    {
        if (components == null)
        {
            components = new List<Type>();
        }
        components.Add(component);
    }

    public virtual void ParseXml(XmlElement node)
    {
        name = node.GetAttribute("name");
        path = node.GetAttribute("path");
        closeDestroy = bool.Parse(node.GetAttribute("closeDestroy"));

        var it = node.ChildNodes.GetEnumerator();
        while (it.MoveNext())
        {
            var componentChild = it.Current as XmlElement;
            if (componentChild.Name == "Component")
            {
                Type type = Type.GetType(componentChild.GetAttribute("type"));
                AddComponent(type);
            }
        }

    }
}

public sealed class WindowContext : WindowContextBase
{
    /// <summary>
    /// 固定层级
    /// </summary>
    public int fixedOrder { get; private set; }
    /// <summary>
    ///打开时是否隐藏别的UI
    /// </summary>
    public bool hideOther { get; private set; }

    public override WindowType type => WindowType.Normal;
    /// <summary>
    /// 固定的子UI
    /// </summary>
    public List<WidgetContext> fixedWidgets;


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
                fixedWidgets = new List<WidgetContext>(windowContext.fixedWidgets);
            }
        }
    }

    public void AddFixedWidget(WidgetContext widget)
    {
        if (fixedWidgets == null)
        {
            fixedWidgets = new List<WidgetContext>();
        }
        fixedWidgets.Add(widget);
    }

    public WidgetContext GetWidget(string name)
    {
        var it = widgets.GetEnumerator();
        while (it.MoveNext())
        {
            if (it.Current.Value.name == name)
            {
                return it.Current.Value;
            }
        }
        return null;
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
        fixedOrder = int.Parse(node.GetAttribute("fixedOrder"));
        hideOther = bool.Parse(node.GetAttribute("hideOther"));
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
                    string name = child.GetAttribute("name");
                    WidgetContext widget = func(name);
                    if (widget != null)
                    {
                        bool clone = bool.Parse(child.GetAttribute("clone"));
                        if (!clone)
                        {
                            AddFixedWidget(widget);
                        }
                        else
                        {
                            int sortingOrderOffset = int.Parse(child.GetAttribute("sortingOrderOffset"));
                            int group = int.Parse(child.GetAttribute("group"));

                            WidgetContext cloneWidget = new WidgetContext();
                            cloneWidget.CopyFrom(widget);
                            cloneWidget.sortingOrderOffset = sortingOrderOffset;
                            cloneWidget.group = group;
                            AddFixedWidget(cloneWidget);
                        }
                    }
                }
            }
        }
    }
}

public class WidgetContext : WindowContextBase
{
    /// <summary>
    ///相当于父界面的层级差
    /// </summary>
    public int sortingOrderOffset { get; set; }
    public int group { get; set; }

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
            sortingOrderOffset = widget.sortingOrderOffset;
            group = widget.group;
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

    public override void ParseXml(XmlElement node)
    {
        base.ParseXml(node);
        sortingOrderOffset = int.Parse(node.GetAttribute("sortingOrderOffset"));
    }

    public override void Clear()
    {
        base.Clear();
        mParent = null;
    }
}