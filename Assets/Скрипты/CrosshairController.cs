using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public static CrosshairController Instance;

    [Header("Crosshair Settings")]
    public bool EnableCrosshair = true;
    public Image CrosshairImage;
    public Sprite DefaultCrosshairSprite;
    public Sprite AimCrosshairSprite;
    public Sprite CanInteractSprite;

    private Vector3 _standartScale;
    private Color _standartColor;
    
    private bool _isAiming = false;
    private bool _isInteracting = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else 
        {
            Destroy(gameObject);
        }
        
        _standartScale = CrosshairImage.transform.localScale;
        _standartColor = CrosshairImage.color;
    }

    public void Enable()
    {
        if (CrosshairImage != null) CrosshairImage.enabled = true;
    }

    public void Disable()
    {
        if (CrosshairImage != null) CrosshairImage.enabled = false;
    }

    public void EnableAimMode()
    {
        if (_isAiming) return;
        _isAiming = true;
        UpdateVisuals();
    }

    public void DisableAimMode()
    {
        if (!_isAiming) return;
        _isAiming = false;
        UpdateVisuals();
    }
    
    public void UpdateCrosshair(bool isActive)
    {
        if (_isInteracting == isActive) return;
        _isInteracting = isActive;
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (CrosshairImage == null) return;

        CrosshairImage.transform.DOKill();
        CrosshairImage.DOKill();

        if (_isInteracting && _isAiming)
        {
            if (CanInteractSprite != null) CrosshairImage.sprite = CanInteractSprite;
            
            Color targetColor = _standartColor;
            targetColor.a = 1f;
            
            CrosshairImage.DOColor(targetColor, 0.35f).SetEase(Ease.OutSine);
            CrosshairImage.transform.DOScale(new Vector3(0.35f, 0.35f, 0.35f), 0.2f).SetEase(Ease.InOutSine);
        }
        else if (_isInteracting)
        {
            if (CanInteractSprite != null) CrosshairImage.sprite = CanInteractSprite;
            
            Color targetColor = _standartColor;
            targetColor.a = 1f;
            
            CrosshairImage.DOColor(targetColor, 0.35f).SetEase(Ease.OutSine);
            CrosshairImage.transform.DOScale(_standartScale, 0.2f).SetEase(Ease.InOutSine);
        }
        else if (_isAiming)
        {
            if (AimCrosshairSprite != null) CrosshairImage.sprite = AimCrosshairSprite;
            
            CrosshairImage.DOColor(_standartColor, 0.35f);
            CrosshairImage.transform.DOScale(new Vector3(0.35f, 0.35f, 0.35f), 0.2f).SetEase(Ease.InOutSine);
        }
        else
        {
            if (DefaultCrosshairSprite != null) CrosshairImage.sprite = DefaultCrosshairSprite;
            
            CrosshairImage.DOColor(_standartColor, 0.35f).SetEase(Ease.OutSine);
            CrosshairImage.transform.DOScale(_standartScale, 0.2f).SetEase(Ease.InOutSine);
        }
    }
}

