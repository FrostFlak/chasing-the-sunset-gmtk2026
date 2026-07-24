using UnityEngine;

namespace Quests.Triggers {
    
    public class ButterflyQuestTrigger : MonoBehaviour {

        [SerializeField] private string _targetQuestID;

        // private void OnMouseDown() {
        //     var quest = QuestsService.Instance.GetActiveQuest(_targetQuestID);
        //
        //     if (quest == null || !quest.IsActive) 
        //         return;
        //     
        //     quest.AddProgress(1);
        //     Destroy(gameObject);
        // }

        // private void OnTriggerEnter(Collider other) {
        //     var quest = QuestsService.Instance.GetActiveQuest(_targetQuestID);
        //
        //     if (quest == null || !quest.IsActive) 
        //         return;
        //     
        //     quest.AddProgress(1);
        //     Destroy(gameObject);
        // }
    }
}