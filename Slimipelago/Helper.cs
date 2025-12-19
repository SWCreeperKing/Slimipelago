using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Slimipelago;

public static class Helper
{
    public static Dictionary<Vector3, string> PosHashCache = [];
    
    extension(GameObject gobj)
    {
        public GameObject[] GetChildren()
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

        public GameObject GetParent() => gobj.transform.parent.gameObject;
        public GameObject GetChild(int index) => gobj.GetChildren()[index];
    }

    extension<TMonoBehavior>(TMonoBehavior behavior) where TMonoBehavior : MonoBehaviour
    {
        public GameObject[] GetChildren() => behavior.gameObject.GetChildren();
        public GameObject GetChild(int index) => behavior.gameObject.GetChild(index);
        public GameObject GetParent() => behavior.transform.parent.gameObject;
    }

    extension(object obj)
    {
        public TOut GetPrivateField<TOut>(string field)
        {
            var fieldInfo = obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo is null) throw new ArgumentException($"Field [{field}] is null");
            var value = fieldInfo.GetValue(obj);
            if (value is null) throw new ArgumentException($"Value for [{field}] is null");
            return (TOut)value;
        }

        public void SetPrivateField(string field, object value)
            => obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(obj, value);

        public TOut CallPrivateMethod<TOut>(string methodName, params object[] param)
        {
            var methodInfo = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo is null) throw new ArgumentException($"Method [{methodName}] is null");
            var value = methodInfo!.Invoke(obj, param);
            if (value is null) throw new ArgumentException($"Value for [{methodName}] is null");
            return (TOut)value;
        }

        public void CallPrivateMethod(string methodName, params object[] param)
        {
            var methodInfo = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo is null) throw new ArgumentException($"Method [{methodName}] is null");
            methodInfo.Invoke(obj, param);
        }
    }
    
    public static HorizontalLayoutGroup CreateHorizontalGroup(GameObject parent)
    {
        var hGroup = new GameObject("HorizontalLayoutGroup (custom)").AddComponent<HorizontalLayoutGroup>();
        hGroup.transform.SetParent(parent.transform);
        return hGroup;
    }
    
    public static TextMeshProUGUI CreateText(string text, Color color, GameObject parent, int fontSize = 24)
    {
        var textObj = new GameObject("TextMeshProUGUI (custom)").AddComponent<TextMeshProUGUI>();
        textObj.text = text;
        textObj.color = color;
        textObj.fontSize = fontSize;
        textObj.transform.SetParent(parent.transform);
        return textObj;
    }

    public static SRInputField CreateInputField(string desc, GameObject parent)
    {
        var gobj = new GameObject("SRInputField (custom)");
        gobj.transform.SetParent(parent.transform);
        var image = gobj.AddComponent<Image>();
        image.type = Image.Type.Sliced;
        gobj.SetActive(false);
        var mainRect = gobj.GetComponent<RectTransform>();
        mainRect.anchorMin = new Vector2(0, 1 / 2f);
        mainRect.anchorMax = new Vector2(0, 1 / 2f);

        var field = gobj.AddComponent<SRInputField>();
        field.targetGraphic = image;
        field.descKey = $"archi_{desc}";

        var textGobj = new GameObject("SRInputField Text (custom)");
        textGobj.transform.SetParent(gobj.transform);

        var text = field.textComponent = textGobj.AddComponent<Text>();
        var rect = field.textComponent.GetComponent<RectTransform>();
        text.alignment = TextAnchor.LowerLeft;
        // text.resizeTextForBestFit = true;
        text.fontSize = 20;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1 / 2f, 1 / 2f);
        text.gameObject.transform.localPosition = new Vector3(30, 5, 5);
        var delta = rect.sizeDelta;
        rect.sizeDelta = new Vector2(delta.x, delta.y / 3f);

        gobj.AddComponent<FieldStyler>();
        gobj.SetActive(true);
        return field;
    }

    public static Button CreateButton(string text, GameObject parent, UnityAction onPressed, out TextMeshProUGUI label)
    {
        var bGobj = new GameObject("Button (custom)");
        bGobj.transform.SetParent(parent.transform);
        bGobj.AddComponent<CanvasRenderer>();
        var img = bGobj.AddComponent<Image>();
        img.type = Image.Type.Sliced;

        var button = bGobj.AddComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent();
        if (onPressed is not null)
        {
            button.onClick.AddListener(onPressed);
        }

        button.targetGraphic = img;

        var child = new GameObject("Button Text (custom)");
        var textObj = label = child.AddComponent<TextMeshProUGUI>();
        textObj.text = text;
        textObj.autoSizeTextContainer = true;
        child.transform.SetParent(bGobj.transform);

        bGobj.AddComponent<MeshButtonStyler>();
        return button;
    }

    public static Toggle CreateCheckbox(GameObject prefab, GameObject parent, bool setting, string name,
        UnityAction<bool> valueChanged)
    {
        var gameObject = UnityEngine.Object.Instantiate(prefab, parent.transform, false);
        var component = gameObject.GetComponent<Toggle>();
        component.isOn = setting;
        component.onValueChanged.AddListener(valueChanged);
        gameObject.transform.Find("Label").GetComponent<TMP_Text>().text = name;
        return component;
    }
    
    public static string HashPos(this Vector3 pos)
    {
        if (PosHashCache.TryGetValue(pos, out var hashPos)) return hashPos;
        return PosHashCache[pos] = $"[x:{Math.Floor(pos.x)}|y:{Math.Floor(pos.y)}|z:{Math.Floor(pos.z)}]";
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class PatchAll : Attribute;