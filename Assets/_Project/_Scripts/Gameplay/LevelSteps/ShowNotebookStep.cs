using System;
using UnityEngine;

namespace Gameplay.LevelSteps {
    [Serializable]
    public class ShowNotebookStep : ILevelStep {

        [SerializeField] private int _page;
        private LevelContext _levelContext;
        public event Action<ILevelStep, StepResult> OnStepResult;
        public void Initialize(LevelContext levelContext) => _levelContext =  levelContext;

        public void Enter() {
            _levelContext.NotebookView.Show(_page);
            _levelContext.NotebookView.OnHide += OnHideNotebook;
        }

        private void OnHideNotebook() {
            _levelContext.NotebookView.OnHide -= OnHideNotebook;
            OnStepResult?.Invoke(this, StepResult.Success);
        }
    }
}