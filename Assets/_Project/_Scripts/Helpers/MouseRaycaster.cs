using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Helpers {
    public class MouseRaycaster {

        #region Private Fields
        private readonly Camera _camera;
        private readonly float _distance;
        private readonly LayerMask _layerMask;
        private readonly QueryTriggerInteraction _triggerInteraction;
        private bool _hasHit;
        #endregion

        #region Properties
        public event Action<GameObject> OnEnter;
        public event Action<GameObject> OnExit;
        public GameObject CurrentHitObject { get; private set; }
        public RaycastHit CurrentRayHit { get; private set; }
        public Vector3 CurrentHitPoint => CurrentRayHit.point;
        public Vector3 CurrentHitNormal => CurrentRayHit.normal;
        #endregion

        #region Constructor
        public MouseRaycaster(
            Camera camera,
            float distance,
            LayerMask layerMask = default,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore
        ) {
            _camera = camera;
            _distance = distance;
            _layerMask = layerMask == 0 ? Physics.DefaultRaycastLayers : layerMask;
            _triggerInteraction = triggerInteraction;
        }
        #endregion

        #region Raycast
        public void Raycast() {
            if (_camera == null)
                return;

            Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, _distance, _layerMask, _triggerInteraction)) {
                _hasHit = true;
            
                GameObject newObject = hit.collider.gameObject;

                if (CurrentHitObject != newObject) {
                    if (CurrentHitObject != null)
                        OnExit?.Invoke(CurrentHitObject);

                    CurrentHitObject = newObject;
                    CurrentRayHit = hit;

                    OnEnter?.Invoke(CurrentHitObject);
                }
                else {
                    CurrentRayHit = hit;
                }
            }
            else {
                _hasHit = false;

                if (CurrentHitObject != null)
                    OnExit?.Invoke(CurrentHitObject);

                CurrentHitObject = null;
                CurrentRayHit = default;
            }

#if UNITY_EDITOR
            DrawDebug(ray);
#endif
        }
        #endregion

        #region Debug
        private void DrawDebug(Ray ray) {
            Debug.DrawRay(ray.origin, ray.direction * _distance, Color.green);

            if (!_hasHit)
                return;

            Debug.DrawLine(ray.origin, CurrentRayHit.point, Color.yellow);
            Debug.DrawRay(CurrentRayHit.point, CurrentRayHit.normal * 0.25f, Color.red);

            const float size = 0.04f;

            Debug.DrawLine(CurrentRayHit.point + Vector3.left * size, CurrentRayHit.point + Vector3.right * size, Color.cyan);
            Debug.DrawLine(CurrentRayHit.point + Vector3.up * size, CurrentRayHit.point + Vector3.down * size, Color.cyan);
            Debug.DrawLine(CurrentRayHit.point + Vector3.forward * size, CurrentRayHit.point + Vector3.back * size, Color.cyan);
        }
        #endregion
    }
}