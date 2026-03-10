using System.Collections.Generic;
using UnityEngine;

public class UIWindowManager : MonoBehaviour
{
    [SerializeField] private Transform windowParent;
    [SerializeField] WindowType startWindowSource;
    [SerializeField] WindowMode startWindowMode;

    private Dictionary<WindowType, UIWindow> windowDictionary = new Dictionary<WindowType, UIWindow>();
    private Stack<HashSet<WindowType>> windowSnapshot = new Stack<HashSet<WindowType>>();
    private void Awake()
    {
        foreach (UIWindow window in windowParent.GetComponentsInChildren<UIWindow>())
        {
            if (!windowDictionary.ContainsKey(window.Type))
            {
                windowDictionary.Add(window.Type, window);
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
        if (windowDictionary.TryGetValue(type, out window))
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
            if (windowSnapshot.TryPop(out HashSet<WindowType> snapshot))
            {
                foreach (KeyValuePair<WindowType, UIWindow> kvp in windowDictionary)
                {
                    kvp.Value.gameObject.SetActive(snapshot.Contains(kvp.Key));
                }
            }
            return;
        }

        // 현재 상태 스냅샷
        HashSet<WindowType> currentActiveWindows = new HashSet<WindowType>();
        foreach (KeyValuePair<WindowType, UIWindow> kvp in windowDictionary)
        {
            if (kvp.Value.gameObject.activeSelf)
            {
                currentActiveWindows.Add(kvp.Key);
            }
        }
        windowSnapshot.Push(currentActiveWindows);

        // 윈도우 변경
        if (source == WindowType.None || mode == WindowMode.Single)
        {
            foreach (UIWindow w in windowDictionary.Values)
            {
                w.gameObject.SetActive(false);
            }
        }

        if (windowDictionary.TryGetValue(source, out UIWindow targetWindow))
        {
            targetWindow.gameObject.SetActive(true);
        }
        //UIWindow window;
        //HashSet<WindowType> snapshot;

        //if (source == WindowType.None)
        //{
        //    foreach (UIWindow w in windowDictionary.Values)
        //    {
        //        w.gameObject.SetActive(false);
        //    }
        //}

        //if (mode == WindowMode.Revert)
        //{
        //    if (windowSnapshot.Count > 0)
        //    {
        //        snapshot = windowSnapshot.Pop();
        //        foreach (KeyValuePair<WindowType, UIWindow> kvp in windowDictionary)
        //        {
        //            kvp.Value.gameObject.SetActive(snapshot.Contains(kvp.Key));
        //        }
        //    }
        //}
        //else
        //{
        //    snapshot = new HashSet<WindowType>();
        //    foreach (KeyValuePair<WindowType, UIWindow> kvp in windowDictionary)
        //    {
        //        if (kvp.Value.gameObject.activeSelf)
        //        {
        //            snapshot.Add(kvp.Key);
        //        }
        //    }
        //    windowSnapshot.Push(snapshot);

        //    if (windowDictionary.TryGetValue(source, out window))
        //    {
        //        if (mode == WindowMode.Single)
        //        {
        //            foreach (UIWindow w in windowDictionary.Values)
        //            {
        //                w.gameObject.SetActive(false);
        //            }
        //        }

        //        window.gameObject.SetActive(true);
        //    }
        //}
    }
}
