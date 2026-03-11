using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntentView : MonoBehaviour, ITooltip
{
    [SerializeField] private Image symbol;
    [SerializeField] private TextMeshProUGUI text;

    private CanvasGroup canvasGroup;
    private IntentType type;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public Sequence Show(EnemyActionSO intent)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            type = intent.IntentType;
            symbol.sprite = intent.IntentIcon;
            text.text = intent.Effects[0].Value.ToString();

            canvasGroup.alpha = 0f;
            gameObject.SetActive(true);
        });
        sequence.Append(canvasGroup.DOFade(1f, 0.5f));

        return sequence;
    }
    public Sequence Hide()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(canvasGroup.DOFade(0f, 0.5f));
        sequence.AppendCallback(() => { gameObject.SetActive(false); });

        return sequence;
    }

    public void GetTooltip(out string name, out string description)
    {
        switch (type)
        {
            case IntentType.Attack:
                name = "공격";
                description = "공격을 준비하고 있습니다";
                break;
            case IntentType.AttackBlock:
                name = "공격 & 방어";
                description = "공격과 방어를 준비하고 있습니다";
                break;
            case IntentType.Block:
                name = "방어";
                description = "방어를 준비하고 있습니다";
                break;
            case IntentType.Buff:
                name = "스킬";
                description = "몬스터에게 이로운 효과를 준비하고 있습니다";
                break;
            case IntentType.Debuff:
                name = "스킬";
                description = "플레이어에게 해로운 효과를 준비하고 있습니다";
                break;
            default:
                name = string.Empty;
                description = string.Empty;
                break;
        }
    }
}
