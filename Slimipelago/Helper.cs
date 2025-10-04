using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

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
}

[AttributeUsage(AttributeTargets.Class)]
public class HarmonyPatchAll : Attribute;