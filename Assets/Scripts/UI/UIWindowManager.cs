using System.Collections.Generic;
using UnityEngine;

public class UIWindowManager : MonoBehaviour
{
    [SerializeField] private Transform windowParent;
    [SerializeField] WindowType startWindowSource;
    [SerializeField] WindowMode startWindowMode;

    private Dictionary<WindowType, UIWindow> dictionary = new Dictionary<WindowType, UIWindow>();
    private Stack<HashSet<WindowType>> snapshots = new Stack<HashSet<WindowType>>();
    private void Awake()
    {
        foreach (UIWindow window in windowParent.GetComponentsInChildren<UIWindow>())
        {
            if (!dictionary.ContainsKey(window.Type))
            {
                dictionary.Add(window.Type, window);
                window.ChangeWindow = ChangeWindow;
            }
        }

        ChangeWindow(startWindowSource, startWindowMode);
    }
    public void ActivateWindow(WindowType source, WindowMode mode)
    {
        if (!TryGetWindow(source, out UIWindow window) ||
            window.gameObject.activeSelf)
        {
            return;
        }

        ChangeWindow(source, mode);
    }
    public bool TryGetWindow(WindowType type, out UIWindow window)
    {
        if (dictionary.TryGetValue(type, out window))
        {
            return true;
        }

        return false;
    }
    private void ChangeWindow(WindowType source, WindowMode mode)
    {
        // Revert
        if (mode == WindowMode.Revert)
        {
            if (snapshots.TryPop(out HashSet<WindowType> snapshot))
            {
                foreach (KeyValuePair<WindowType, UIWindow> kvp in dictionary)
                {
                    kvp.Value.gameObject.SetActive(snapshot.Contains(kvp.Key));
                }
            }
            return;
        }

        // 현재 상태 스냅샷
        HashSet<WindowType> currentActiveWindows = new HashSet<WindowType>();
        foreach (KeyValuePair<WindowType, UIWindow> kvp in dictionary)
        {
            if (kvp.Value.gameObject.activeSelf)
            {
                currentActiveWindows.Add(kvp.Key);
            }
        }
        snapshots.Push(currentActiveWindows);

        // 윈도우 변경
        if (source == WindowType.None || mode == WindowMode.Single)
        {
            foreach (UIWindow w in dictionary.Values)
            {
                w.gameObject.SetActive(false);
            }
        }

        if (dictionary.TryGetValue(source, out UIWindow targetWindow))
        {
            targetWindow.gameObject.SetActive(true);
        }
    }
}
