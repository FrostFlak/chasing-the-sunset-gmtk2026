using System;
using Quests;
using UnityEngine;

namespace Gameplay.LevelSteps {
    [Serializable]
    public class QuestStep : ILevelStep {

        [SerializeField] private Quest _quest;
        [SerializeField] private GameObject _questObj;
        private LevelContext _levelContext;

        public event Action<ILevelStep, StepResult> OnStepResult;

        public void Initialize(LevelContext levelContext) {
            _levelContext = levelContext;
            if (_questObj != null)
                _questObj.SetActive(false);
        }

        public void Enter() {
            _levelContext.QuestsService.Run(_quest);
            _quest.OnCompleted += OnQuestCompleted;
            _quest.OnFailed += OnQuestExpired;
            if (_questObj != null)
                _questObj.SetActive(true);
        }
        
        private void OnQuestCompleted(Quest quest) {
            _quest.OnCompleted -= OnQuestCompleted;
            _quest.OnFailed -= OnQuestExpired;
            if (_questObj != null)
                _questObj.SetActive(false);
            
            OnStepResult?.Invoke(this, StepResult.Success);
        }
        
        private void OnQuestExpired(Quest quest) {
            _quest.OnCompleted -= OnQuestCompleted;
            _quest.OnFailed -= OnQuestExpired;
            if (_questObj != null)
                _questObj.SetActive(false);
            
            OnStepResult?.Invoke(this, StepResult.Failure);
        }
    }
}