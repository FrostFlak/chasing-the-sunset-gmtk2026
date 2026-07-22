using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using Helpers.ExtMethods;
using Helpers.WeightedRandom;
using UnityEngine;
using Random = System.Random;

namespace Helpers.Terrain {
    public abstract class TerrainObjectPlacer : MonoBehaviour {

        #region SerializedFields
        [field: SerializeField, BoxGroup("Prefabs")] public List<WeightedItem<GameObject>> Variants { get; private set; }
        [SerializeField, BoxGroup("References")] protected MeshFilter _meshFilter;
        [SerializeField, BoxGroup("Settings")] protected float _minDistance = 2f;
        [field: SerializeField, BoxGroup("Settings")] public int Seed { get; private set; }
        [SerializeField, BoxGroup("Settings")] protected float _densityNoiseScale = 0.05f;
        [SerializeField, Range(0f, 1f), BoxGroup("Settings")] protected float _densityThreshold = 0.35f;
        [SerializeField, BoxGroup("Settings")] protected LayerMask _groundLayer;
        [SerializeField, BoxGroup("Settings")] protected float _raycastHeight = 50f;
        [SerializeField, BoxGroup("Settings")] protected Vector2 _scaleRange = new(0.8f, 1.2f);
        [SerializeField, Range(0f, 90f), BoxGroup("Settings")] protected float _maxSlopeAngle = 40f;
        [SerializeField, Range(0f, 1f), BoxGroup("Settings")] protected float _slopeInfluence;
        [SerializeField, BoxGroup("Settings")] protected bool _createContainer = true;
        [SerializeField, BoxGroup("Settings")] protected List<DensityZone> _densityZones = new();
        [SerializeField, BoxGroup("Debug")] protected bool _showPreviewPoints = true;
        [SerializeField, BoxGroup("Debug")] protected bool _showDensityZones = true;
        #endregion

        #region PrivateFields
        private Random _random;
        private GameObject _container;
        #endregion

        #region ProtectedProperties
        protected Random RandomGen => _random;
        #endregion

        #region Structs
        public struct PlacementPoint {
            public Vector3 Position;
            public Vector3 Normal;
        }

        [Serializable]
        protected struct DensityZone {
            public BoxCollider Collider;

            [Range(0f, 1f)]
            public float Density;
        }
        #endregion

        #region Properties
        public List<GameObject> PlacedObjects { get; } = new();
        public List<PlacementPoint> PreviewPoints { get; } = new();
        #endregion

        #region Generation
        protected void GeneratePoints() {
            if (_meshFilter == null || _meshFilter.sharedMesh == null)
                return;

            PreviewPoints.Clear();
            _random = new Random(Seed);

            Bounds bounds = _meshFilter.sharedMesh.bounds;
            Vector3 scale = _meshFilter.transform.lossyScale;

            float width = bounds.size.x * scale.x;
            float depth = bounds.size.z * scale.z;

            Vector3 origin = _meshFilter.transform.position + new Vector3(bounds.min.x * scale.x, 0f, bounds.min.z * scale.z);

            List<Vector2> points = PoissonDisc(width, depth, _minDistance);

            float noiseOffX = Seed * 37.3f;
            float noiseOffZ = Seed * 91.7f;

            foreach (Vector2 p in points) {
                Vector3 worldXZ =
                    origin + new Vector3(p.x, _raycastHeight, p.y);

                if (!Physics.Raycast(worldXZ, Vector3.down, out RaycastHit hit, _raycastHeight * 2f, _groundLayer))
                    continue;

                if (Vector3.Angle(Vector3.up, hit.normal) > _maxSlopeAngle)
                    continue;

                float zoneDensity = GetZoneDensity(hit.point);

                float effectiveThreshold = zoneDensity >= 0f
                    ? Mathf.Lerp(1f, _densityThreshold, zoneDensity)
                    : _densityThreshold;

                float noiseDensity = Mathf.PerlinNoise(
                    (p.x + noiseOffX) * _densityNoiseScale,
                    (p.y + noiseOffZ) * _densityNoiseScale
                );

                if (noiseDensity < effectiveThreshold)
                    continue;

                PreviewPoints.Add(new PlacementPoint {
                    Position = hit.point,
                    Normal = hit.normal
                });
            }
        }

        private float GetZoneDensity(Vector3 worldPoint) {
            foreach (DensityZone zone in _densityZones) {
                if (zone.Collider == null)
                    continue;

                if (zone.Collider.bounds.Contains(worldPoint))
                    return zone.Density;
            }

            return -1f;
        }
        #endregion

        #region Placement
        protected virtual void SpawnAll(Transform parent = null) {
            if (Variants == null || Variants.Count == 0)
                return;

            var weighted = new WeightedRandom<GameObject>(Variants);

            foreach (PlacementPoint point in PreviewPoints)
                Spawn(point, parent, weighted);
        }

        public virtual GameObject Spawn(
            PlacementPoint point,
            Transform parent,
            WeightedRandom<GameObject> weighted
        ) {
            GameObject prefab = weighted.GetRandom();

            float scale = _random.Range(_scaleRange.x, _scaleRange.y);
            float yRot = _random.Range(0f, 360f);

            Quaternion upright = Quaternion.Euler(0f, yRot, 0f);
            Quaternion aligned = Quaternion.FromToRotation(Vector3.up, point.Normal) * upright;
            Quaternion rotation = Quaternion.Slerp(upright, aligned, _slopeInfluence);

            GameObject obj = Instantiate(prefab, point.Position, rotation, parent);

            obj.transform.localScale = Vector3.one * scale;
            PlacedObjects.Add(obj);
            return obj;
        }

        [Button, BoxGroup("Debug")]
        public void Clear() {
            foreach (GameObject obj in PlacedObjects) {
                if (obj == null)
                    continue;

#if UNITY_EDITOR
                DestroyImmediate(obj);
#else
                Destroy(obj);
#endif
            }
            
#if UNITY_EDITOR
            if (_container != null)
                DestroyImmediate(_container.gameObject);

#else
            if (_container != null)
                Destroy(_container.gameObject);
#endif

            PlacedObjects.Clear();
            PreviewPoints.Clear();
        }

        private Transform CreateContainer() {
            _container = new GameObject("PlacedObjects");

            _container.transform.SetParent(transform);
            _container.transform.localPosition = Vector3.zero;

            return _container.transform;
        }
        #endregion

        #region PoissonDisc
        private List<Vector2> PoissonDisc(
            float width,
            float height,
            float minDist,
            int samplesK = 30
        ) {

            float cellSize = minDist / Mathf.Sqrt(2f);

            int gridW = Mathf.CeilToInt(width / cellSize);
            int gridH = Mathf.CeilToInt(height / cellSize);

            int[] grid = new int[gridW * gridH];

            for (int i = 0; i < grid.Length; i++)
                grid[i] = -1;

            List<Vector2> points = new();
            List<int> active = new();

            Vector2 first = new(_random.Range(0f, width), _random.Range(0f, height));

            points.Add(first);
            active.Add(0);

            grid[GridIndex(first, cellSize, gridW)] = 0;

            while (active.Count > 0) {
                int idx = active[_random.Range(0, active.Count)];

                Vector2 current = points[idx];
                bool found = false;

                for (int k = 0; k < samplesK; k++) {
                    float angle = _random.Range(0f, Mathf.PI * 2f);

                    float dist = _random.Range(minDist, minDist * 2f);

                    Vector2 candidate = current + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

                    if (candidate.x < 0f ||
                        candidate.x >= width ||
                        candidate.y < 0f ||
                        candidate.y >= height)
                        continue;

                    if (!IsFarEnough(
                            candidate,
                            points,
                            grid,
                            cellSize,
                            gridW,
                            gridH,
                            minDist))
                        continue;

                    points.Add(candidate);

                    int newIdx = points.Count - 1;
                    active.Add(newIdx);
                    grid[GridIndex(candidate, cellSize, gridW)] = newIdx;

                    found = true;
                    break;
                }

                if (!found)
                    active.Remove(idx);
            }

            return points;
        }

        private bool IsFarEnough(
            Vector2 candidate,
            List<Vector2> points,
            int[] grid,
            float cellSize,
            int gridW,
            int gridH,
            float minDist) {

            int cx = Mathf.FloorToInt(candidate.x / cellSize);
            int cz = Mathf.FloorToInt(candidate.y / cellSize);

            for (int dz = -2; dz <= 2; dz++) {
                for (int dx = -2; dx <= 2; dx++) {
                    int nx = cx + dx;
                    int nz = cz + dz;

                    if (nx < 0 ||
                        nx >= gridW ||
                        nz < 0 ||
                        nz >= gridH)
                        continue;

                    int stored = grid[nz * gridW + nx];

                    if (stored == -1)
                        continue;

                    if (Vector2.Distance(candidate, points[stored]) < minDist)
                        return false;
                }
            }

            return true;
        }

        private int GridIndex(
            Vector2 p,
            float cellSize,
            int gridW
        ) {

            int x = Mathf.FloorToInt(p.x / cellSize);
            int z = Mathf.FloorToInt(p.y / cellSize);

            return z * gridW + x;
        }
        #endregion

        #region Gizmos
        private void OnDrawGizmosSelected() {
            if (_showPreviewPoints &&
                PreviewPoints != null &&
                PreviewPoints.Count > 0
            ) {

                Gizmos.color = new Color(0.2f, 0.9f, 0.2f, 0.8f);

                foreach (PlacementPoint p in PreviewPoints) 
                    Gizmos.DrawSphere(p.Position, _minDistance * 0.15f);
            }

            if (!_showDensityZones)
                return;

            if (_densityZones == null)
                return;

            foreach (DensityZone zone in _densityZones) {
                if (zone.Collider == null)
                    continue;
                
                Color c = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.9f, 0.2f), zone.Density);
                Gizmos.matrix = Matrix4x4.TRS(zone.Collider.bounds.center, zone.Collider.transform.rotation, Vector3.one);
                Gizmos.color = new Color(c.r, c.g, c.b, 0.25f);
                Gizmos.DrawCube(Vector3.zero, zone.Collider.bounds.size);
                Gizmos.color = new Color(c.r, c.g, c.b, 0.9f);
                Gizmos.DrawWireCube(Vector3.zero, zone.Collider.bounds.size);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
        #endregion

        #region Debug
        [Button, BoxGroup("Debug")]
        private void GeneratePreview() => GeneratePoints();

        [Button, BoxGroup("Debug")]
        private void PlaceObjects() {
            Clear();

            GeneratePoints();
            SpawnAll(_createContainer ? CreateContainer() : null);
        }
        #endregion
    }
}