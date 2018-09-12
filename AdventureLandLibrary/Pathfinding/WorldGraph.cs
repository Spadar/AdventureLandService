using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dijkstra.NET.Extensions;
using AdventureLandLibrary.Geometry;
using Dijkstra.NET.Model;
using AdventureLandLibrary.GameObjects;
using AdventureLandLibrary.Global;
using Newtonsoft.Json.Linq;

namespace AdventureLandLibrary.Pathfinding
{
    public class WorldGraph
    {
        //Graph<Point, string> graph;
        Dictionary<MapConnection, uint> mapMapping;
        Dictionary<string, MapConnection> mapRootMapping;
        Graph<MapConnection, string> graph;

        public WorldGraph()
        {
            CreateMapGraph();
        }

        private void CreateMapGraph()
        {
            graph = new Graph<MapConnection, string>();
            mapMapping = new Dictionary<MapConnection, uint>();
            mapRootMapping = new Dictionary<string, MapConnection>();

            //Create Nodes
            foreach (var map in Maps.MapDictionary.Values)
            {
                if (!mapRootMapping.ContainsKey(map.MapID))
                {
                    var rootConnection = new MapConnection();
                    rootConnection.MapName = map.MapID;
                    rootConnection.isRoot = true;

                    var id = graph.AddNode(rootConnection);
                    mapMapping.Add(rootConnection, id);
                    mapRootMapping.Add(map.MapID, rootConnection);
                }

                var rootID = mapMapping[mapRootMapping[map.MapID]];

                int maxDist = 0;

                foreach (var connection in map.Connections)
                {
                    if (!mapMapping.ContainsKey(connection))
                    {
                        var id = graph.AddNode(connection);
                        mapMapping.Add(connection, id);
                    }

                    var connectionID = mapMapping[connection];

                    //Connect to all outbound maps
                    var connectedList = Maps.MapDictionary[connection.ConnectedMap].Connections.Where(c => c.SpawnID == connection.ConnectedSpawnID && c.ConnectedSpawnID == connection.SpawnID).ToList();

                    if (connectedList.Count > 0)
                    {
                        var connected = connectedList[0];
                        if (!mapMapping.ContainsKey(connected))
                        {
                            var id = graph.AddNode(connected);
                            mapMapping.Add(connected, id);
                        }

                        var connectedID = mapMapping[connected];

                        graph.Connect(connectionID, connectedID, 1, "");
                    }

                    //Connect this connection to all other connections in this map
                    foreach (var otherConnection in map.Connections)
                    {
                        if (otherConnection.SpawnID != connection.SpawnID && otherConnection.ConnectedSpawnID != connection.ConnectedSpawnID)
                        {
                            if (!mapMapping.ContainsKey(otherConnection))
                            {
                                var id = graph.AddNode(otherConnection);
                                mapMapping.Add(otherConnection, id);
                            }

                            var otherConnectionID = mapMapping[otherConnection];

                            var path = map.FindPath(new Point(connection.SpawnPoint), new Point(otherConnection.SpawnPoint));
                            var smoothed = map.SmoothPath(path);

                            int dist = (int)map.PathDistance(smoothed);
                            if (dist > maxDist)
                            {
                                maxDist = dist;
                            }
                            //Placeholder cost of 1. May want to caculate a path and evaluate path distance as cost. 
                            graph.Connect(connectionID, otherConnectionID, dist, "");
                        }
                    }
                }

                foreach (var connection in map.Connections)
                {
                    if (!mapMapping.ContainsKey(connection))
                    {
                        var id = graph.AddNode(connection);
                        mapMapping.Add(connection, id);
                    }

                    var connectionID = mapMapping[connection];

                    //Connect to root
                    graph.Connect(rootID, connectionID, maxDist*2, "");
                    graph.Connect(connectionID, rootID, maxDist*2, "");
                }
            }
        }


        public List<MapConnection> GetPath(string from, string to)
        {
            if(mapRootMapping.ContainsKey(from) && mapRootMapping.ContainsKey(to))
            {
                var fromConnection = mapRootMapping[from];
                var toConnection = mapRootMapping[to];

                var nodeFrom = mapMapping[fromConnection];
                var nodeTo = mapMapping[toConnection];

                var shortestPath = graph.Dijkstra(nodeFrom, nodeTo);

                var path = shortestPath.GetPath();

                List<MapConnection> mapPath = new List<MapConnection>();
                foreach (var node in path)
                {
                    var connection = graph[node].Item;

                    if(!connection.isRoot)
                    {
                        mapPath.Add(connection);
                    }
                }

                return mapPath;
            }
            else
            {
                return new List<MapConnection>();
            }
        }
    }
}

