using System;
using System.Collections.Generic;
using Helpers;
using UnityEngine;

namespace Quests {
    public class QuestsService : MonoBehaviour {
        
        private readonly Dictionary<string, Quest> _activeQuests = new();
        public event Action<Quest> OnQuestStarted;
        public event Action<Quest> OnQuestUpdated;
        public event Action<Quest> OnQuestCompleted;

        public void Run(Quest quest) {
            if (_activeQuests.ContainsKey(quest.ID)) {
                Log.Warning($"Quest '{quest.ID}' is already active.");
                return;
            }
        
            quest.OnUpdated += HandleQuestUpdated;
            quest.OnCompleted += HandleQuestCompleted;
        
            _activeQuests.Add(quest.ID, quest);
            quest.Run();
        
            OnQuestStarted?.Invoke(quest);
            Log.Debug($"Quest accepted: {quest.Title}");
        }

        private void HandleQuestUpdated(Quest quest) => OnQuestUpdated?.Invoke(quest);

        private void HandleQuestCompleted(Quest quest) {
            Log.Debug($"Quest completed: {quest.Title}");

            quest.OnUpdated -= HandleQuestUpdated;
            quest.OnCompleted -= HandleQuestCompleted;

            _activeQuests.Remove(quest.ID);

            OnQuestCompleted?.Invoke(quest);
        }

        public Quest GetActiveQuest(string questId) => _activeQuests.GetValueOrDefault(questId);
    }
}