using Quests;
using UnityEngine;

namespace Entities {
    public class Mushroom : MonoBehaviour, IOutline, IPickable{
        
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private string _targetQuestID;
        [SerializeField] private int _outlineThickness;
        
        private readonly int _outlineThicknessID = Shader.PropertyToID("_Thickness");
        private MaterialPropertyBlock _mpb;

        private void Start() => _mpb = new MaterialPropertyBlock();

        public void Pick() {
            var quest = QuestsService.Instance.GetActiveQuest(_targetQuestID);

            if (quest == null || !quest.IsActive)
                return;

            quest.AddProgress(1);
            Destroy(gameObject);
        }

        public void SetOutlineState(bool state) {
            _spriteRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(_outlineThicknessID, state ? _outlineThickness : 0);
            _spriteRenderer.SetPropertyBlock(_mpb);
        }
    }
}