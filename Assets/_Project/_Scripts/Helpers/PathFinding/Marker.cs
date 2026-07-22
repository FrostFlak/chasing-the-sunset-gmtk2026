using UnityEngine;

namespace Helpers.Pathfinding {
    public class Marker {
        #region Properties
        public Vector3 Position { get; private set; }
        public bool Walkable { get; private set; }
        public int GCost { get; set; } // Cost from start to current position
        public int HCost { get; set; } // Cost to goal 
        public int FCost => GCost + HCost;
        public Marker ParentMarker { get; set; }
        public int GridMarkerX { get; private set; }
        public int GridMarkerY { get; private set; }
        #endregion

        #region Constructor
        public Marker(
            bool walkable,
            Vector3 pos,
            int gridMarkerX,
            int gridMarkerY
        ) {
            Walkable = walkable;
            Position = pos;
            GridMarkerX = gridMarkerX;
            GridMarkerY = gridMarkerY;
        }
        #endregion

    }
}