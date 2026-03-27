using UnityEngine;

/// <summary>
/// 여러 씬 컨트롤러가 공통으로 참조하는 상단 헤더 메뉴 모달 기능을 통합 관리하는 부모 클래스
/// </summary>
public abstract class BaseSceneCtrl : MonoBehaviour
{
    [SerializeField] protected UIWindowManager windowManager;

    /// <summary>
    /// 덱 카드 목록 뷰 UI 모달 호출
    /// </summary>
    public virtual void OnClickCardDisplay()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);
        windowManager.ActivateWindow(WindowType.CardDisplay, WindowMode.Single);
    }

    /// <summary>
    /// 진행 맵 뷰 UI 모달 호출
    /// </summary>
    public virtual void OnClickMap()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);
        windowManager.ActivateWindow(WindowType.Map, WindowMode.Single);
    }

    /// <summary>
    /// 세팅 뷰 UI 모달 호출
    /// </summary>
    public virtual void OnClickSetting()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);
        windowManager.ActivateWindow(WindowType.Setting, WindowMode.Single);
    }
}
