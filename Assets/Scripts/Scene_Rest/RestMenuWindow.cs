using TMPro;
using UnityEngine;

public class RestMenuWindow : UIWindow
{
    [SerializeField] private RestCompleteWindow completeWindow;
    [SerializeField] private TextMeshProUGUI tipText;
    protected override void OnEnable()
    {
        Utils.TMPDOText(tipText, 2f);
    }
    public void OnClickRest()
    {
        int heal = (int)(RunManager.Instance.CurrentData.MaxHp * 0.3f);

        RunManager.Instance.CurrentData.SetHp(
            RunManager.Instance.CurrentData.CurrentHp + heal,
            RunManager.Instance.CurrentData.MaxHp
        );

        completeWindow.CompleteHeal();
        ChangeWindow(WindowType.RestComplete, WindowMode.Single);
    }
    public void OnClickEnhance()
    {
        ChangeWindow(WindowType.EnhanceDisplay, WindowMode.Single);
    }
}
