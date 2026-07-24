using System;
using Helpers.Tick;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
    public class PlayerAnimator : ITick, IDisposable {
        
        private readonly Animator _animator;
        private readonly GameplayInput _input;
        private readonly SpriteRenderer _spriteRenderer;
        private readonly Player _player;

        private readonly int _velocityX = Animator.StringToHash("VelocityX");
        private readonly int _velocityY = Animator.StringToHash("VelocityY");
        private readonly int _age = Animator.StringToHash("Age");
        
        private Vector2 _moveInput;

        public PlayerAnimator(GameplayInput input, Animator animator, SpriteRenderer spriteRenderer, Player player) {
            _input = input;
            _animator = animator;
            _spriteRenderer = spriteRenderer;
            _player = player;
            
            _input.Gameplay.Move.performed += OnMovePerformed;
            _input.Gameplay.Move.canceled += OnMovePerformed;
        }
        
        public void Dispose() {
            _input.Gameplay.Move.performed -= OnMovePerformed;
            _input.Gameplay.Move.canceled -= OnMovePerformed;
        }
        
        public void Tick(float dt) {
            _animator.SetFloat(_age, _player.Age);
            
            if (_moveInput.sqrMagnitude < 0.001f) {
                _animator.SetFloat(_velocityX, 0f);
                _animator.SetFloat(_velocityY, 0f);
                return;
            }

            if (Mathf.Abs(_moveInput.x) >= Mathf.Abs(_moveInput.y)) {
                _animator.SetFloat(_velocityX, 1f);
                _animator.SetFloat(_velocityY, 0f);

                _spriteRenderer.flipX = _moveInput.x > 0f;
            }
            else {
                _animator.SetFloat(_velocityX, 0f);
                _animator.SetFloat(_velocityY, 1f);
            }
        }
        
        private void OnMovePerformed(InputAction.CallbackContext ctx) => _moveInput = ctx.ReadValue<Vector2>();
    }
}