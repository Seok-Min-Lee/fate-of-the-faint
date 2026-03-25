using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image eventIcon;
    [SerializeField] private Image visitedIcon;
    public RectTransform RectTransform { get; private set; }
    private Button button;
    public MapNode Node { get; private set; }

    private Sprite normal;
    private Sprite hover;

    public int id;
    public int floor;
    public int col;

    private Func<MapNode, bool> checkProcess;
    private Action<MapNode> successProcess;
    private Sequence sequence;
    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
    }
    public void Init(MapNode node, Sprite normal, Sprite hover, bool isVisible)
    {
        this.Node = node;
        this.normal = normal;
        this.hover = hover;

        eventIcon.sprite = normal;
        eventIcon.raycastTarget = true;
        if (node.State == MapNodeState.Visited)
        {
            visitedIcon.gameObject.SetActive(true);
            visitedIcon.fillAmount = 1;
            visitedIcon.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));
        }
        else
        {
            visitedIcon.gameObject.SetActive(false);
            visitedIcon.fillAmount = 0;
        }

        button.interactable = true;

        gameObject.SetActive(isVisible);

        //
        id = node.Id;
        floor = node.Floor;
        col = node.Col;
    }
    public void Bind(Func<MapNode, bool> checkProcess, Action<MapNode> successProcess)
    {
        this.checkProcess = checkProcess;
        this.successProcess = successProcess;
    }
    public void Highlight()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();
        Vector3 start = Vector3.one * 0.9f;
        Vector3 end = Vector3.one * 1.5f;

        sequence.AppendCallback(() =>
        {
            transform.localScale = start;
        });
        sequence.Append(transform.DOScale(end, 1f).SetLoops(-1, LoopType.Yoyo));
    }
    public void Hide()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        button.interactable = false;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Node.State == MapNodeState.None)
        {
            eventIcon.sprite = hover;
            RectTransform.localScale = Vector3.one * 1.25f;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        eventIcon.sprite = normal;
        RectTransform.localScale = Vector3.one;
    }
    public void OnClick()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        if (checkProcess != null && checkProcess.Invoke(Node))
        {
            Sequence sequence = DOTween.Sequence();

            sequence.AppendCallback(() =>
            {
                visitedIcon.gameObject.SetActive(true);
                visitedIcon.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));
            });

            sequence.Append(visitedIcon.DOFillAmount(1f, 0.25f));

            sequence.AppendCallback(() =>
            {
                Hide();
                eventIcon.raycastTarget = false;

                successProcess?.Invoke(Node);
            });
        }
    }
}
