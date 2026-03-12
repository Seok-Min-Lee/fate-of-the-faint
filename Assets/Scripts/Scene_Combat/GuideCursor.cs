using UnityEngine;

public interface ITooltip 
{
    public void GetTooltip(out string name, out string description);
}

public class GuideCursor : RaycastCursor<ITooltip>
{
    [SerializeField] TooltipView view;
    private void Update()
    {
        ITooltip tooltip = RaycastTargetUnderCursor();

        if (tooltip == null)
        {
            view.Clear();
            return;
        }

        tooltip.GetTooltip(out string name, out string description);

        if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(description))
        {
            return;
        }

        view.Bind(name, description);
        view.transform.position = Input.mousePosition;
    }
}
