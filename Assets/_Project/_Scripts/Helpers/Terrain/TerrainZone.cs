using UnityEngine;

namespace Helpers.Terrain {
    public enum ZoneType { Flatten, Raise, Lower }
    public enum ZoneFalloff { Smooth, Linear }

    [ExecuteAlways]
    public class TerrainZone : MonoBehaviour {

        #region SerializedFields
        [SerializeField] private ZoneType _type = ZoneType.Flatten;
        [SerializeField] private ZoneFalloff _falloff = ZoneFalloff.Smooth;
        [SerializeField, Range(0f, 1f)] private float _strength = 1f;
        [SerializeField, Min(0.1f)] private float _radius = 10f;
        [SerializeField] private float _heightOffset = 2f;
        [SerializeField] private bool _drawGizmos;
        #endregion

        #region Unity
        private void OnValidate() => NotifyParent();
        private void OnTransformParentChanged() => NotifyParent();
        #endregion

        #region Influence
        public float Apply(float height, Vector3 vertexWorldPos, Transform terrainTransform) {
            float influence = GetInfluence(vertexWorldPos);
            if (influence <= 0f)
                return height;

            return _type switch {
                ZoneType.Flatten => Mathf.Lerp(height, terrainTransform.InverseTransformPoint(transform.position).y, influence),
                ZoneType.Raise => height + _heightOffset * influence,
                ZoneType.Lower => height - _heightOffset * influence,
                _ => height
            };
        }

        private float GetInfluence(Vector3 worldPos) {
            float dist = new Vector2(
                worldPos.x - transform.position.x,
                worldPos.z - transform.position.z
            ).magnitude;

            if (dist >= _radius)
                return 0f;

            float t = 1f - dist / _radius;
            float falloff = _falloff == ZoneFalloff.Smooth ? Mathf.SmoothStep(0f, 1f, t) : t;
            return falloff * _strength;
        }
        #endregion

        #region Helpers
        private void NotifyParent() => GetComponentInParent<TerrainGenerator>()?.Generate();
        #endregion

        #region Gizmos
        private void OnDrawGizmosSelected() {
            if (!_drawGizmos)
                return;
            
            Color color = _type switch {
                ZoneType.Flatten => new Color(1f, 0.9f, 0f),
                ZoneType.Raise => new Color(0.2f, 0.9f, 0.2f),
                ZoneType.Lower => new Color(0.9f, 0.2f, 0.2f),
                _ => Color.white
            };

#if UNITY_EDITOR
            UnityEditor.Handles.color = new Color(color.r, color.g, color.b, 0.15f);
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, _radius);

            UnityEditor.Handles.color = new Color(color.r, color.g, color.b, 0.9f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _radius);

            string label = _type switch {
                ZoneType.Flatten => $"Flatten  str:{_strength:F2}",
                ZoneType.Raise => $"Raise +{_heightOffset:F1}m  str:{_strength:F2}",
                ZoneType.Lower => $"Lower -{_heightOffset:F1}m  str:{_strength:F2}",
                _ => string.Empty
            };
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.2f, label);
#endif
        }
        #endregion
    }
}
