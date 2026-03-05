
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicWindow : UIWindow
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI flaverText;

    public void Bind(RelicView relic)
    {
        RelicSO data = relic.Instance.Origin;

        name.text = data.DisplayName;
        icon.sprite = data.Icon;
        description.text = data.Description;
        flaverText.text = data.FlaverText;
    }
    public void OnClick()
    {
        ChangeWindow(WindowType.Relic, WindowMode.Revert);
    }
}
