using System;
using Helpers;
using Quests;
using UnityEngine;

namespace Gameplay.LevelSteps {
    [Serializable]
    public class QuestStep : ILevelStep {

        [SerializeField] private Quest _quest;
        private LevelContext _levelContext;

        public event Action<ILevelStep, StepResult> OnStepResult;

        public void Initialize(LevelContext levelContext) => _levelContext = levelContext;

        public void Enter() {
            _levelContext.QuestsService.Run(_quest);
            _quest.OnCompleted += OnQuestCompleted;
            _quest.OnFailed += OnQuestExpired;
        }
        
        private void OnQuestCompleted(Quest quest) {
            _quest.OnCompleted -= OnQuestCompleted;
            _quest.OnFailed -= OnQuestExpired;
            
            OnStepResult?.Invoke(this, StepResult.Success);
        }
        
        private void OnQuestExpired(Quest quest) {
            _quest.OnCompleted -= OnQuestCompleted;
            _quest.OnFailed -= OnQuestExpired;
            
            OnStepResult?.Invoke(this, StepResult.Failure);
        }
    }
}