using System;
using DG.Tweening;
using Gameplay;
using Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entities {
    public class HouseHitbox : MonoBehaviour, IInteractable, IOutline {
        
        [Header("References")]
        [SerializeField] private MeshRenderer _outlineMeshRenderer;
        [SerializeField] private int _outlineThickness;
        [SerializeField] private Vector2 _hitAmountRange;
        [SerializeField] private float _lifetime;
        [Header("Tween")]
        [SerializeField] private float _punchDuration;
        
        public event Action<HouseHitbox> OnHit;
        
        private readonly int _outlineThicknessID = Shader.PropertyToID("_OutlineThickness");
        private MaterialPropertyBlock _mpb;
        private Timer _timer;
        private int _currentHitAmount;
        private int _hitAmount;
        private Vector3 _initialScale;

        private void Start() {
            _mpb = new MaterialPropertyBlock();
            _hitAmount = Mathf.RoundToInt(Random.Range(_hitAmountRange.x, _hitAmountRange.y));
            _initialScale = transform.localScale;
        }

        private void OnEnable() => _timer = new Timer(this).Start(_lifetime, onComplete: OnExpire);

        private void OnDisable() {
            transform.DOKill();
            transform.localScale = _initialScale;
            _currentHitAmount = 0;
            _timer.Stop();
        }

        public void Interact() {
            if (_currentHitAmount >= _hitAmount) {
                OnHit?.Invoke(this);
                OnExpire();
                return;
            }
            
            _currentHitAmount++;
            transform.DOPunchScale(transform.localScale * 0.25f, _punchDuration, vibrato: 2).SetEase(Ease.OutBounce);
            AudioService.Instance.PlayBuildSFX();
        }

        private void OnExpire() => gameObject.SetActive(false);

        public void SetOutlineState(bool state) {
            _outlineMeshRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(_outlineThicknessID, state ? _outlineThickness : 0);
            _outlineMeshRenderer.SetPropertyBlock(_mpb);
        }
    }
}