using DG.Tweening;
using System;
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
    CombatAnnounce,
    PlayerTurnAnnounce,
    FadeOut,
    FadeIn,
}
public class UIWindow : MonoBehaviour
{
    [SerializeField] protected WindowType type;
    public WindowType Type => type;
    public Action<WindowType, WindowMode> ChangeWindow;

    protected SideButton[] sideButtions;
    protected virtual void Awake()
    {
        sideButtions = transform.GetComponentsInChildren<SideButton>();
    }

    protected virtual void OnEnable()
    {
        for (int i = 0; i < sideButtions.Length; i++)
        {
            sideButtions[i].Show();
        }
    }
}
public class UIMotionWindow : UIWindow
{
    protected Dictionary<MotionKey, Func<Sequence>> _handler = new Dictionary<MotionKey, Func<Sequence>>();
    public Sequence GetMotion(MotionKey type)
    {
        if (!_handler.TryGetValue(key: type, value: out Func<Sequence> value))
        {
            return null;
        }
        return value.Invoke();
    }
}
