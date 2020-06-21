using System;
using System.Collections.Generic;

public enum WindowType
{
    Normal = 0,    //0、普通界面,会加入到导航栈
    Widget = 1,    //1、小组件，比如飘字或者子界面
}

public enum WindowStatus
{
    None = 0,
    Loading = 1,     //正在加载中
    Done = 2,     //加载完成
}
public class WindowContext
{
    public const int LAYER = 5;
    public const int LAYER_MODEL = 6;
    public const int LAYER_HIDE = 7;

    public readonly string name;
    public readonly WindowType type;
    public readonly bool hideOther;
    /// <summary>
    /// 固定层级，只对Widget有效
    /// </summary>
    public readonly int fixedOrder;
    /// <summary>
    /// 固定的子UI
    /// </summary>
    public readonly List<WidgetContext> fixedWidgets;

    /// <summary>
    /// 关闭是是否Destroy
    /// </summary>
    public readonly bool closeDestroy = true;
    public readonly Type component;

    public WindowStatus status = WindowStatus.None;
    public readonly Dictionary<string, WindowContext> widgets = new Dictionary<string, WindowContext>();

    public WindowContext(string name,
        WindowType type = WindowType.Normal,
        Type component = null,
        bool hideOther = false,
        int fixedOrder = 0,
        bool closeDestroy = true,
        params WidgetContext[] fixedWidgets)
    {
        this.name = name;
        this.type = type;
        this.component = component;
        this.hideOther = hideOther;
        this.fixedOrder = fixedOrder;
        this.closeDestroy = closeDestroy;
        if (fixedWidgets != null)
        {
            this.fixedWidgets = new List<WidgetContext>(fixedWidgets);
        }

        Clear();

    }

    private int mLayer;
    public bool active
    {
        get { return mLayer == LAYER; }
        set
        {
            if (active != value)
            {
                mLayer = value ? LAYER : LAYER_HIDE;
            }
        }
    }


    public virtual void Clear()
    {
        mLayer = 0;
        status = WindowStatus.None;

        if (widgets != null)
        {
            widgets.Clear();
        }
    }
}

public class WidgetContext : WindowContext
{
    /// <summary>
    ///
    /// </summary>
    public readonly int sortingOrderOffset;
    public WidgetContext(WindowContext context, int sortingOrderOffset) : 
        base(context.name,
        WindowType.Widget,
        context.component,
        context.hideOther,
        context.fixedOrder,
        context.closeDestroy
        )
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
                if (mParent.widgets.ContainsKey(name) == false)
                {
                    mParent.widgets.Add(name, this);
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