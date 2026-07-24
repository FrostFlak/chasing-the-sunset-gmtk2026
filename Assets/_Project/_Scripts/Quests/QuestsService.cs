using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using Helpers;
using UnityEngine;

namespace Quests {
    public class QuestsService : SingletonMonoBehaviour<QuestsService> {
        
        [SerializeField] private List<Quest> _databaseQuests = new();
        private readonly Dictionary<string, Quest> _activeQuests = new();

        public event Action<Quest> OnQuestStarted;
        public event Action<Quest> OnQuestUpdated;
        public event Action<Quest> OnQuestCompleted;

        [Button]
        public void Run(string questId) {
            var quest = _databaseQuests.Find(q => q.ID == questId);

            if (quest == null) {
                Log.Error($"Quest with ID '{questId}' not found in database!");
                return;
            }

            if (_activeQuests.ContainsKey(questId)) {
                Log.Warning($"Quest '{questId}' is already active.");
                return;
            }

            quest.OnUpdated += HandleQuestUpdated;
            quest.OnCompleted += HandleQuestCompleted;

            _activeQuests.Add(questId, quest);
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