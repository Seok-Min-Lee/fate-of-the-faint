using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum WindowType
{
    Combat,
    Defeat,
    Victory,
    CardRewards,
    CardDisplay,
    Map,
    Setting,
    Relic,
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
