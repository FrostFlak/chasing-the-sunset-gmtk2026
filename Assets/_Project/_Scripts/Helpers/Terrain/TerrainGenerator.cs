using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Helpers.Terrain {
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class TerrainGenerator : MonoBehaviour {

        #region SerializedFields
        [Header("Grid")]
        [SerializeField, Range(2, 100)] private int _resolution = 30;
        [SerializeField] private bool _squaredSize = false;
        [SerializeField, Min(0)] private int _sizeX = 100;
        [SerializeField, HideIf(nameof(_squaredSize))] private int _sizeZ = 120;
        [Header("Height")]
        [SerializeField] private TerrainMode _terrainMode = TerrainMode.Generated;
        [SerializeField, Min(0f)] private float _heightScale = 4f;
        [SerializeField, HideIf(nameof(IsHeightmapMode)), Range(0f, 0.25f)] private float _noiseScale = 0.08f;
        [SerializeField, HideIf(nameof(IsHeightmapMode)), Range(0, 999999)] private int _seed = 0;
        [SerializeField, HideIf(nameof(IsGeneratedMode))] private Texture2D _heightmap;
        [Header("Chunks")]
        [SerializeField] private bool _useChunks = false;
        [SerializeField, Range(1, 16)] private int _chunkCount = 4;
        [Header("Boundary Mountains")]
        [SerializeField, HideIf(nameof(IsHeightmapMode))] private bool _boundaryMountains = false;
        [SerializeField, HideIf(nameof(IsHeightmapMode)), Range(1f, 60f)] private float _mountainHeight = 12f;
        [SerializeField, HideIf(nameof(IsHeightmapMode)), Range(1f, 60f)] private float _mountainWidth = 10f;
        [SerializeField, HideIf(nameof(IsHeightmapMode)), Range(0f, 1f)] private float _mountainRoughness = 0.6f;
        [Header("Save")]
        [SerializeField] private string _savePath = "Assets/_Project/Terrains";
        #endregion

        #region PrivateFields
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private TerrainZone[] _zones;
        private Texture2D _readableHeightmap;
        [SerializeField, HideInInspector] private List<GameObject> _chunkObjects = new();
        #endregion

        #region Properties
        public int Seed => _seed;
        public bool SquaredSize => _squaredSize;
        public IReadOnlyList<GameObject> ChunkObjects => _chunkObjects;
        private float SizeZ => _squaredSize ? _sizeX : _sizeZ;
        private bool IsGeneratedMode => _terrainMode == TerrainMode.Generated;
        private bool IsHeightmapMode => _terrainMode == TerrainMode.Heightmap;
        #endregion

        #region Unity
        private void OnEnable() => Generate();

        private void OnValidate() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += OnDelayCall;
#else
            Generate();
#endif
        }

        private void OnDisable() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall -= OnDelayCall;
#endif
        }

        private void OnDelayCall() {
            if (this == null) 
                return;
            
            Generate();
        }
        #endregion

        #region Generation
        public void Generate() {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();
            _zones = GetComponentsInChildren<TerrainZone>();
            _readableHeightmap = _heightmap != null ? MakeReadable(_heightmap) : null;

            ClearChunks();

            if (_useChunks) {
                BuildChunks();
                _meshFilter.sharedMesh = null;
                _meshCollider.sharedMesh = null;
                _meshRenderer.enabled = false;
            } else {
                _meshRenderer.enabled = true;
                Mesh mesh = BuildMesh();
                _meshFilter.sharedMesh = mesh;
                _meshCollider.sharedMesh = mesh;
            }
        }

        private void BuildChunks() {
            float sizeZ = SizeZ;

            float aspect = _sizeX / sizeZ;
            int chunksZ = Mathf.Max(1, Mathf.RoundToInt(Mathf.Sqrt(_chunkCount / aspect)));
            int chunksX = Mathf.Max(1, Mathf.CeilToInt((float)_chunkCount / chunksZ));

            float longest = Mathf.Max(_sizeX, sizeZ);
            int totalResX = Mathf.Max(chunksX, Mathf.RoundToInt(_resolution * (_sizeX / longest)));
            int totalResZ = Mathf.Max(chunksZ, Mathf.RoundToInt(_resolution * (sizeZ / longest)));

            totalResX = Mathf.CeilToInt((float)totalResX / chunksX) * chunksX;
            totalResZ = Mathf.CeilToInt((float)totalResZ / chunksZ) * chunksZ;

            int chunkResX = totalResX / chunksX;
            int chunkResZ = totalResZ / chunksZ;
            float stepX = (float)_sizeX / totalResX;
            float stepZ = sizeZ / totalResZ;
            float offsetX = _seed * 100f;
            float offsetZ = _seed * 73f;

            var parentRenderer = GetComponent<MeshRenderer>();

            for (int cz = 0; cz < chunksZ; cz++)
            for (int cx = 0; cx < chunksX; cx++) {
                var chunkGo = new GameObject($"Chunk_{cx}_{cz}");
                chunkGo.transform.SetParent(transform);
                chunkGo.transform.localPosition = Vector3.zero;
                chunkGo.transform.localRotation = Quaternion.identity;
                chunkGo.transform.localScale = Vector3.one;

                var mf = chunkGo.AddComponent<MeshFilter>();
                var mr = chunkGo.AddComponent<MeshRenderer>();
                var mc = chunkGo.AddComponent<MeshCollider>();

                if (parentRenderer != null)
                    mr.sharedMaterials = parentRenderer.sharedMaterials;

                Mesh mesh = BuildChunkMesh(cx, cz, chunkResX, chunkResZ, totalResX, totalResZ, stepX, stepZ, offsetX, offsetZ);
                mf.sharedMesh = mesh;
                mc.sharedMesh = mesh;

                _chunkObjects.Add(chunkGo);
            }
        }

        private Mesh BuildChunkMesh(int cx, int cz, int chunkResX, int chunkResZ, int totalResX, int totalResZ, float stepX, float stepZ, float ox, float oz) {
            int startX = cx * chunkResX;
            int startZ = cz * chunkResZ;
            int vCountX = chunkResX + 1;
            int vCountZ = chunkResZ + 1;
            float sizeZ = SizeZ;

            var vertices = new Vector3[vCountX * vCountZ];
            var uvs = new Vector2[vCountX * vCountZ];
            var triangles = new int[chunkResX * chunkResZ * 6];

            for (int z = 0; z <= chunkResZ; z++)
            for (int x = 0; x <= chunkResX; x++) {
                int i = z * vCountX + x;
                float nx = (startX + x) * stepX;
                float nz = (startZ + z) * stepZ;

                float height = IsHeightmapMode && _readableHeightmap != null
                    ? _readableHeightmap.GetPixelBilinear(nx / _sizeX, nz / SizeZ).grayscale * _heightScale
                    : SampleHeight(nx, nz, ox, oz);

                if (_boundaryMountains && IsGeneratedMode)
                    height = ApplyBoundaryMountains(height, nx, nz, ox, oz);

                Vector3 worldPos = transform.TransformPoint(new Vector3(nx - _sizeX * 0.5f, 0f, nz - sizeZ * 0.5f));

                if (_zones != null)
                    foreach (TerrainZone zone in _zones)
                        height = zone.Apply(height, worldPos, transform);

                vertices[i] = new Vector3(nx - _sizeX * 0.5f, height, nz - sizeZ * 0.5f);
                uvs[i] = new Vector2((float)(startX + x) / totalResX, (float)(startZ + z) / totalResZ);
            }

            int t = 0;
            for (int z = 0; z < chunkResZ; z++)
            for (int x = 0; x < chunkResX; x++) {
                int bl = z * vCountX + x;
                int br = bl + 1;
                int tl = bl + vCountX;
                int tr = tl + 1;

                triangles[t++] = bl; triangles[t++] = tl; triangles[t++] = tr;
                triangles[t++] = bl; triangles[t++] = tr; triangles[t++] = br;
            }

            var mesh = new Mesh { name = $"TerrainChunk_{_seed}_{cx}_{cz}" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void ClearChunks() {
            foreach (GameObject go in _chunkObjects) {
                if (go == null)
                    continue;
#if UNITY_EDITOR
                DestroyImmediate(go);
#else
                Destroy(go);
#endif
            }
            _chunkObjects.Clear();
        }

        private Mesh BuildMesh() {
            float sizeZ = SizeZ;
            int resX = Mathf.Max(1, Mathf.RoundToInt(_resolution * (_sizeX / Mathf.Max(_sizeX, sizeZ))));
            int resZ = Mathf.Max(1, Mathf.RoundToInt(_resolution * (sizeZ / Mathf.Max(_sizeX, sizeZ))));
            int vCountX = resX + 1;
            int vCountZ = resZ + 1;
            float stepX = (float)_sizeX / resX;
            float stepZ = sizeZ / resZ;
            float offsetX = _seed * 100f;
            float offsetZ = _seed * 73f;

            var vertices = new Vector3[vCountX * vCountZ];
            var uvs = new Vector2[vCountX * vCountZ];
            var triangles = new int[resX * resZ * 6];

            for (int z = 0; z <= resZ; z++)
            for (int x = 0; x <= resX; x++) {
                int i = z * vCountX + x;
                float nx = x * stepX;
                float nz = z * stepZ;

                float height = IsHeightmapMode && _readableHeightmap != null
                    ? _readableHeightmap.GetPixelBilinear(nx / _sizeX, nz / SizeZ).grayscale * _heightScale
                    : SampleHeight(nx, nz, offsetX, offsetZ);

                if (_boundaryMountains && IsGeneratedMode)
                    height = ApplyBoundaryMountains(height, nx, nz, offsetX, offsetZ);

                Vector3 worldPos = transform.TransformPoint(new Vector3(nx - _sizeX * 0.5f, 0f, nz - sizeZ * 0.5f));

                if (_zones != null)
                    foreach (TerrainZone zone in _zones)
                        height = zone.Apply(height, worldPos, transform);

                vertices[i] = new Vector3(nx - _sizeX * 0.5f, height, nz - sizeZ * 0.5f);
                uvs[i] = new Vector2((float)x / resX, (float)z / resZ);
            }

            int t = 0;
            for (int z = 0; z < resZ; z++)
            for (int x = 0; x < resX; x++) {
                int bl = z * vCountX + x;
                int br = bl + 1;
                int tl = bl + vCountX;
                int tr = tl + 1;

                triangles[t++] = bl; triangles[t++] = tl; triangles[t++] = tr;
                triangles[t++] = bl; triangles[t++] = tr; triangles[t++] = br;
            }

            var mesh = new Mesh { name = $"Terrain_{_seed}" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private float ApplyBoundaryMountains(float height, float nx, float nz, float ox, float oz) {
            float distFromEdgeX = Mathf.Min(nx, _sizeX - nx);
            float distFromEdgeZ = Mathf.Min(nz, SizeZ - nz);
            float distFromEdge = Mathf.Min(distFromEdgeX, distFromEdgeZ);

            float t = 1f - Mathf.Clamp01(distFromEdge / _mountainWidth);
            if (t <= 0f)
                return height;

            t = t * t;

            float ridge1 = RidgedNoise((nx + ox) * 0.05f, (nz + oz) * 0.05f);
            float ridge2 = RidgedNoise((nx + ox) * 0.11f, (nz + oz) * 0.11f) * 0.5f;
            float ridge3 = RidgedNoise((nx + ox) * 0.23f, (nz + oz) * 0.23f) * 0.25f;
            float ridged = (ridge1 + ridge2 + ridge3) / 1.75f;

            float baseNoise = Mathf.PerlinNoise((nx + ox) * 0.03f, (nz + oz) * 0.03f);
            float peakHeight = _mountainHeight * (0.6f + baseNoise * 0.4f);
            float finalPeak = Mathf.Lerp(peakHeight * 0.5f, peakHeight, Mathf.Lerp(1f, ridged, _mountainRoughness));

            return Mathf.Lerp(height, finalPeak, t);
        }

        private static float RidgedNoise(float x, float z) {
            float n = Mathf.PerlinNoise(x, z);
            return 1f - Mathf.Abs(n * 2f - 1f);
        }

        private float SampleHeight(float x, float z, float ox, float oz) {
            float h = Mathf.PerlinNoise((x + ox) * _noiseScale, (z + oz) * _noiseScale) * 1.00f;
                  h += Mathf.PerlinNoise((x + ox) * _noiseScale * 2.7f, (z + oz) * _noiseScale * 2.7f) * 0.35f;
                  h += Mathf.PerlinNoise((x + ox) * _noiseScale * 6.1f, (z + oz) * _noiseScale * 6.1f) * 0.10f;
            return h * _heightScale;
        }

        private static Texture2D MakeReadable(Texture2D source) {
#if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GetAssetPath(source);
            if (!string.IsNullOrEmpty(path)) {
                var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
                if (importer != null) {
                    bool needsSave = false;
                    if (!importer.isReadable)                          { importer.isReadable = true;                          needsSave = true; }
                    if (importer.wrapMode != TextureWrapMode.Clamp)    { importer.wrapMode   = TextureWrapMode.Clamp;         needsSave = true; }
                    if (needsSave) {
                        importer.SaveAndReimport();
                        source = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    }
                }
                return source;
            }
#endif
            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            var readable = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);
            readable.wrapMode = TextureWrapMode.Clamp;
            readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readable.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return readable;
        }
        #endregion

#if UNITY_EDITOR
        #region Saving
        [Button]
        private void SaveMesh() {
            if (_meshFilter == null || _meshFilter.sharedMesh == null) {
                Debug.LogWarning("[TerrainGenerator] No mesh to save.");
                return;
            }

            System.IO.Directory.CreateDirectory(_savePath);
            string path = $"{_savePath.TrimEnd('/')}/Terrain_{_seed}.asset";

            var copy = Object.Instantiate(_meshFilter.sharedMesh);
            copy.name = $"Terrain_{_seed}";

            UnityEditor.AssetDatabase.CreateAsset(copy, path);
            UnityEditor.AssetDatabase.SaveAssets();

            _meshFilter.sharedMesh = copy;
            _meshCollider.sharedMesh = copy;

            Debug.Log($"[TerrainGenerator] Saved → {path}");
        }
        #endregion
#endif
    }

    public enum TerrainMode { Generated, Heightmap }
}
