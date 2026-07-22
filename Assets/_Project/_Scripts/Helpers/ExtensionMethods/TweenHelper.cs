using System;
using System.Globalization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers.ExtMethods {
    public static class TweenHelper {

        #region RectTransforms
        public static Tween Pop(
            this RectTransform rect,
            bool open,
            float duration = 0.3f,
            Action onComplete = null,
            Action onKill = null
        ) {
            return rect
                .DOScale(open ? 1f : 0f, duration)
                .From(open ? 0f : 1f)
                .SetEase(open ? Ease.OutBack : Ease.InBack)
                .OnComplete(() => onComplete?.Invoke())
                .OnKill(() => onKill?.Invoke());
        }
        
        public static Tween Slide(
            this RectTransform rect,
            Vector2 finalPosition,
            float duration = 0.3f,
            Action onComplete = null,
            Action onKill = null
        ) {
            return rect
                .DOAnchorPos(finalPosition, duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => onComplete?.Invoke())
                .OnKill(() => onKill?.Invoke());
        }
        
        public static Tween Punch(
            this RectTransform rect,
            float duration = 0.5f,
            float multiplySize = 0.2f,
            float elasticity = 0.8f
        ) {
            rect.DOKill();
            rect.localScale = Vector3.one;
            
            return rect.DOPunchScale(
                Vector3.one * multiplySize,
                duration,
                vibrato: 6,
                elasticity
            );
        }
        #endregion

        #region Graphic
        public static Tween Fade(
            this Graphic graphic,
            bool fadeIn,
            float duration = 0.3f,
            Action onComplete = null,
            Action onKill = null
        ) {
            graphic.DOKill();
            
            return graphic
                .DOFade(fadeIn ? 1f : 0f, duration)
                .From(fadeIn ? 0f : 1f)
                .OnComplete(() => onComplete?.Invoke())
                .OnKill(() => onKill?.Invoke());
        }
        
        public static Tween Fade(
            this CanvasGroup canvasGroup,
            bool fadeIn,
            float duration = 0.3f,
            Action onComplete = null,
            Action onKill = null
        ) {
            return canvasGroup
                .DOFade(fadeIn ? 1f : 0f, duration)
                .From(fadeIn ? 0f : 1f)
                .OnComplete(() => onComplete?.Invoke())
                .OnKill(() => onKill?.Invoke());
        }
        
        public static Tween Lerp(
            this Slider slider,
            float endValue,
            float duration = 0.3f,
            Action onComplete = null,
            Action onKill = null
        ) {
            return slider
                .DOValue(endValue, duration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => onComplete?.Invoke())
                .OnKill(() => onKill?.Invoke());
        }
        
        public static Tween Lerp(
            this Image image,
            float endValue,
            float duration = 0.3f,
            Action onComplete = null,
            Action onKill = null
        ) {
            return image
                .DOFillAmount(endValue, duration)
                .From(image.fillAmount == 0 ? 1 : 0)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => onComplete?.Invoke())
                .OnKill(() => onKill?.Invoke());
        }
        
        public static Tween FakeLoading(
            this Image image,
            float duration = 0.3f
        ) {
            DOTween.Kill(image);
            
            return DOTween.Sequence()
                .Append(image.transform.DORotate(new Vector3(0, 0, image.transform.eulerAngles.z - 360), duration, RotateMode.FastBeyond360))
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Incremental)
                .SetId(image);
        }
        #endregion

        #region Labels
        public static Tween FakeLoading(
            this TMPro.TMP_Text tmpText,
            string loadingText,
            bool returnToInitialText = true
        ) {
            var initialText = tmpText.text;
            
            DOTween.Kill(tmpText);

            return DOTween.Sequence()
                .SetId(tmpText)
                .AppendCallback(() => tmpText.SetText($"{loadingText}.")).AppendInterval(0.3f)
                .AppendCallback(() => tmpText.SetText($"{loadingText}..")).AppendInterval(0.3f)
                .AppendCallback(() => tmpText.SetText($"{loadingText}...")).AppendInterval(0.3f)
                .SetLoops(-1, LoopType.Restart).OnKill(() => {
                    if (returnToInitialText)
                        tmpText.SetText(initialText);
                });
        }
        
        public static Tween LerpLabelCount(
            this TMPro.TMP_Text label,
            int fromValue,
            int toValue,
            float duration = 0.5f
        ) {
            return DOTween.To(
                getter: () => fromValue,
                setter: x => {
                    fromValue = x;
                    label.SetText(fromValue.ToString("N0", CultureInfo.InvariantCulture));
                },
                toValue,
                duration
            ).SetEase(Ease.OutQuad);
        }
        #endregion
    }
}