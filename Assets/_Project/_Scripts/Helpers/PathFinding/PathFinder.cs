using System.Collections.Generic;
using UnityEngine;

namespace Helpers.Pathfinding {
     public class PathFinder {
        
        #region PrivateFields
        private readonly LevelGrid _levelGrid;
        #endregion

        #region Properties
        public List<Marker> Path { get; private set; }
        #endregion

        #region Constructor
        public PathFinder(LevelGrid levelGrid) => _levelGrid = levelGrid;
        #endregion

        #region PathFinding
        public List<Marker> FindPath(Vector3 currentPosition, Vector3 targetPosition) {
            Marker startMarker = _levelGrid.GetMarkerFromPosition(currentPosition);
            Marker targetMarker = _levelGrid.GetMarkerFromPosition(targetPosition);

            List<Marker> openSet = new();
            HashSet<Marker> closedSet = new();

            openSet.Add(startMarker);

            while (openSet.Count > 0) {
                Marker currentMarker = openSet[0];

                for (int i = 1; i < openSet.Count; i++) {
                    if (openSet[i].FCost < currentMarker.FCost ||
                        openSet[i].FCost == currentMarker.FCost && openSet[i].HCost < currentMarker.HCost) {
                        currentMarker = openSet[i];
                    }
                }

                openSet.Remove(currentMarker);
                closedSet.Add(currentMarker);

                if (currentMarker == targetMarker)
                    return Path = RetracePath(startMarker, targetMarker);
                
                foreach (Marker neighborMarker in _levelGrid.GetNeighbors(currentMarker)) {
                    if (!neighborMarker.Walkable || closedSet.Contains(neighborMarker)) 
                        continue;

                    int costToNeighbor = currentMarker.GCost + GetDistance(currentMarker, neighborMarker);
                    
                    if (costToNeighbor < neighborMarker.GCost || !openSet.Contains(neighborMarker)) {
                        neighborMarker.GCost = costToNeighbor;
                        neighborMarker.HCost = GetDistance(neighborMarker, targetMarker);
                        neighborMarker.ParentMarker = currentMarker;

                        if (!openSet.Contains(neighborMarker)) 
                            openSet.Add(neighborMarker);
                    }
                }
            }

            return null;
        }

        private List<Marker> RetracePath(Marker startMarker, Marker endMarker) {
            List<Marker> path = new List<Marker>();
            Marker currentMarker = endMarker;

            while (currentMarker != startMarker) {
                path.Add(currentMarker);
                currentMarker = currentMarker.ParentMarker;
            }

            path.Reverse();
            
            return path;
        }

        private int GetDistance(Marker markerX, Marker markerY) {
            int distanceX = Mathf.Abs(markerX.GridMarkerX - markerY.GridMarkerX);
            int distanceY = Mathf.Abs(markerX.GridMarkerY - markerY.GridMarkerY);
            
            return distanceX + distanceY;
        }
        #endregion
    }
}