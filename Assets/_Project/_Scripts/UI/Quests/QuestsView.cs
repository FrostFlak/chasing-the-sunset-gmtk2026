using Helpers;
using Quests;
using TMPro;
using UnityEngine;

namespace UI.Quests {
    public class QuestsView : MonoBehaviour {
        
        [SerializeField] private TMP_Text _titleLbl;
        [SerializeField] private TMP_Text _durationLbl;
        [SerializeField] private TMP_Text _progressLbl;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        private void Start() {
            _canvasGroup.SetAlpha(false);
            
            QuestsService.Instance.OnQuestStarted += OnQuestStarted;
            QuestsService.Instance.OnQuestUpdated += OnQuestUpdated;
            QuestsService.Instance.OnQuestTimeUpdated += OnQuestTimeUpdated;
            QuestsService.Instance.OnQuestCompleted += OnQuestCompleted;
            QuestsService.Instance.OnQuestFailed += OnQuestFailed;
        }

        private void OnDestroy() {
            QuestsService.Instance.OnQuestStarted -= OnQuestStarted;
            QuestsService.Instance.OnQuestUpdated -= OnQuestUpdated;
            QuestsService.Instance.OnQuestTimeUpdated -= OnQuestTimeUpdated;
            QuestsService.Instance.OnQuestCompleted -= OnQuestCompleted;
            QuestsService.Instance.OnQuestFailed -= OnQuestFailed;
        }
        
        private void OnQuestStarted(Quest quest) {
            _canvasGroup.SetAlpha(true);
            OnQuestUpdated(quest);
        }

        private void OnQuestUpdated(Quest quest) {
            _titleLbl.SetText(quest.Title);
            _progressLbl.SetText($"{quest.CurrentAmount} / {quest.RequiredAmount}");
        }

        private void OnQuestTimeUpdated(Quest quest, int remainingTime) => _durationLbl.SetText($"{remainingTime} / {quest.Duration}");
        
        private void OnQuestCompleted(Quest quest) => _canvasGroup.SetAlpha(false);

        private void OnQuestFailed(Quest quest) => _canvasGroup.SetAlpha(false);
    }
}