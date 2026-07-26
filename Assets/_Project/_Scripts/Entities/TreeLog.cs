using Gameplay;
using Quests;
using UnityEngine;

namespace Entities {
    public class TreeLog : MonoBehaviour, IOutline, IInteractable {
        
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private string _targetQuestID;
        [SerializeField] private int _outlineThickness;
        
        private readonly int _outlineThicknessID = Shader.PropertyToID("_Thickness");
        private MaterialPropertyBlock _mpb;
        
        private void Start() => _mpb = new MaterialPropertyBlock();

        public void Interact() {
            var quest = QuestsService.Instance.GetActiveQuest(_targetQuestID);
            if (quest == null || !quest.IsActive) 
                return;
            
            quest.AddProgress(1);
            AudioService.Instance.PlayPickupSFX();
            Destroy(gameObject);
        }
        
        public void SetOutlineState(bool state) {
            var quest = QuestsService.Instance.GetActiveQuest(_targetQuestID);
            if (quest == null || !quest.IsActive) 
                return;
            
            _spriteRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(_outlineThicknessID, state ? _outlineThickness : 0);
            _spriteRenderer.SetPropertyBlock(_mpb);
        }
    }
}