using TMPro;
using UnityEngine;

public class GoldMonoSystem : BaseMonoSystem
{
    [SerializeField] private TextMeshProUGUI goldText;

    private EventBus eventBus;
    public void Start()
    {
        Refresh();
    }
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }
    public void Add(int amount)
    {
        int startAmount = RunManager.Instance.CurrentData.Gold;
        RunManager.Instance.CurrentData.AddGold(amount);

        int endAmount = RunManager.Instance.CurrentData.Gold;
        goldText.text = endAmount.ToString();

        eventBus.Publish<GoldChanged>(new GoldChanged(
            context: new EventContext(this, null, null, null),
            motion: null,
            startAmount: startAmount,
            endAmount: endAmount
        ));
    }
    public void Substract(int amount)
    {
        int startAmount = RunManager.Instance.CurrentData.Gold;
        RunManager.Instance.CurrentData.SubtractGold(amount);

        int endAmount = RunManager.Instance.CurrentData.Gold;
        goldText.text = endAmount.ToString();

        eventBus.Publish<GoldChanged>(new GoldChanged(
            context: new EventContext(this, null, null, null),
            motion: null,
            startAmount: startAmount,
            endAmount: endAmount
        ));
    }
    public void Refresh()
    {
        goldText.text = RunManager.Instance.CurrentData.Gold.ToString();
    }
}