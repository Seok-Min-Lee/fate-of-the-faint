using TMPro;
using UnityEngine;

public class GoldMonoSystem : BaseMonoSystem
{
    [SerializeField] private TextMeshProUGUI goldText;

    private EventBus eventBus;
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;
        goldText.text = PlayManager.Instance.CurrentData.Gold.ToString();
    }
    public void Add(int amount)
    {
        int startAmount = PlayManager.Instance.CurrentData.Gold;
        PlayManager.Instance.CurrentData.AddGold(amount);

        int endAmount = PlayManager.Instance.CurrentData.Gold;
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
        int startAmount = PlayManager.Instance.CurrentData.Gold;
        PlayManager.Instance.CurrentData.SubtractGold(amount);

        int endAmount = PlayManager.Instance.CurrentData.Gold;
        goldText.text = endAmount.ToString();

        eventBus.Publish<GoldChanged>(new GoldChanged(
            context: new EventContext(this, null, null, null),
            motion: null,
            startAmount: startAmount,
            endAmount: endAmount
        ));
    }
}