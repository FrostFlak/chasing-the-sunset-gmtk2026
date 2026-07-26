using System;
using DG.Tweening;
using Helpers;
using UnityEngine;

namespace Gameplay.LevelSteps {
    [Serializable]
    public class EndScreenState : ILevelStep {
        
        [SerializeField] private CanvasGroup _endScreenUI;
        [SerializeField] private int _endScreenFadeDuration = 3;
        [SerializeField] private int _cutsceneNotebookScaleDuration = 3;
        [SerializeField] private float _cutsceneNotebookEndScale = 1.6f;
        [SerializeField] private float _waitToShowEndscreen = 3f;
        
        private LevelContext _levelContext;
        private Timer _timer;
        public event Action<ILevelStep, StepResult> OnStepResult;
        
        public void Initialize(LevelContext levelContext) {
            _levelContext = levelContext;
            _timer = new Timer(_levelContext.Player);
        }

        public void Enter() {
            _levelContext.CutsceneNotebook.root.gameObject.SetActive(true);
            _levelContext.CutsceneNotebook.DOScale(_cutsceneNotebookEndScale, _cutsceneNotebookScaleDuration).SetEase(Ease.InOutBack).OnComplete(ShowLabel);
        }

        private void ShowLabel() {
            AudioService.Instance.PlayPencilSfx();
            _levelContext.CutsceneNotebookLbl.gameObject.SetActive(true);
            _timer.Start(_waitToShowEndscreen, onComplete: ShowEndScreenUI);
        }

        private void ShowEndScreenUI() {
            _levelContext.CutsceneNotebook.root.gameObject.SetActive(false);
            
            _levelContext.FadeScreenUI.FadeIn(
                _endScreenFadeDuration,
                false,
                onComplete: () => _endScreenUI.DOFade(1, _endScreenFadeDuration)
            );
        }
    }
}