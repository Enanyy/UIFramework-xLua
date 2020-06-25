
public static class UIDefine
{
    public static WindowContext UIBg                = new WindowContext("UIBg", WindowType.Widget, typeof(UIBg), false, 0);
    public static WindowContext UIFixed             = new WindowContext("UIFixed", WindowType.Widget,typeof(UIFixed));
    public static WindowContext UIRight             = new WindowContext("UIRight", WindowType.Widget, typeof(UIRight));
    public static WindowContext UIMain              = new WindowContext("UIMain", WindowType.Normal,typeof(UIMain), true, 0, true, new WidgetContext(UIFixed,1), new WidgetContext(UIRight,1));
    public static WindowContext UINormal            = new WindowContext("UINormal", WindowType.Normal, typeof(UINormal), true, 0, true, new WidgetContext(UIBg, -1), new WidgetContext(UIFixed, 1));
    public static WindowContext UIPop               = new WindowContext("UIPop",WindowType.Normal,typeof(UIPop));
    public static WindowContext UIWidget            = new WindowContext("UIWidget", WindowType.Widget, typeof(UIWidget), false, 1000);
    public static WindowContext UISerialized        = new WindowContext("UISerialized", WindowType.Normal, typeof(UISerialized), false, 0, true, new WidgetContext(UIBg, -1));
}
