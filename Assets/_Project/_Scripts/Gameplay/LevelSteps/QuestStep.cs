using System;
using Quests;
using UnityEngine;

namespace Gameplay.LevelSteps {
    [Serializable]
    public class QuestStep : ILevelStep {

        [SerializeField] private Quest _quest;
        private LevelContext _levelContext;

        public event Action<ILevelStep> OnCompleted;
        
        public void Initialize(LevelContext levelContext) => _levelContext = levelContext;

        public void Enter() {
            _levelContext.QuestsService.Run(_quest);
            _quest.OnCompleted += OnQuestCompleted;
        }

        private void OnQuestCompleted(Quest quest) {
            _quest.OnCompleted -= OnQuestCompleted;
            OnCompleted?.Invoke(this);
        }
    }
}