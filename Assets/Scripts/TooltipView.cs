using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI head;
    [SerializeField] private TextMeshProUGUI description;
    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            return _rectTransform;
        }
    }
    private RectTransform _rectTransform;

    public void Bind(string name, string description)
    {
        this.head.text = name;
        this.description.text = description;

        gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
    }
    public void Clear()
    {
        gameObject.SetActive(false);

        head.text = "";
        description.text = "";
    }
}
