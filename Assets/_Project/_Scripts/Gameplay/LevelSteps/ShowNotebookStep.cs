using System;

namespace Gameplay.LevelSteps {
    [Serializable]
    public class ShowNotebookStep : ILevelStep {

        private LevelContext _levelContext;
        public event Action<ILevelStep> OnCompleted;
        public void Initialize(LevelContext levelContext) => _levelContext =  levelContext;

        public void Enter() {
            _levelContext.NotebookView.Show();
            _levelContext.NotebookView.OnHide += OnHideNotebook;
        }

        private void OnHideNotebook() {
            _levelContext.NotebookView.OnHide -= OnHideNotebook;
            OnCompleted?.Invoke(this);
        }
    }
}