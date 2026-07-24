using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
    public class PlayerController : MonoBehaviour {
        
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [Header("Camera Rotation")]
        [SerializeField] private float[] _cameraAngles = { 45f, 135f, 225f, 315f };
        [SerializeField] private float _cameraRotationSmooth = 10f;
        [Header("Physics")]
        [SerializeField] private float _maxSlopeAngle = 45f;

        private GameplayInput _input;
        private Transform _cameraPivot;
        private Rigidbody _rigidBody;
        
        private Vector2 _moveInput;
        private Vector3 _groundNormal = Vector3.up;
        private int _currentCameraAngleIndex;
        private Quaternion _targetCameraRotation;

        public void Initialize(GameplayInput input, Rigidbody rb, Transform cameraPivot) {
            _input = input;
            _rigidBody = rb;
            _cameraPivot = cameraPivot;
            
            _input.Gameplay.Move.performed += OnMove;
            _input.Gameplay.Move.canceled += OnMove;

            _input.Gameplay.PrevCameraY.performed += OnRotateLeft;
            _input.Gameplay.NextCameraY.performed += OnRotateRight;

            InitializeCameraRotation();
        }

        private void OnDisable() {
            _input.Gameplay.Move.performed -= OnMove;
            _input.Gameplay.Move.canceled -= OnMove;

            _input.Gameplay.PrevCameraY.performed -= OnRotateLeft;
            _input.Gameplay.NextCameraY.performed -= OnRotateRight;
        }

        private void Update() => HandleCameraRotation();

        private void FixedUpdate() => HandleMovement();

        private void HandleMovement() {
            Vector3 forward = _cameraPivot.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = _cameraPivot.right;
            right.y = 0f;
            right.Normalize();


            Vector3 movement = right * _moveInput.x + forward * _moveInput.y;


            if (movement.sqrMagnitude > 0.001f) {
                movement.Normalize();
                movement = Vector3.ProjectOnPlane(movement, _groundNormal);

                Vector3 velocity = _rigidBody.linearVelocity;

                velocity.x = movement.x * _moveSpeed;
                velocity.z = movement.z * _moveSpeed;

                _rigidBody.linearVelocity = velocity;
            }
            else {
                Vector3 velocity = _rigidBody.linearVelocity;

                velocity.x = 0f;
                velocity.z = 0f;

                _rigidBody.linearVelocity = velocity;
            }
        }

        private void HandleCameraRotation() {
            float t = 1f - Mathf.Exp(-_cameraRotationSmooth * Time.deltaTime);
            _cameraPivot.rotation = Quaternion.Slerp(_cameraPivot.rotation, _targetCameraRotation, t);
        }

        private void InitializeCameraRotation() {
            float currentY = _cameraPivot.eulerAngles.y;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < _cameraAngles.Length; i++) {
                float distance = Mathf.Abs(Mathf.DeltaAngle(currentY, _cameraAngles[i]));

                if (distance < closestDistance) {
                    closestDistance = distance;
                    _currentCameraAngleIndex = i;
                }
            }

            UpdateTargetCameraRotation();
        }

        private void RotateCamera(int direction) {
            _currentCameraAngleIndex += direction;

            if (_currentCameraAngleIndex < 0)
                _currentCameraAngleIndex = _cameraAngles.Length - 1;

            if (_currentCameraAngleIndex >= _cameraAngles.Length) 
                _currentCameraAngleIndex = 0;

            UpdateTargetCameraRotation();
        }

        private void UpdateTargetCameraRotation() {
            Vector3 currentEuler = _cameraPivot.eulerAngles;
            _targetCameraRotation = Quaternion.Euler(currentEuler.x, _cameraAngles[_currentCameraAngleIndex], currentEuler.z);
        }

        private void OnMove(InputAction.CallbackContext context) => _moveInput = context.ReadValue<Vector2>();
        private void OnRotateLeft(InputAction.CallbackContext context) => RotateCamera(-1);
        private void OnRotateRight(InputAction.CallbackContext context) => RotateCamera(1);


        private void OnCollisionStay(Collision collision) {
            foreach (ContactPoint contact in collision.contacts) {
                if (Vector3.Angle(contact.normal, Vector3.up) <= _maxSlopeAngle) {
                    _groundNormal = contact.normal;
                    return;
                }
            }


            _groundNormal = Vector3.up;
        }


        private void OnCollisionExit() => _groundNormal = Vector3.up;
    }
}