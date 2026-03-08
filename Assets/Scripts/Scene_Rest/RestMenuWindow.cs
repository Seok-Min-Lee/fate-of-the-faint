using System;
using UnityEngine;

public class RestMenuWindow : UIWindow
{
    [SerializeField] private RestCompleteWindow completeWindow;
    public void OnClickRest()
    {
        int heal = (int)(PlayManager.Instance.CurrentData.MaxHp * 0.3f);

        PlayManager.Instance.CurrentData.SetHp(
            PlayManager.Instance.CurrentData.CurrentHp + heal,
            PlayManager.Instance.CurrentData.MaxHp
        );

        completeWindow.CompleteHeal();
        ChangeWindow(WindowType.RestComplete, WindowMode.Single);
    }
    public void OnClickEnhance()
    {
        ChangeWindow(WindowType.EnhanceDisplay, WindowMode.Single);
    }
}
