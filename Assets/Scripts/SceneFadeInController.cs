using DG.Tweening;
using UnityEngine;

public class SceneFadeInController : MonoBehaviour
{
    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [SerializeField, Min(0f)] private float _fadeDuration = 0.6f;
    [SerializeField] private Ease _fadeEase = Ease.InOutQuad;

    private void Start()
    {
        if (_fadeCanvasGroup == null)
        {
            return;
        }

        _fadeCanvasGroup.alpha = 1f;
        _fadeCanvasGroup.blocksRaycasts = true;

        _fadeCanvasGroup
            .DOFade(0f, _fadeDuration)
            .SetEase(_fadeEase)
            .OnComplete(() =>
            {
                _fadeCanvasGroup.blocksRaycasts = false;
            });
    }
}