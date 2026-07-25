using CMSResources;
using Quests;
using UnityEngine;

namespace Entities {
    public class Tree : MonoBehaviour, IOutline, ICuttable {
        
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _logSp;
        [Header("Properties")]
        [SerializeField] private string _targetQuestID;
        [SerializeField] private int _outlineThickness;
        [SerializeField] private Vector2 _logsSpawnRange;
        [SerializeField] private Vector2 _requiredHitsRange;
        
        private readonly int _outlineThicknessID = Shader.PropertyToID("_Thickness");
        private MaterialPropertyBlock _mpb;
        private int _currentHitsAmount;
        private float _requiredHitsAmount;

        private void Start() {
            _mpb = new MaterialPropertyBlock();
            _requiredHitsAmount = Random.Range(_requiredHitsRange.x, _requiredHitsRange.y);
        }

        public void SetOutlineState(bool state) {
            var quest = QuestsService.Instance.GetActiveQuest(_targetQuestID);
            if (quest == null || !quest.IsActive) 
                return;
            
            _spriteRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(_outlineThicknessID, state ? _outlineThickness : 0);
            _spriteRenderer.SetPropertyBlock(_mpb);
        }

        public void Hit() {
            var quest = QuestsService.Instance.GetActiveQuest(_targetQuestID);
            if (quest == null || !quest.IsActive) 
                return;
            
            if (_currentHitsAmount >= _requiredHitsAmount) {
                SpawnLogs();
                Destroy(gameObject);
                return;
            }

            _currentHitsAmount++;
        }

        private void SpawnLogs() {
            var logsToSpawnAmount = Random.Range(_logsSpawnRange.x, _logsSpawnRange.y);
            var logPb = CMS.Get<TreeLog>(CMSIdRegistry.TreeLog);
            for (int i = 0; i < logsToSpawnAmount; i++)
                Instantiate(logPb, _logSp.transform.position, Quaternion.identity);
        }
    }
}