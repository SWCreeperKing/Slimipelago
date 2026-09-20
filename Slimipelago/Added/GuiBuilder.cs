using UnityEngine;

namespace Slimipelago.Archipelago.CustGui;

public abstract class GuiBuilder
{
    private bool Init;

    protected abstract void InitGUI();
    protected abstract void OnGUI();

    public void Render()
    {
        if (!Init)
        {
            Init = true;
            InitGUI();
        }

        OnGUI();
    }
}

public class ScrollWindow(int id, float x, float y, float w, float h, TextRef title, float padding = 3,
    bool dragable = true)
    : GuiContainer<ScrollWindow>(x, y, w, title, padding, h)
{
    private static Rect DragSize = new(0, 0, 10000, 20);
    private Rect ScrollRect = new(0, 0, w + 40, h + 40);
    private Rect VisibleRect = new(0, 20, w, h);
    private Vector2 ScrollPos = Vector2.zero;

    protected override void SelfRender(Rect rect, string title) => SetRect(
        GUI.Window(
            id, rect, __ =>
            {
                if (dragable) GUI.DragWindow(DragSize);
                ScrollPos = GUI.BeginScrollView(VisibleRect, ScrollPos, ScrollRect);
                RenderChildren(false, out _, out _, out var h);
                GUI.EndScrollView();
                ScrollRect = ScrollRect with { height = h };
            }, title, GUI.skin.box
        )
    );
}

public class Window(int id, float x, float y, float w, TextRef title, float padding = 3, bool dragable = true)
    : GuiContainer<Window>(x, y, w, title, padding)
{
    private static Rect DragSize = new(0, 0, 10000, 20);

    protected override void SelfRender(Rect rect, string title) => SetRect(
        GUI.Window(
            id, rect, __ =>
            {
                if (dragable) GUI.DragWindow(DragSize);
                RenderChildren(false, out _, out _, out var h);
                SetRect(GetRect() with { height = h });
            }, title, GUI.skin.box
        )
    );
}

public class TitleBox(float x, float y, float w, TextRef title, float padding = 3)
    : GuiContainer<TitleBox>(x, y, w, title, padding)
{
    protected override void SelfRender(Rect rect, string title)
    {
        GUI.Box(rect, title);
        RenderChildren(true, out _, out _, out var h);
        SetRect(GetRect() with { height = h });
    }
}

public abstract class GuiContainer<T>(float x, float y, float w, TextRef title, float padding = 3, float h = 0)
    where T : GuiContainer<T>
{
    public List<IGuiElement> Children = [];

    private Rect BoxRect = new(x, y, w, h);

    public void Render() => SelfRender(BoxRect, title);

    protected void RenderChildren(bool isChildrenRelative, out float xPos, out float yPos, out float h)
    {
        xPos = (isChildrenRelative ? x : 0) + padding;
        yPos = (isChildrenRelative ? y : 0) + padding;

        foreach (var child in Children)
        {
            child.Render(ref xPos, ref yPos, w - padding * 2);
            yPos += padding;
        }

        h = yPos - (isChildrenRelative ? y : 0);
    }

    protected abstract void SelfRender(Rect rect, string title);

    public void SetRect(Rect rect) => BoxRect = rect;
    public Rect GetRect() => BoxRect;

    public T AddChild(IGuiElement children)
    {
        Children.Add(children);
        return (T)this;
    }

    public T AddChildren(params IGuiElement[] children)
    {
        Children.AddRange(children);
        return (T)this;
    }
}

public class Spacer(float height) : IGuiElement
{
    public void Render(ref float x, ref float y, float w) => y += height;
}

public class ButtonGroup(SetterGetter<int> button, string[] buttonOptions, float height) : IGuiElement
{
    public Rect Rect = new(0, 0, height, 0);

    public void Render(ref float x, ref float y, float w)
    {
        button.Data = GUI.Toolbar(Rect with { x = x, y = y, width = w, height = height }, button, buttonOptions);
        y += height;
    }
}

public class Label(TextRef text, float height, Color? color = null, int fontSize = 12,
    TextAnchor alignment = TextAnchor.LowerLeft) : IGuiElement
{
    public Rect Rect = new(0, 0, height, 0);

    private GUIStyle Style = new()
    {
        fontSize = fontSize, alignment = alignment, normal = { textColor = color ?? Color.white },
    };

    public void Render(ref float x, ref float y, float w)
    {
        GUI.Label(Rect with { x = x, y = y, width = w, height = height }, text, Style);
        y += height;
    }
}

public class TextField(SetterGetter<string> text, float height) : IGuiElement
{
    private GUIStyle Style = new(GUI.skin.textField) { alignment = TextAnchor.MiddleLeft };
    public Rect Rect = new(0, 0, height, 0);

    public void Render(ref float x, ref float y, float w)
    {
        text.Data = GUI.TextField(Rect with { x = x, y = y, width = w, height = height }, text, Style);
        y += height;
    }
}

public class Toggle(SetterGetter<bool> toggle, TextRef text, float height)
    : IGuiElement
{
    public Rect Rect = new(0, 0, height, 0);

    public void Render(ref float x, ref float y, float w)
    {
        toggle.Data = GUI.Toggle(Rect with { x = x, y = y, width = w, height = height }, toggle, text);
        y += height;
    }
}

public class Button(TextRef text, float height, Action pressed) : IGuiElement
{
    public Rect Rect = new(0, 0, height, 0);

    public void Render(ref float x, ref float y, float w)
    {
        if (GUI.Button(Rect with { x = x, y = y, width = w, height = height }, text)) pressed();
        y += height;
    }
}

public interface IGuiElement
{
    public void Render(ref float x, ref float y, float w);
}

public class TextRef(Func<string> textFunc)
{
    public string Data => textFunc();

    public static implicit operator TextRef(string s) => new(() => s);
    public static implicit operator string(TextRef textRef) => textRef.Data;
}

public class SetterGetter<TSetGet>(Func<TSetGet> get, Action<TSetGet> set)
{
    public TSetGet Data { get => get(); set => set(value); }

    public static implicit operator TSetGet(SetterGetter<TSetGet> setterGetter) => setterGetter.Data;
}