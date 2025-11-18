using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Slimipelago;

public static class Helper
{
    public static GameObject[] GetChildren(this GameObject gobj)
    {
        var transform = gobj.transform;
        var count = transform.childCount;
        var children = new GameObject[count];

        for (var i = 0; i < count; i++)
        {
            children[i] = transform.GetChild(i).gameObject;
        }

        return children;
    }

    public static GameObject GetParent(this GameObject gobj) => gobj.transform.parent.gameObject;

    public static GameObject GetChild(this GameObject gobj, int index) => gobj.GetChildren()[index];

    public static GameObject[] GetChildren<TMonoBehavior>(this TMonoBehavior behavior)
        where TMonoBehavior : MonoBehaviour
        => behavior.gameObject.GetChildren();

    public static GameObject GetChild<TMonoBehavior>(this TMonoBehavior behavior, int index)
        where TMonoBehavior : MonoBehaviour
        => behavior.gameObject.GetChild(index);

    public static GameObject GetParent<TMonoBehavior>(this TMonoBehavior behavior) where TMonoBehavior : MonoBehaviour
        => behavior.transform.parent.gameObject;

    public static TOut GetPrivateField<TOut>(this object obj, string field)
    {
        var fieldInfo = obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
        if (fieldInfo is null) throw new ArgumentException($"Field [{field}] is null");
        var value = fieldInfo.GetValue(obj);
        if (value is null) throw new ArgumentException($"Value for [{field}] is null");
        return (TOut)value;
    }

    public static void SetPrivateField(this object obj, string field, object value)
        => obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(obj, value);

    public static TOut CallPrivateMethod<TOut>(this object obj, string methodName, params object[] param)
    {
        var methodInfo = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (methodInfo is null) throw new ArgumentException($"Method [{methodName}] is null");
        var value = methodInfo!.Invoke(obj, param);
        if (value is null) throw new ArgumentException($"Value for [{methodName}] is null");
        return (TOut)value;
    }

    public static void CallPrivateMethod(this object obj, string methodName, params object[] param)
    {
        var methodInfo = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (methodInfo is null) throw new ArgumentException($"Method [{methodName}] is null");
        methodInfo.Invoke(obj, param);
    }

    public static TextMeshProUGUI CreateText(string text, Color color, GameObject parent, int fontSize = 24)
    {
        var textObj = new GameObject("TextMeshProUGUI (custom)").AddComponent<TextMeshProUGUI>();
        textObj.text = text;
        textObj.color = color;
        textObj.fontSize = fontSize;
        textObj.gameObject.transform.parent = parent.gameObject.transform;
        return textObj;
    }

    public static HorizontalLayoutGroup CreateHorizontalGroup(GameObject parent)
    {
        var hGroup = new GameObject("HorizontalLayoutGroup (custom)").AddComponent<HorizontalLayoutGroup>();
        hGroup.transform.parent = parent.transform;
        return hGroup;
    }

    public static SRInputField CreateInputField(string desc, GameObject parent)
    {
        var gobj = new GameObject("SRInputField (custom)");
        gobj.transform.parent = parent.transform;
        var image = gobj.AddComponent<Image>();
        image.type = Image.Type.Sliced;
        gobj.SetActive(false);
        var mainRect = gobj.GetComponent<RectTransform>();
        mainRect.anchorMin = new Vector2(0, 1/2f);
        mainRect.anchorMax = new Vector2(0, 1/2f);

        var field = gobj.AddComponent<SRInputField>();
        field.targetGraphic = image;
        field.descKey = $"archi_{desc}";

        var textGobj = new GameObject("SRInputField Text (custom)");
        textGobj.transform.parent = gobj.transform;

        var text = field.textComponent = textGobj.AddComponent<Text>();
        var rect = field.textComponent.GetComponent<RectTransform>();
        text.alignment = TextAnchor.LowerLeft;
        // text.resizeTextForBestFit = true;
        text.fontSize = 20;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1/2f, 1/2f);
        text.gameObject.transform.localPosition = new Vector3(30, 5, 5);
        var delta = rect.sizeDelta;
        rect.sizeDelta = new Vector2(delta.x, delta.y/3f);

        gobj.AddComponent<FieldStyler>();
        gobj.SetActive(true);
        return field;
    }

    public static Button CreateButton(string text, GameObject parent, UnityAction onPressed)
    {
        var bGobj = new GameObject("Button (custom)");
        bGobj.transform.parent = parent.transform;
        bGobj.AddComponent<CanvasRenderer>();
        var img = bGobj.AddComponent<Image>();
        img.type = Image.Type.Sliced;

        var button = bGobj.AddComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(onPressed);
        button.targetGraphic = img;

        var child = new GameObject("Button Text (custom)");
        var textObj = child.AddComponent<TextMeshProUGUI>();  
        textObj.text = text;
        textObj.autoSizeTextContainer = true;
        child.transform.parent = bGobj.transform;

        bGobj.AddComponent<MeshButtonStyler>();
        return button;
    }

    public static string HashPos(this Vector3 pos) => $"[x:{Math.Floor(pos.x)}|y:{Math.Floor(pos.y)}|z:{Math.Floor(pos.z)}]";
}

[AttributeUsage(AttributeTargets.Class)]
public class PatchAll : Attribute;