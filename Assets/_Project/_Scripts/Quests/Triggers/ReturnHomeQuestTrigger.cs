using UnityEngine;

namespace Quests.Triggers {
    public class ReturnHomeQuestTrigger : MonoBehaviour {
        
        [SerializeField] private string _questID;
        
        private void OnTriggerEnter(Collider other) {
            if (!other.TryGetComponent(out Player.Player player))
                return;
            
            var quest = QuestsService.Instance.GetActiveQuest(_questID);
            if (quest == null || !quest.IsActive) 
                return;
            
            quest.AddProgress(1);
        }
    }
}