using System;
using System.Collections.Generic;

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
    public const int LAYER_MODEL = 6;
    public const int LAYER_HIDE = 7;

    private static ulong ID = 0;
    public readonly ulong id;
    public readonly string name;
    public readonly WindowType type;


    /// <summary>
    /// 关闭是是否Destroy
    /// </summary>
    public readonly bool closeDestroy = true;
    public readonly Type component;

    public WindowStatus status = WindowStatus.None;
    public int layer;
    public bool active
    {
        get { return layer == LAYER; }
        set
        {
            if (active != value)
            {
                layer = value ? LAYER : LAYER_HIDE;
            }
        }
    }


    public WindowContextBase(string name,
        WindowType type,
        Type component = null,
        bool closeDestroy = true)
    {
        id = ID++;

        this.name = name;
        this.type = type;
        this.component = component;
        this.closeDestroy = closeDestroy;
    }


    public virtual void Clear()
    {
        status = WindowStatus.None;
       
    }
}

public sealed class WindowContext: WindowContextBase
{
    /// <summary>
    /// 固定层级
    /// </summary>
    public readonly int fixedOrder;
    /// <summary>
    ///打开时是否隐藏别的UI
    /// </summary>
    public readonly bool hideOther;
  
    /// <summary>
    /// 固定的子UI
    /// </summary>
    public readonly List<WidgetContext> fixedWidgets;


    public readonly Dictionary<ulong, WidgetContext> widgets = new Dictionary<ulong, WidgetContext>();

    public WindowContext(string name,
        Type component = null,
        bool hideOther = false,
        int fixedOrder = 0,
        bool closeDestroy = true,
        params WidgetContext[] fixedWidgets):base(name, WindowType.Normal, component,closeDestroy)

    {
        this.fixedOrder = fixedOrder;
        this.hideOther = hideOther;

        if (fixedWidgets != null)
        {
            this.fixedWidgets = new List<WidgetContext>(fixedWidgets);
        }
        Clear();
    }

    public WidgetContext GetWidget(string name)
    {
        var it = widgets.GetEnumerator();
        while(it.MoveNext())
        {
            if(it.Current.Value.name == name)
            {
                return it.Current.Value;
            }
        }
        return null;
    }

    
    public override void Clear()
    {
        base.Clear();
        layer = 0;
        if (widgets != null)
        {
            widgets.Clear();
        }
    }
}

public class WidgetContext : WindowContextBase
{
    /// <summary>
    ///相当于父界面的层级差
    /// </summary>
    public readonly int sortingOrderOffset;
    public WidgetContext(string name,
        Type component = null,
        bool closeDestroy = true,
        int sortingOrderOffset = 1) : base(name, WindowType.Widget, component, closeDestroy)
    {
        this.sortingOrderOffset = sortingOrderOffset;
    }
    public WidgetContext(WidgetContext widget, int sortingOrderOffset = 1) : base(widget.name, widget.type, widget.component, widget.closeDestroy)
    {
        this.sortingOrderOffset = sortingOrderOffset;
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

    public override void Clear()
    {
        base.Clear();
        mParent = null;
    }
}