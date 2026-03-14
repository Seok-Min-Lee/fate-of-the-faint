using TMPro;
using UnityEngine;

public class HpMonoSystem : BaseMonoSystem
{
    [SerializeField] private TextMeshProUGUI text;

    private EventBus eventBus;
    public void Start()
    {
        Refresh();
    }
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }
    public void Refresh()
    {
        SetHpText(RunManager.Instance.CurrentData.CurrentHp, RunManager.Instance.CurrentData.MaxHp);
    }
    public void OnActionEnded(ActionEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        SetHpText(e.Context.Combat.Player.CurrentHp, e.Context.Combat.Player.MaxHp);
    }
    private void SetHpText(int currentHp, int maxHp)
    {
        text.text = $"{currentHp.ToString()}/{maxHp.ToString()}";
    }
}
