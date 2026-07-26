using Gameplay;
using Quests;
using UnityEngine;

namespace Entities {
    public class Butterfly : MonoBehaviour, IOutline, IInteractable {

        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private string _targetQuestID;
        [SerializeField] private int _outlineThickness;
        [SerializeField] private Transform[] _positions;
        [SerializeField] private float _moveSpeed = 2f;
        [Header("Height")]
        [SerializeField] private float _minY = 1f;
        [SerializeField] private float _maxY = 3f;
        [Header("Butterfly Motion")]
        [SerializeField] private float _wobbleAmount = 0.3f;
        [SerializeField] private float _wobbleSpeed = 5f;

        private readonly int _outlineThicknessID = Shader.PropertyToID("_Thickness");
        private MaterialPropertyBlock _mpb;
        private Vector3 _targetPosition;

        private void Start() {
            _mpb = new MaterialPropertyBlock();
            PickRandomTarget();
        }

        private void Update() {
            Vector3 direction = (_targetPosition - transform.position).normalized;
            Vector3 wobble = transform.right * (Mathf.Sin(Time.time * _wobbleSpeed) * _wobbleAmount);
            Vector3 movement = (direction + wobble).normalized;
            
            transform.position += movement * (_moveSpeed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, _targetPosition) < 0.3f)
                PickRandomTarget();
        }

        private void PickRandomTarget() {
            if (_positions == null || _positions.Length == 0)
                return;

            Transform target;

            do {
                target = _positions[Random.Range(0, _positions.Length)];
            }
            while (target.position == _targetPosition && _positions.Length > 1);

            Vector3 position = target.position;
            position.y = Random.Range(_minY, _maxY);
            _targetPosition = position;
        }

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