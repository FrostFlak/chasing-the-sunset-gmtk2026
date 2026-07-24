using System;
using Helpers;
using UnityEngine;

namespace Gameplay.LevelSteps {
    [Serializable]
    public class FadeScreenStep : ILevelStep {

        [SerializeField] private float _fadeDuration;
        [SerializeField] private bool _fadeIn;
        
        private LevelContext _levelContext;
        private Timer _fadeTimer;

        public event Action<ILevelStep> OnCompleted;
        
        public void Initialize(LevelContext levelContext) => _levelContext =  levelContext;

        public void Enter() {
            if (_fadeIn)
                _levelContext.FadeScreenUI.FadeIn(_fadeDuration);
            else
                _levelContext.FadeScreenUI.FadeOut(_fadeDuration);
            
            _fadeTimer = new Timer(_levelContext.QuestsService);
            _fadeTimer.Start(_fadeDuration, onComplete: OnTimerEnd);
        }

        private void OnTimerEnd() => OnCompleted?.Invoke(this);
    }
}