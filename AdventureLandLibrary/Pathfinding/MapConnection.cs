using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdventureLandLibrary.Geometry;

namespace AdventureLandLibrary.Pathfinding
{
    public struct MapConnection
    {
        public bool isRoot;
        public int SpawnID;
        public PointStruct SpawnPoint;
        public string MapName;
        public string ConnectedMap;
        public int ConnectedSpawnID;
        public PointStruct ConnectedSpawnPoint;
    }
}
