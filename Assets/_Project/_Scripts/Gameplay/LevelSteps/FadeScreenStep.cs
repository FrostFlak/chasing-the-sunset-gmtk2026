using System;
using UnityEngine;

namespace Gameplay.LevelSteps {
    [Serializable]
    public class FadeScreenStep : ILevelStep {
        
        [SerializeField] private float _fadeDuration;
        [SerializeField] private bool _fadeIn;
        [SerializeField] private bool _requireContinueClick;
        
        private LevelContext _levelContext;

        public event Action<ILevelStep, StepResult> OnStepResult;
        
        public void Initialize(LevelContext levelContext) => _levelContext = levelContext;

        public void Enter() {
            _levelContext.FadeScreenUI.OnContinueButtonClick += OnFadeScreenBtnClicked;
            
            if (_fadeIn)
                _levelContext.FadeScreenUI.FadeIn(_fadeDuration, _requireContinueClick, onComplete: OnFadeComplete);
            else
                _levelContext.FadeScreenUI.FadeOut(_fadeDuration, _requireContinueClick, onComplete: OnFadeComplete);
        }

        private void OnFadeScreenBtnClicked() {
            if (!_requireContinueClick)
                return;
            
            _levelContext.FadeScreenUI.OnContinueButtonClick -= OnFadeScreenBtnClicked;
            OnStepResult?.Invoke(this, StepResult.Success);
        }

        private void OnFadeComplete() {
            if (!_requireContinueClick)
                OnStepResult?.Invoke(this, StepResult.Success);
        }
    }
}