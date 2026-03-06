using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class RestCtrl : MonoBehaviour
{
    [SerializeField] private CanvasGroup dimmedCG;

    [SerializeField] private Transform windowParent;
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
    }
    private void Start()
    {
        dimmedCG.alpha = 1f;
        dimmedCG.DOFade(0.75f, 2.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);

        ChangeWindow(WindowType.RestMenu, WindowMode.Single);
    }
    private void ChangeWindow(WindowType source, WindowMode mode)
    {
        UIWindow window;
        HashSet<WindowType> snapshot;
        if (mode == WindowMode.Revert)
        {
            if (windowSnapshot.Count > 0)
            {
                snapshot = windowSnapshot.Pop();
                foreach (KeyValuePair<WindowType, UIWindow> kvp in windowDictionary)
                {
                    kvp.Value.gameObject.SetActive(snapshot.Contains(kvp.Key));
                }
            }
        }
        else
        {
            snapshot = new HashSet<WindowType>();
            foreach (KeyValuePair<WindowType, UIWindow> kvp in windowDictionary)
            {
                if (kvp.Value.gameObject.activeSelf)
                {
                    snapshot.Add(kvp.Key);
                }
            }
            windowSnapshot.Push(snapshot);

            if (windowDictionary.TryGetValue(source, out window))
            {
                if (mode == WindowMode.Single)
                {
                    foreach (UIWindow w in windowDictionary.Values)
                    {
                        w.gameObject.SetActive(false);
                    }
                }

                window.gameObject.SetActive(true);
            }
        }
    }
}
