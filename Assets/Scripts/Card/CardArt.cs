using UnityEngine;
using UnityEngine.UI;

public class CardArt : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Image frame;
    [SerializeField] private Image typeTag;

    [SerializeField] private Color commonColor;
    [SerializeField] private Color uncommonColor;
    [SerializeField] private Color rareColor;
    public void Activate(Sprite sprite, CardRarity rarity)
    {
        image.sprite = sprite;

        switch (rarity)
        {
            case CardRarity.Common:
                frame.color = commonColor;
                typeTag.color = commonColor;
                break;
            case CardRarity.Uncommon:
                frame.color = uncommonColor;
                typeTag.color = uncommonColor;
                break;
            case CardRarity.Rare:
                frame.color = rareColor;
                typeTag.color = rareColor;
                break;
        }

        gameObject.SetActive(true);
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
