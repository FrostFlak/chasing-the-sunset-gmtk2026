using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Helpers.UI {
    public class FloatingText : MonoBehaviour {
        
        #region SerializedFields
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private CanvasGroup _canvasGroup;
        #endregion

        #region PrivateFields
        private RectTransform _rect;
        private Sequence _sequence;
        #endregion

        #region Behaviour
        private void Awake() => _rect = transform as RectTransform;

        private void OnDisable() => _sequence?.Kill();
        #endregion

        #region API
        public FloatingText Show(
            string text,
            float duration,
            Vector2 positionOffset = default,
            Action<FloatingText> onFinished = null
        ) {
            _text.text = text;
            _canvasGroup.SetAlpha(1f);
            
            _rect.anchoredPosition = positionOffset;

            _sequence?.Kill();
            _sequence = DOTween.Sequence()
                .AppendInterval(duration)
                .OnComplete(() => {
                    _canvasGroup.SetAlpha(0f);
                    onFinished?.Invoke(this);
                });

            return this;
        }

        public FloatingText FadeOut(float duration, Ease ease = Ease.OutBack) {
            _sequence.Join(_canvasGroup.DOFade(0f, duration).SetEase(ease));
            return this;
        }

        public FloatingText Scale(
            float duration,
            float endScale = 1.2f,
            Ease ease = Ease.OutBack
        ) {
            _sequence.Join(_rect.DOScale(endScale, duration).SetEase(ease));
            return this;
        }
        
        public FloatingText Move(
            float duration,
            Vector2 offset,
            Ease ease = Ease.OutBack
        ) {
            _sequence.Join(_rect.DOAnchorPos(_rect.anchoredPosition + offset, duration).SetEase(ease));
            return this;
        }
        #endregion
    }
}