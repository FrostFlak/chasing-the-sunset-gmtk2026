using System;
using System.Collections.Generic;
using Entities;
using Helpers;
using Helpers.ExtMethods;
using Helpers.Tick;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

namespace Player {
    public class Player :  MonoBehaviour {

        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private DecalProjector _shadowDecal;
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private PlayerController _controller;
        [Header("Properties")]
        [field: SerializeField] public int Age { get; set; }
        [SerializeField] private int _raycastDistance;
        
        private readonly HashSet<ITick> _tickables = new();
        private readonly HashSet<IDisposable> _disposables = new();
        private GameplayInput _input;
        private PlayerAnimator _playerAnimator;
        private MouseRaycaster _mouseRaycaster;

        private void Awake() {
            _input = new GameplayInput();
            _mouseRaycaster = new MouseRaycaster(Camera.main, _raycastDistance, triggerInteraction: QueryTriggerInteraction.Collide);
        }

        private void Start() {
            _controller.Initialize(_input, _rigidbody, _cameraPivot);
            _playerAnimator = new PlayerAnimator(_input, _animator, _spriteRenderer, this);
            
            _tickables.Add(_playerAnimator);
            _disposables.Add(_playerAnimator);

            _mouseRaycaster.OnEnter += OnRayEnter;
            _mouseRaycaster.OnExit += OnRayExit;
        }
        
        private void Update() {
            foreach (var tickable in _tickables)
                tickable.Tick(Time.deltaTime);
            
            _mouseRaycaster.Raycast();

            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            if (_mouseRaycaster.CurrentHitObject == null) 
                return;

            if (_mouseRaycaster.CurrentHitObject.TryGetComponent(out IInteractable pickable))
                pickable.Interact();
        }

        private void OnDestroy() {
            _input.Dispose();
            
            foreach (var d in _disposables) 
                d.Dispose();
            
            _mouseRaycaster.OnEnter -= OnRayEnter;
            _mouseRaycaster.OnExit -= OnRayExit;
        }

        public void SetState(bool active) {
            if (active)
                _input.Enable();
            else
                _input.Disable();
            
            _spriteRenderer.enabled = active;
            _shadowDecal.gameObject.SetActive(active);
        }
        
        private void OnRayEnter(GameObject go) {
            if (!go.TryGetComponent(out IOutline pickable))
                return;
            
            pickable.SetOutlineState(true);
        }
        
        private void OnRayExit(GameObject go) {
            if (!go.TryGetComponent(out IOutline pickable))
                return;
            
            pickable.SetOutlineState(false);
        }
    }
}