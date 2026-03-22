using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WindowType
{
    //Combat Scene
    Combat,
    Defeat,
    Victory,
    CardRewards,
    CardDisplay,
    Map,
    Setting,
    Relic,
    //Rest Scene
    RestMenu,
    EnhanceDisplay,
    EnhancePreview,
    RestComplete,
    //Shop
    Shop,
    CardRemove,
    //
    None
}
public enum WindowMode
{
    Single,
    Overlap,
    Revert,
}
public enum MotionKey
{
    WindowShow,
    CombatStarted,
    PlayerTurnStarted,
    EnemyTurnStarted,
}
public class UIWindow : MonoBehaviour
{
    [SerializeField] protected WindowType type;
    public WindowType Type => type;
    public Action<WindowType, WindowMode> ChangeWindow;

    protected SideButton[] sideButtons;
    protected virtual void Awake()
    {
        sideButtons = transform.GetComponentsInChildren<SideButton>();
    }

    protected virtual void OnEnable()
    {
        //if (sideButtons == null || sideButtons.Length == 0)
        //{
        //    return;
        //}

        //for (int i = 0; i < sideButtons.Length; i++)
        //{
        //    sideButtons[i].Show();
        //}
    }
}
public class UIMotionWindow : UIWindow
{
    protected Dictionary<MotionKey, Func<IEnumerator>> _handler = new Dictionary<MotionKey, Func<IEnumerator>>();
    public Func<IEnumerator> GetMotion(MotionKey type)
    {
        if (_handler.TryGetValue(key: type, value: out Func<IEnumerator> value))
        {
            return value;
        }

        return null;
    }
}
