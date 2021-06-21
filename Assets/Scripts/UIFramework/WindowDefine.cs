using System.Collections.Generic;
using System.Xml;
public class WindowDefine
{
    public readonly Dictionary<string, WindowContextBase> contexts = new Dictionary<string, WindowContextBase>();
    public void Load(string xml)
    {
        if (string.IsNullOrEmpty(xml))
        {
            return;
        }

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);
        var widgetNode = doc.SelectSingleNode("/Root/WidgetContexts");

        var widgetIter = widgetNode.ChildNodes.GetEnumerator();
        while (widgetIter.MoveNext())
        {
            var child = widgetIter.Current as XmlElement;

            if (child.Name == "WidgetContext")
            {
                WidgetContext widget = new WidgetContext();
                widget.ParseXml(child);

                contexts.Add(widget.name, widget);
            }
        }

        var windowNode = doc.SelectSingleNode("/Root/WindowContexts");

        var windowIter = windowNode.ChildNodes.GetEnumerator();
        while (windowIter.MoveNext())
        {
            var child = windowIter.Current as XmlElement;

            if (child.Name == "WindowContext")
            {
                WindowContext context = new WindowContext();
                context.ParseXml(child, GetWidgetContext);
                contexts.Add(context.name, context);
            }
        }
    }
    public WindowContextBase GetWindowContextBase(string name)
    {
        WindowContextBase context;
        contexts.TryGetValue(name, out context);
        return context;
    }

    public WidgetContext GetWidgetContext(string name)
    {
        return GetWindowContextBase(name) as WidgetContext;
    }
    public WindowContext GetWindowContext(string name)
    {
        return GetWindowContextBase(name) as WindowContext;
    }
    public void Clear()
    {
        contexts.Clear();

    }
}
