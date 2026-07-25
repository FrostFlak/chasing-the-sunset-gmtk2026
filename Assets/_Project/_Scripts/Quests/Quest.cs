using System;
using Alchemy.Inspector;
using Helpers;
using UnityEngine;

namespace Quests {
    [Serializable]
    public class Quest {
        [field: SerializeField] public string ID { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public int Duration { get; private set; }
        [field: SerializeField] public int RequiredAmount { get; private set; }
        public int CurrentAmount { get; private set; }
        
        public bool IsActive { get; private set; }
        public bool IsCompleted => IsActive && CurrentAmount >= RequiredAmount;
        
        public event Action<Quest> OnUpdated;
        public event Action<Quest> OnCompleted;
        public event Action<Quest> OnFailed;
        
        private Timer _questTimer;
        
        public void Run(QuestsService questsService) {
            _questTimer = new Timer(questsService);
            
            CurrentAmount = 0;
            IsActive = true;
            OnUpdated?.Invoke(this);
            
            _questTimer.Start(Duration, onComplete: () => OnFailed?.Invoke(this));
        }

        public void AddProgress(int amount) {
            if (!IsActive || IsCompleted)
                return;

            CurrentAmount = Mathf.Min(CurrentAmount + amount, RequiredAmount);
            OnUpdated?.Invoke(this);

            if (CurrentAmount < RequiredAmount) 
                return;
            
            IsActive = false;
            OnCompleted?.Invoke(this);
        }
        
        #if UNITY_EDITOR
        [Button]
        private void ForceComplete() => AddProgress(RequiredAmount);
        #endif
    }
}