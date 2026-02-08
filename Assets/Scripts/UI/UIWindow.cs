using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum MotionKey
{
    WindowShow,
    CombatAnnounce, 
    PlayerTurnAnnounce,
}
public class UIWindow : MonoBehaviour
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
