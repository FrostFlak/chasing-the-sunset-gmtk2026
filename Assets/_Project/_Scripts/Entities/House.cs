using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Helpers;
using Helpers.ExtMethods;
using Quests;
using UnityEngine;

namespace Entities {
    public class House : MonoBehaviour {
        
        [Header("References")]
        [SerializeField] private List<GameObject> _parts = new();
        [SerializeField] private List<HouseHitbox> _hitboxes;
        [SerializeField] private string _targetQuestID;
        [SerializeField] private int _hbSpawnRate;

        private Timer _hbTimer;
        private void Start() {
            QuestsService.Instance.OnQuestStarted += OnQuestStarted;
            QuestsService.Instance.OnQuestCompleted += OnQuestCompleted;
            QuestsService.Instance.OnQuestFailed += OnQuestFailed;
        }

        private void OnDestroy() {
            _hbTimer?.Stop();
            QuestsService.Instance.OnQuestStarted -= OnQuestStarted;
            QuestsService.Instance.OnQuestCompleted -= OnQuestCompleted;
            QuestsService.Instance.OnQuestFailed -= OnQuestFailed;
        }

        private void StopSpawning() {
            _hbTimer?.Stop();
            foreach (var hb in _hitboxes) {
                if (hb != null)
                    hb.OnHit -= OnHit;
                
                hb.gameObject.SetActive(false);
            }
        }
        
        private void OnHit(HouseHitbox hitbox) {
            hitbox.OnHit -= OnHit;
            
            var quest = QuestsService.Instance.GetActiveQuest(_targetQuestID);
            if (quest == null || !quest.IsActive)
                return;

            var go = _parts.FirstOrDefault(p => !p.activeInHierarchy);
            if (go == null)
                return;
            
            quest.AddProgress(1);
            go.SetActive(true);
        }
        
        private void ShowHitbox() {
            var available = _hitboxes.Where(h => !h.gameObject.activeInHierarchy).ToList();
            if (available.Count == 0)
                return;

            var hitbox = available.GetRandom();
            hitbox.gameObject.SetActive(true);
            hitbox.OnHit += OnHit;
        }
        
        private void OnQuestStarted(Quest quest) {
            if (quest.ID != _targetQuestID)
                return;
            
            _hbTimer = new Timer(this, true).Start(_hbSpawnRate, onComplete: ShowHitbox);
        }
        
        private void OnQuestCompleted(Quest quest) {
            if (quest.ID != _targetQuestID)
                return;

            StopSpawning();
        }
        
        private void OnQuestFailed(Quest quest) {
            if (quest.ID != _targetQuestID)
                return;

            StopSpawning();
        }
    }
}