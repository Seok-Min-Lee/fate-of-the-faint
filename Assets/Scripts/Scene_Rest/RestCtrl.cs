using DG.Tweening;
using UnityEngine;

public class RestCtrl : BaseSceneCtrl
{
    [SerializeField] private CanvasGroup dimmedCG;

    private void Start()
    {
        AudioManager.Instance.PlayBGM(SoundKey.NormalBGM);

        dimmedCG.alpha = 1f;
        dimmedCG.DOFade(0.75f, 2.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }
}
