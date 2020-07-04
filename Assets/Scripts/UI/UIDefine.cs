
public static class UIDefine
{
    public static WidgetContext UIBg                = new WidgetContext("UIBg", typeof(UIBg));
    public static WidgetContext UIFixed             = new WidgetContext("UIFixed", typeof(UIFixed));
    public static WidgetContext UIRight             = new WidgetContext("UIRight", typeof(UIRight));

    public static WindowContext UIMain              = new WindowContext("UIMain", typeof(UIMain), true, 0, true, UIFixed, UIRight);
    public static WindowContext UINormal            = new WindowContext("UINormal", typeof(UINormal), true, 0, true, new WidgetContext(UIBg, -1), UIFixed);
    public static WindowContext UIPop               = new WindowContext("UIPop",typeof(UIPop));
    public static WindowContext UIWidget            = new WindowContext("UIWidget", typeof(UIWidget), false, 1000);
    public static WindowContext UISerialized        = new WindowContext("UISerialized", typeof(UISerialized), false, 0, true, new WidgetContext(UIBg, -1));
}
