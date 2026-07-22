using System.Collections.Generic;
using UnityEngine;

namespace Helpers.Pathfinding {
    public class LevelGrid : MonoBehaviour {
       
        #region SerializedFields
        [Header("Components")]
        [SerializeField, Tooltip("The walkable map plane")] private Transform _plane;
        [Header("GridProperties")]
        [SerializeField, Tooltip("Radius of grid cell")] private float _cellRadius;
        [SerializeField, Tooltip("Masks that are not walkable, like <Obstacle> or else")] private LayerMask _unwalkableMask;
        [SerializeField] private bool _showGridGizmos;
        #endregion

        #region PrivateFields
        private const int ScaleToMetersMultiplier = 10;

        private Vector2 _gridWorldSize;
        private Marker[,] _grid;
        private float _cellDiameter;
        private int _gridSizeX;
        private int _gridSizeY; 
        #endregion

        #region MonoBehavior
        public void Awake() {
            _gridWorldSize = new Vector2(_plane.transform.localScale.x, _plane.transform.localScale.z) * ScaleToMetersMultiplier;
            CreateGrid();
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (_grid == null)
                return;

            if (!_showGridGizmos)
                return;
            
            // Grid
            Gizmos.color = Color.green;
            foreach (Marker marker in _grid) {
                Gizmos.color = marker.Walkable ? Color.green : Color.red;
                Gizmos.DrawCube(marker.Position, Vector3.one * (_cellDiameter - 0.1f));
            }
            
            // Grid borders
            Gizmos.color = Color.white;
            for (int x = 0; x < _gridSizeX; x++) {
                for (int y = 0; y < _gridSizeY; y++) {
                    Vector3 startPosition = _grid[x, y].Position;
                    Vector3 endPosition = _grid[x, y].Position;
            
                    // Horizontal lines
                    if (x + 1 < _gridSizeX) {
                        endPosition = _grid[x + 1, y].Position;
                        Gizmos.DrawLine(startPosition, endPosition);
                    }
            
                    // Vertical lines
                    if (y + 1 < _gridSizeY) {
                        endPosition = _grid[x, y + 1].Position;
                        Gizmos.DrawLine(startPosition, endPosition);
                    }
                }
            }
        }
        #endif
        #endregion

        #region Grid
        private void CreateGrid() {
            if (_cellRadius == 0) {
                Log.Error("Cell radius is 0");
                
                return;
            }

            if (_gridWorldSize == Vector2.zero) {
                Log.Error("Grid world size is 0");
                
                return;
            }

            _cellDiameter = _cellRadius * 2;
            _gridSizeX = Mathf.RoundToInt(_gridWorldSize.x / _cellDiameter);
            _gridSizeY = Mathf.RoundToInt(_gridWorldSize.y / _cellDiameter);

            _grid = new Marker[_gridSizeX, _gridSizeY];
            Vector3 worldBottomLeft = _plane.transform.position - Vector3.right * _gridWorldSize.x / 2 - Vector3.forward * _gridWorldSize.y / 2;

            Collider[] overlapBuffer = new Collider[1];

            for (int x = 0; x < _gridSizeX; x++) {
                for (int y = 0; y < _gridSizeY; y++) {
                    Vector3 basePosition = worldBottomLeft + Vector3.right * (x * _cellDiameter + _cellRadius) + Vector3.forward * (y * _cellDiameter + _cellRadius);
            
                    Vector3 rayOrigin = basePosition + Vector3.up * 50f;
                    Vector3 finalPosition = basePosition;

                    if (gameObject.scene.GetPhysicsScene().Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, ~_unwalkableMask, QueryTriggerInteraction.Ignore))
                        finalPosition.y = hit.point.y;

                    int hitCount = gameObject.scene.GetPhysicsScene().OverlapSphere(finalPosition, _cellRadius, overlapBuffer, _unwalkableMask, QueryTriggerInteraction.Ignore);
                    bool walkable = hitCount == 0;

                    _grid[x, y] = new Marker(walkable, finalPosition, x, y);
                }
            }
        }
        #endregion

        #region Getters
        public Marker GetMarkerFromPosition(Vector3 worldPosition) {
            Vector3 worldBottomLeft = _plane.transform.position - Vector3.right * _gridWorldSize.x / 2 - Vector3.forward * _gridWorldSize.y / 2;

            float percentX = Mathf.Clamp01((worldPosition.x - worldBottomLeft.x) / _gridWorldSize.x);
            float percentY = Mathf.Clamp01((worldPosition.z - worldBottomLeft.z) / _gridWorldSize.y);

            int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

            return _grid[x, y];
        }

        
        public List<Marker> GetNeighbors(Marker marker) {
            List<Marker> neighbors = new();

            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (i == 0 && j == 0) continue;

                    int checkX = marker.GridMarkerX + i;
                    int checkY = marker.GridMarkerY + j;

                    if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
                        neighbors.Add(_grid[checkX, checkY]);
                }
            }

            return neighbors;
        }
        
        public Marker GetRandomMarkerInRadius(Vector3 startPosition, float minRadius, float maxRadius) {
            Marker centerMarker = GetMarkerFromPosition(startPosition);
            int centerX = centerMarker.GridMarkerX;
            int centerY = centerMarker.GridMarkerY;

            int maxCellRadius = Mathf.CeilToInt(maxRadius / _cellDiameter);

            int minX = Mathf.Max(centerX - maxCellRadius, 0);
            int maxX = Mathf.Min(centerX + maxCellRadius, _gridSizeX - 1);
            int minY = Mathf.Max(centerY - maxCellRadius, 0);
            int maxY = Mathf.Min(centerY + maxCellRadius, _gridSizeY - 1);

            int findAttempts = 50;

            for (int i = 0; i < findAttempts; i++) {
                int randX = Random.Range(minX, maxX + 1);
                int randY = Random.Range(minY, maxY + 1);

                Marker marker = _grid[randX, randY];
                float distance = Vector3.Distance(marker.Position, startPosition);

                if (marker.Walkable && distance >= minRadius && distance <= maxRadius)
                    return marker;
            }

            for (int dx = -1; dx <= 1; dx++) {
                for (int dy = -1; dy <= 1; dy++) {
                    int x = centerX + dx;
                    int y = centerY + dy;

                    if (x >= 0 && x < _gridSizeX && y >= 0 && y < _gridSizeY) {
                        Marker fallback = _grid[x, y];
                        if (fallback.Walkable)
                            return fallback;
                    }
                }
            }

            return centerMarker;
        }
        #endregion
    }
}