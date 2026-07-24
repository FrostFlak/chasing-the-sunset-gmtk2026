using System;
using System.Collections.Generic;
using Helpers.Tick;
using UnityEngine;

namespace Player {
    public class Player :  MonoBehaviour {

        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private PlayerController _controller;
        [Header("Properties")]
        [field: SerializeField] public int Age { get; private set; }

        private readonly HashSet<ITick> _tickables = new();
        private readonly HashSet<IDisposable> _disposables = new();
        private GameplayInput _input;
        private PlayerAnimator _playerAnimator;

        private void OnEnable() {
            _input = new GameplayInput();
            _input.Enable();
        }

        private void Start() {
            _controller.Initialize(_input, _rigidbody, _cameraPivot);
            _playerAnimator = new PlayerAnimator(_input, _animator, _spriteRenderer, this);
            
            _tickables.Add(_playerAnimator);
            _disposables.Add(_playerAnimator);
        }

        private void Update() {
            foreach (var tickable in _tickables) 
                tickable.Tick(Time.deltaTime);
        }

        private void OnDisable() {
            _input.Disable();
            _input.Dispose();
            
            foreach (var d in _disposables) 
                d.Dispose();
        }
    }
}