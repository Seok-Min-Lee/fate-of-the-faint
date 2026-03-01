
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicWindow : UIWindow
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI flaverText;

    public void Bind(RelicSO relic)
    {
        name.text = relic.DisplayName;
        icon.sprite = relic.Icon;
        description.text = relic.Description;
        flaverText.text = relic.FlaverText;
    }
    public void OnClick()
    {
        ChangeWindow(WindowType.Relic, WindowMode.Revert);
    }
}
