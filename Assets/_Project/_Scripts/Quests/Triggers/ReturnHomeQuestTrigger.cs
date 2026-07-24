using UnityEngine;

namespace Quests.Triggers {
    public class ReturnHomeQuestTrigger : MonoBehaviour {
        
        [SerializeField] private QuestsService _questsService;
        [SerializeField] private string _questID;
        
        private void OnTriggerEnter(Collider other) {
            if (!other.TryGetComponent(out Player.Player player))
                return;
            
            var quest = _questsService.GetActiveQuest(_questID);
            if (quest == null || !quest.IsActive) 
                return;
            
            quest.AddProgress(1);
        }
    }
}