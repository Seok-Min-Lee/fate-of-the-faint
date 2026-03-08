using DG.Tweening;
using UnityEngine;

public class EnhanceDisplayWindow : CardDisplayWindow
{
    [SerializeField] private EnhancePreviewWindow previewWindow;

    [Header("[Fail React]")]
    [SerializeField] private int strength;
    [SerializeField] private float duration;
    [SerializeField] private int vibrato;
    protected override void OnEnable()
    {
        base.OnEnable();

        foreach (CardDisplayView view in views)
        {
            view.BindOnClickListener(() => OnClickView(view));
        }
    }
    private void OnClickView(CardDisplayView view)
    {
        if (view.Origin.UpgradeCard == null)
        {
            view.RectTransform.DOPunchPosition(
                punch: Random.insideUnitCircle * strength, 
                duration: duration, 
                vibrato: vibrato
            );

            return;
        }

        previewWindow.Bind(
            before: PlayManager.Instance.CurrentData.Cards[view.Index], 
            after: view.Origin.UpgradeCard
        );

        ChangeWindow(WindowType.EnhancePreview, WindowMode.Single);
    }
}
