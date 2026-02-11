using UnityEngine;
using UnityEngine.UI;

public class CardArt : MonoBehaviour
{
    [SerializeField] private Image image;

    public void Activate(Sprite sprite)
    {
        image.sprite = sprite;
        gameObject.SetActive(true);
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
