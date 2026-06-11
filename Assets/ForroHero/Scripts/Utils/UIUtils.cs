using DG.Tweening;
using UnityEngine;

namespace ForroHero.Scripts.Utils
{
    public static class UIUtils
    {
        private const float DOWN_SCALE = 0.9f;
        private const float UP_SCALE = 1f;
        private const float DOWN_DURATION = 0.1f;
        private const float UP_DURATION = 0.15f;

        public static void ButtonKeyDown(RectTransform button, ref Tween tween)
        {
            tween?.Kill();

            tween = button
                .DOScale(DOWN_SCALE, DOWN_DURATION)
                .SetEase(Ease.OutQuad);
        }

        public static void ButtonKeyUp(RectTransform button, ref Tween tween, System.Action onComplete = null)
        {
            tween?.Kill();

            tween = button
                .DOScale(UP_SCALE, UP_DURATION)
                .SetEase(Ease.OutBack)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}
