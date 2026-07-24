using Quests;
using TMPro;
using UnityEngine;

namespace UI.Quests {
    public class QuestView : MonoBehaviour {
        
        [SerializeField] private TMP_Text _titleLbl;
        [SerializeField] private TMP_Text _descLbl;
        [SerializeField] private TMP_Text _progressLbl;

        private Quest _quest;
        
        private void OnDestroy() => Unbind();

        public void Bind(Quest quest) {
            Unbind();

            _quest = quest;

            if (_quest == null)
                return;

            Refresh(_quest);

            _quest.OnUpdated += Refresh;
            _quest.OnCompleted += OnQuestCompleted;
        }
        
        private void Unbind() {
            if (_quest == null) 
                return;
            
            _quest.OnUpdated -= Refresh;
            _quest.OnCompleted -= OnQuestCompleted;
            _quest = null;
        }

        private void Refresh(Quest quest) {
            _titleLbl.SetText(quest.Title);
            _descLbl.SetText(quest.Description);
            _progressLbl.SetText($"{quest.CurrentAmount} / {quest.RequiredAmount}");
        }

        private void OnQuestCompleted(Quest quest) {
            Unbind();
            gameObject.SetActive(false);
        }
    }
}