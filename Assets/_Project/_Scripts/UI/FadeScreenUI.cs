using System;
using DG.Tweening;
using Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class FadeScreenUI : MonoBehaviour {
        
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _continueButton;

        public event Action OnContinueButtonClick;
        private void Start() {
            _continueButton.onClick.AddListener(OnContinueClicked);
            _canvasGroup.SetStateNoAlpha(false);;
        }

        private void OnDestroy() {
            _continueButton.onClick.RemoveAllListeners();
        }

        public void Show() {
            _canvasGroup.DOFade(1, 0).From(0).SetEase(Ease.Linear).OnComplete(() => {
                _canvasGroup.SetStateNoAlpha(true);
            });
        }
        
        public void FadeIn(float duration, Action onComplete) {
            _canvasGroup.DOFade(1, duration).From(0).SetEase(Ease.Linear).OnComplete(() => {
                _canvasGroup.SetStateNoAlpha(true);
                onComplete?.Invoke();
            });
        }
        
        public void FadeOut(float duration, Action onComplete) {
            _canvasGroup.DOFade(0, duration).From(1).SetEase(Ease.Linear).OnComplete(() => {
                _canvasGroup.SetStateNoAlpha(false);
                onComplete?.Invoke();
            });
        }
        
        private void OnContinueClicked() => OnContinueButtonClick?.Invoke();
    }
}