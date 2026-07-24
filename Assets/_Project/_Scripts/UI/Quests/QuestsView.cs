using Helpers;
using Quests;
using TMPro;
using UnityEngine;

namespace UI.Quests {
    public class QuestsView : MonoBehaviour {
        
        [SerializeField] private TMP_Text _titleLbl;
        [SerializeField] private TMP_Text _descLbl;
        [SerializeField] private TMP_Text _progressLbl;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private QuestsService _questsService;
        
        private void Start() {
            _canvasGroup.SetAlpha(false);
            _questsService.OnQuestStarted += OnQuestStarted;
            _questsService.OnQuestUpdated += OnQuestUpdated;
            _questsService.OnQuestCompleted += OnQuestCompleted;
        }
        
        private void OnDestroy() {
            _questsService.OnQuestStarted -= OnQuestStarted;
            _questsService.OnQuestUpdated -= OnQuestUpdated;
            _questsService.OnQuestCompleted -= OnQuestCompleted;
        }
        
        private void OnQuestStarted(Quest quest) {
            _canvasGroup.SetAlpha(true);
            OnQuestUpdated(quest);
        }

        private void OnQuestUpdated(Quest quest) {
            _titleLbl.SetText(quest.Title);
            _descLbl.SetText(quest.Description);
            _progressLbl.SetText($"{quest.CurrentAmount} / {quest.RequiredAmount}");
        }

        private void OnQuestCompleted(Quest quest) {
            _canvasGroup.SetAlpha(false);
        }
    }
}