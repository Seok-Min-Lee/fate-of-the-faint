using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicSimplePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI name;
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

    public void Bind(string name, string description, Transform viewTransform)
    {
        this.name.text = name;
        this.description.text = description;

        transform.position = viewTransform.position;
        transform.parent = viewTransform.parent.parent;

        gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
    }
    public void Clear()
    {
        gameObject.SetActive(false);

        name.text = "";
        description.text = "";
    }
}
