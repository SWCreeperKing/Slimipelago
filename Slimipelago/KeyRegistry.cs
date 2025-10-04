using JetBrains.Annotations;
using UnityEngine;

namespace Slimipelago;

public static class KeyRegistry
{
    private static Dictionary<KeyCode, bool> Previous = [];
    [CanBeNull] public static event Action<KeyCode> OnKeyPressed;
    
    public static void Update()
    {
        if (!Previous.Any()) return;
        foreach (var key in Previous.Keys.ToArray())
        {
            var isKey = Input.GetKey(key);
            if (isKey == Previous[key]) continue;
            if (isKey)
            {
                try
                {
                    OnKeyPressed?.Invoke(key);
                }
                catch (Exception e)
                {
                    Core.Log.Msg($"Error when running action on [{key}]");
                    Core.Log.Error(e);
                }
            }
            Previous[key] = isKey;
        }
    }

    public static void AddKey(KeyCode key, Action action)
    {
        Previous[key] = false;
        OnKeyPressed += keycode =>
        {
            if (keycode != key) return;
            action();
        };
        Core.Log.Msg($"Added Debug keybind for: [{key}]");
    }
}