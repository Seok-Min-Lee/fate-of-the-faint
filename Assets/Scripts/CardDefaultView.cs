using TMPro;
using UnityEngine;

public class CardDefaultView : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI cost;
    [SerializeField] protected TextMeshProUGUI name;
    [SerializeField] protected TextMeshProUGUI desc;
    [SerializeField] protected CardArt[] arts;

    public TextMeshProUGUI Cost => cost;
    public TextMeshProUGUI Name => name;
    public TextMeshProUGUI Desc => desc;

    public CardSO Origin { get; private set; }
    protected void Init(CardSO data)
    {
        Origin = data;

        cost.text = data.Cost.ToString();
        name.text = data.Name;
        desc.text = data.Description;

        int typeIndex = data switch
        {
            AttackCardSO => 0,
            SkillCardSO => 1,
            _ => 2
        };

        for (int i = 0; i < arts.Length; i++)
        {
            if (i == typeIndex)
            {
                arts[i].Activate(data.Image);
            }
            else
            {
                arts[i].Deactivate();
            }
        }
    }
}
