using System.Collections.Generic;
using CMSResources;
using Quests;
using UnityEngine;

namespace UI.Quests {
    public class QuestUI : MonoBehaviour {
        [SerializeField] private Transform _activeQuestsContainer;

        private readonly Dictionary<string, QuestView> _activeViews = new();
        private QuestsService _questsService;

        private void OnEnable() {
            _questsService = QuestsService.Instance;
            _questsService.OnQuestStarted += OnQuestStarted;
            _questsService.OnQuestUpdated += OnQuestUpdated;
            _questsService.OnQuestCompleted += OnQuestCompleted;
        }

        private void OnDisable() {
            if (_questsService == null) 
                return;
            
            _questsService.OnQuestStarted -= OnQuestStarted;
            _questsService.OnQuestUpdated -= OnQuestUpdated;
            _questsService.OnQuestCompleted -= OnQuestCompleted;
        }

        private void OnQuestStarted(Quest quest) {
            if (_activeViews.ContainsKey(quest.ID))
                return;

            var pb = CMS.Get<QuestView>(CMSIdRegistry.QuestView);
            var view = Instantiate(pb, _activeQuestsContainer);
            view.Bind(quest);
            _activeViews.Add(quest.ID, view);
        }

        private void OnQuestUpdated(Quest quest) {
            if (_activeViews.TryGetValue(quest.ID, out var view)) 
                view.Bind(quest);
        }

        private void OnQuestCompleted(Quest quest) {
            if (_activeViews.Remove(quest.ID, out var view)) 
                Destroy(view.gameObject);
        }
    }
}