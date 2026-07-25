using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using Helpers.WeightedRandom;
using UnityEngine;
using Random = System.Random;

namespace Helpers.Terrain {
    public abstract class TerrainObjectPlacer : MonoBehaviour {
        #region Serialized Fields

        [field: SerializeField, BoxGroup("Prefabs")] public List<WeightedItem<GameObject>> Variants { get; private set; }
        [Header("References")]
        [SerializeField, BoxGroup("References")] protected MeshFilter _meshFilter;
        [SerializeField, BoxGroup("References")] protected UnityEngine.Terrain _terrain;
        [Header("Settings")]
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
        [SerializeField, BoxGroup("Settings")]
        protected List<DensityZone> _densityZones = new();
        [Header("Debug")]
        [SerializeField, BoxGroup("Debug")]
        protected bool _showPreviewPoints = true;
        [SerializeField, BoxGroup("Debug")]
        protected bool _showDensityZones = true;
        #endregion

        #region Private Fields
        private Random _random;
        private GameObject _container;
        #endregion

        #region Properties
        protected Random RandomGen => _random;
        public List<GameObject> PlacedObjects { get; } = new();
        public List<PlacementPoint> PreviewPoints { get; } = new();
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

        #region Generation
        protected void GeneratePoints() {
            if (!HasSurface())
                return;

            PreviewPoints.Clear();
            _random = new Random(Seed);
            Bounds bounds = GetSurfaceBounds();


            float width = bounds.size.x;
            float depth = bounds.size.z;


            Vector3 origin = new(bounds.min.x, 0f, bounds.min.z);


            List<Vector2> points = PoissonDisc(width, depth, _minDistance);


            float noiseOffX = Seed * 37.3f;
            float noiseOffZ = Seed * 91.7f;


            foreach (Vector2 p in points) {
                Vector3 samplePosition = origin + new Vector3(p.x, _raycastHeight, p.y);


                if (!TrySampleSurface(samplePosition, out Vector3 position, out Vector3 normal))
                    continue;


                if (Vector3.Angle(Vector3.up, normal) > _maxSlopeAngle)
                    continue;


                float zoneDensity = GetZoneDensity(position);


                float threshold =
                    zoneDensity >= 0f ? Mathf.Lerp(1f, _densityThreshold, zoneDensity) : _densityThreshold;


                float noise = Mathf.PerlinNoise((p.x + noiseOffX) * _densityNoiseScale,
                    (p.y + noiseOffZ) * _densityNoiseScale);


                if (noise < threshold)
                    continue;


                PreviewPoints.Add(new PlacementPoint { Position = position, Normal = normal });
            }
        }



        private bool HasSurface() {
            return (_meshFilter != null && _meshFilter.sharedMesh != null) || _terrain != null;
        }



        private Bounds GetSurfaceBounds() {
            if (_terrain != null)
            {
                TerrainData data = _terrain.terrainData;

                Vector3 size = data.size;

                Vector3 center =
                    _terrain.transform.position +
                    new Vector3(
                        size.x * 0.5f,
                        size.y * 0.5f,
                        size.z * 0.5f
                    );

                return new Bounds(center, size);
            }
            
            Mesh mesh = _meshFilter.sharedMesh;

            Bounds bounds = mesh.bounds;


            bounds.center = _meshFilter.transform.TransformPoint(bounds.center);


            bounds.size = Vector3.Scale(mesh.bounds.size, _meshFilter.transform.lossyScale);


            return bounds;
        }



        private bool TrySampleSurface(Vector3 worldPosition, out Vector3 position, out Vector3 normal) {

            if (_terrain != null) {
                TerrainData data = _terrain.terrainData;


                Vector3 local = worldPosition - _terrain.transform.position;


                if (local.x < 0 || local.z < 0 || local.x > data.size.x || local.z > data.size.z) {
                    position = default;
                    normal = default;
                    return false;
                }


                position = new Vector3(worldPosition.x,
                    _terrain.SampleHeight(worldPosition) + _terrain.transform.position.y, worldPosition.z);


                float u = local.x / data.size.x;

                float v = local.z / data.size.z;


                normal = data.GetInterpolatedNormal(u, v);


                return true;
            }



            if (Physics.Raycast(worldPosition, Vector3.down, out RaycastHit hit, _raycastHeight * 2f, _groundLayer)) {
                position = hit.point;
                normal = hit.normal;
                return true;
            }


            position = default;
            normal = default;

            return false;
        }



        private float GetZoneDensity(Vector3 point) {
            foreach (DensityZone zone in _densityZones) {
                if (zone.Collider == null)
                    continue;


                if (zone.Collider.bounds.Contains(point))
                    return zone.Density;
            }


            return -1f;
        }


        #endregion



        #region Placement


        protected virtual void SpawnAll(Transform parent = null) {
            if (Variants == null || Variants.Count == 0)
                return;


            WeightedRandom<GameObject> weighted = new(Variants);


            foreach (PlacementPoint point in PreviewPoints) {
                Spawn(point, parent, weighted);
            }
        }



        public virtual GameObject Spawn(PlacementPoint point, Transform parent, WeightedRandom<GameObject> weighted) {
            GameObject prefab = weighted.GetRandom();


            float scale = (float)_random.NextDouble() * (_scaleRange.y - _scaleRange.x) + _scaleRange.x;


            float yRotation = (float)_random.NextDouble() * 360f;


            Quaternion upright = Quaternion.Euler(0f, yRotation, 0f);


            Quaternion aligned = Quaternion.FromToRotation(Vector3.up, point.Normal) * upright;


            Quaternion rotation = Quaternion.Slerp(upright, aligned, _slopeInfluence);



            GameObject obj = Instantiate(prefab, point.Position, rotation, parent);


            obj.transform.localScale = Vector3.one * scale;


            PlacedObjects.Add(obj);


            return obj;
        }



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


            PlacedObjects.Clear();
            PreviewPoints.Clear();


#if UNITY_EDITOR
            if (_container != null)
                DestroyImmediate(_container);
#else
            if (_container != null)
                Destroy(_container);
#endif
        }



        private Transform CreateContainer() {
            _container = new GameObject("PlacedObjects");


            _container.transform.SetParent(transform);

            return _container.transform;
        }


        #endregion

        #region Poisson Disc


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



            Vector2 first = new((float)_random.NextDouble() * width, (float)_random.NextDouble() * height);



            points.Add(first);
            active.Add(0);


            grid[GridIndex(first, cellSize, gridW)] = 0;



            while (active.Count > 0) {
                int activeIndex = active[_random.Next(0, active.Count)];


                Vector2 current = points[activeIndex];


                bool found = false;



                for (int i = 0; i < samplesK; i++) {
                    float angle = (float)_random.NextDouble() * Mathf.PI * 2f;


                    float distance = minDist + (float)_random.NextDouble() * minDist;



                    Vector2 candidate = current + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;



                    if (candidate.x < 0 || candidate.x >= width || candidate.y < 0 || candidate.y >= height)
                        continue;



                    if (!IsFarEnough(candidate, points, grid, cellSize, gridW, gridH, minDist))
                        continue;



                    points.Add(candidate);


                    int index = points.Count - 1;


                    active.Add(index);



                    grid[GridIndex(candidate, cellSize, gridW)] = index;



                    found = true;

                    break;
                }



                if (!found)
                    active.Remove(activeIndex);
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
            float minDist
        ) {
            int cx = Mathf.FloorToInt(candidate.x / cellSize);


            int cz = Mathf.FloorToInt(candidate.y / cellSize);



            for (int z = -2; z <= 2; z++) {
                for (int x = -2; x <= 2; x++) {
                    int nx = cx + x;
                    int nz = cz + z;


                    if (nx < 0 || nz < 0 || nx >= gridW || nz >= gridH)
                        continue;



                    int stored = grid[nz * gridW + nx];



                    if (stored == -1)
                        continue;



                    if (Vector2.Distance(candidate, points[stored]) < minDist) {
                        return false;
                    }
                }
            }


            return true;
        }



        private int GridIndex(Vector2 point, float cellSize, int gridWidth) {
            int x = Mathf.FloorToInt(point.x / cellSize);


            int z = Mathf.FloorToInt(point.y / cellSize);


            return z * gridWidth + x;
        }


        #endregion



        #region Gizmos


        private void OnDrawGizmosSelected() {
            if (_showPreviewPoints && PreviewPoints != null) {
                Gizmos.color = new Color(0.2f, 0.9f, 0.2f, 0.8f);



                foreach (PlacementPoint point in PreviewPoints) {
                    Gizmos.DrawSphere(point.Position, _minDistance * 0.15f);


                    Gizmos.DrawLine(point.Position, point.Position + point.Normal);
                }
            }



            if (!_showDensityZones || _densityZones == null)
                return;



            foreach (DensityZone zone in _densityZones) {
                if (zone.Collider == null)
                    continue;



                float t = zone.Density;



                Color color = Color.Lerp(Color.red, Color.green, t);



                Gizmos.matrix = Matrix4x4.TRS(zone.Collider.bounds.center, zone.Collider.transform.rotation,
                    Vector3.one);



                Gizmos.color = new Color(color.r, color.g, color.b, 0.25f);



                Gizmos.DrawCube(Vector3.zero, zone.Collider.bounds.size);



                Gizmos.color = new Color(color.r, color.g, color.b, 0.9f);



                Gizmos.DrawWireCube(Vector3.zero, zone.Collider.bounds.size);



                Gizmos.matrix = Matrix4x4.identity;
            }
        }


        #endregion



        #region Debug


        [Button, BoxGroup("Debug")]
        private void GeneratePreview() {
            GeneratePoints();
        }



        [Button, BoxGroup("Debug")]
        private void PlaceObjects() {
            Clear();


            GeneratePoints();


            Transform parent = _createContainer ? CreateContainer() : null;


            SpawnAll(parent);
        }


        #endregion
    }
}