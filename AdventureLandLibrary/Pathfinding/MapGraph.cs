using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dijkstra.NET.Extensions;
using AdventureLandLibrary.Geometry;
using Dijkstra.NET.Model;

namespace AdventureLandLibrary.Pathfinding
{
    public class MapGraph
    {
        Graph<Point, string> graph;
        Dictionary<PointStruct, uint> pointMapping;

        int xOffset;
        int yOffset;

        public MapGraph(TriangleNet.Mesh mesh, int xOffset, int yOffset)
        {
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            CreateGraph(mesh);
        }



        private void CreateGraph(TriangleNet.Mesh mesh)
        {
            pointMapping = new Dictionary<PointStruct, uint>();
            graph = new Graph<Point, string>();


            foreach (var tri in mesh.Triangles)
            {
                var vertices = new List<PointStruct>();

                for (var i = 0; i < 3; i++)
                {
                    for (var x = 0; x < 3; x++)
                    {
                        if (i != x)
                        {
                            var iPoint = new PointStruct(tri.GetVertex(i));
                            var xPoint = new PointStruct(tri.GetVertex(x));

                            if (!pointMapping.ContainsKey(iPoint))
                            {
                                var id = graph.AddNode(new Point(iPoint.X, iPoint.Y));
                                pointMapping.Add(iPoint, id);
                            }

                            if (!pointMapping.ContainsKey(xPoint))
                            {
                                var id = graph.AddNode(new Point(xPoint.X, xPoint.Y));
                                pointMapping.Add(xPoint, id);
                            }

                            var iID = pointMapping[iPoint];
                            var xID = pointMapping[xPoint];

                            var dist = iPoint.Distance(xPoint);

                            graph.Connect(iID, xID, (int)Math.Ceiling(dist), "");
                        }
                    }
                }
            }
        }

        private uint GetNearestNode(PointStruct point)
        {
            PointStruct? closestPoint = null;
            double closestDistance = 0;

            foreach (var key in pointMapping.Keys)
            {
                var dist = key.Distance(point);
                if (closestPoint == null || dist < closestDistance)
                {
                    closestPoint = key;
                    closestDistance = dist;
                }
            }

            return pointMapping[closestPoint.Value];
        }

        public Point[] GetPath(Point from, Point to)
        {
            var nodeFrom = GetNearestNode(new PointStruct(from.X + xOffset, from.Y + yOffset));
            var nodeTo = GetNearestNode(new PointStruct(to.X + xOffset, to.Y + yOffset));

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            timer.Start();
            var shortestPath = graph.Dijkstra(nodeFrom, nodeTo);
            timer.Stop();

            var path = shortestPath.GetPath();
            List<Point> pathPoints = new List<Point>();
            pathPoints.Add(from);
            foreach(uint node in path)
            {
                var point = graph[node].Item;

                pathPoints.Add(new Point(point.X - xOffset, point.Y - yOffset));
            }
            pathPoints.Add(to);

            return pathPoints.ToArray();
        }
    }
}
