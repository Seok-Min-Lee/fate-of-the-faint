using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;
public interface IShopView
{
    public int Price { get; }
    public Tween SuccessedToPurchase();
    public Tween FailedToPurchase();
}
public class PurchasePopup : MonoBehaviour
{
    [SerializeField] private PurchaseOption[] options;

    private Action onSubmit;
    public void Init(Action onSubmit)
    {
        this.onSubmit = onSubmit;
        transform.position = Input.mousePosition;

        for (int i = 0; i < options.Length; i++)
        {
            options[i].Init();
        }

        gameObject.SetActive(true);
    }
    public void OnClickSubmit()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        onSubmit?.Invoke();
        Reset();
    }
    public void OnClickCancel()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        Reset();
    }
    private void Reset()
    {
        onSubmit = null;
        gameObject.SetActive(false);
    }
}
