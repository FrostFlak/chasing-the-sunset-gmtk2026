using System;
using DG.Tweening;
using Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class FadeScreenUI : MonoBehaviour {
        
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TMP_Text _continueLbl;

        public event Action OnContinueButtonClick;
        private void Start() {
            _continueButton.onClick.AddListener(OnContinueClicked);
            _canvasGroup.SetStateNoAlpha(false);;
        }

        private void OnDestroy() {
            _continueButton.onClick.RemoveAllListeners();
        }

        public void FadeIn(float duration, bool requireContinueClick, Action onComplete = null) {
            _continueLbl.gameObject.SetActive(requireContinueClick);
            _canvasGroup.DOFade(1, duration).From(0).SetEase(Ease.Linear).OnComplete(() => {
                _canvasGroup.SetStateNoAlpha(true);
                onComplete?.Invoke();
            });
        }
        
        public void FadeOut(float duration, bool requireContinueClick, Action onComplete = null) {
            _continueLbl.gameObject.SetActive(requireContinueClick);
            _canvasGroup.DOFade(0, duration).From(1).SetEase(Ease.Linear).OnComplete(() => {
                _canvasGroup.SetStateNoAlpha(false);
                onComplete?.Invoke();
            });
        }
        
        private void OnContinueClicked() => OnContinueButtonClick?.Invoke();
    }
}