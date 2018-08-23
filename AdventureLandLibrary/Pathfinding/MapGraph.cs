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
        Line[] edges;

        int xOffset;
        int yOffset;

        public MapGraph(TriangleNet.Mesh mesh, int xOffset, int yOffset)
        {
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            CreateCentroidVertexGraph(mesh);

            List<Line> edges = new List<Line>();
            foreach (var seg in mesh.Segments)
            {
                var p1 = new Point(seg.GetVertex(0));
                var p2 = new Point(seg.GetVertex(1));

                edges.Add(new Line(p1, p2));
            }

            this.edges = edges.ToArray();
        }

        private void CreateCentroidVertexGraph(TriangleNet.Mesh mesh)
        {
            pointMapping = new Dictionary<PointStruct, uint>();
            graph = new Graph<Point, string>();

            pointMapping = new Dictionary<PointStruct, uint>();
            graph = new Graph<Point, string>();

            foreach (var tri in mesh.Triangles)
            {
                var vertices = new List<PointStruct>();

                var centroid = new PointStruct(tri.GetCentroid());

                if (!pointMapping.ContainsKey(centroid))
                {
                    var id = graph.AddNode(new Point(centroid.X, centroid.Y));
                    pointMapping.Add(centroid, id);
                }

                var cID = pointMapping[centroid];

                for (var i = 0; i < 3; i++)
                {
                    var neighbor = tri.GetNeighbor(i);

                    if (neighbor != null)
                    {
                        var neighborCentroid = new PointStruct(((TriangleNet.Topology.Triangle)neighbor).GetCentroid());

                        if (!pointMapping.ContainsKey(neighborCentroid))
                        {
                            var id = graph.AddNode(new Point(neighborCentroid.X, neighborCentroid.Y));
                            pointMapping.Add(neighborCentroid, id);
                        }


                        var nID = pointMapping[neighborCentroid];

                        var dist = centroid.Distance(neighborCentroid);

                        graph.Connect(cID, nID, (int)Math.Ceiling(dist), "");
                    }

                    for (var x = 0; x < 3; x++)
                    {
                        var iPoint = new PointStruct(tri.GetVertex(i));
                        if (!pointMapping.ContainsKey(iPoint))
                        {
                            var id = graph.AddNode(new Point(iPoint.X, iPoint.Y));
                            pointMapping.Add(iPoint, id);
                        }
                        var iID = pointMapping[iPoint];
                        var cdist = iPoint.Distance(centroid);
                        graph.Connect(cID, iID, (int)cdist, "");
                        graph.Connect(iID, cID, (int)cdist, "");
                        if (i != x)
                        {
                            var xPoint = new PointStruct(tri.GetVertex(x));



                            if (!pointMapping.ContainsKey(xPoint))
                            {
                                var id = graph.AddNode(new Point(xPoint.X, xPoint.Y));
                                pointMapping.Add(xPoint, id);
                            }


                            var xID = pointMapping[xPoint];

                            var dist = iPoint.Distance(xPoint);



                            graph.Connect(iID, xID, (int)Math.Ceiling(dist), "");

                        }
                    }
                }
            }
        }

        private void CreateVertexGraph(TriangleNet.Mesh mesh)
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

        private void CreateCentroidGraph(TriangleNet.Mesh mesh)
        {
            pointMapping = new Dictionary<PointStruct, uint>();
            graph = new Graph<Point, string>();


            foreach (var tri in mesh.Triangles)
            {
                var centroid = new PointStruct(tri.GetCentroid());

                for (var i = 0; i < 3; i++)
                {
                    var neighbor = tri.GetNeighbor(i);

                    if (neighbor != null)
                    {
                        var neighborCentroid = new PointStruct(((TriangleNet.Topology.Triangle)neighbor).GetCentroid());

                        if (!pointMapping.ContainsKey(centroid))
                        {
                            var id = graph.AddNode(new Point(centroid.X, centroid.Y));
                            pointMapping.Add(centroid, id);
                        }

                        if (!pointMapping.ContainsKey(neighborCentroid))
                        {
                            var id = graph.AddNode(new Point(neighborCentroid.X, neighborCentroid.Y));
                            pointMapping.Add(neighborCentroid, id);
                        }

                        var iID = pointMapping[centroid];
                        var xID = pointMapping[neighborCentroid];

                        var dist = centroid.Distance(neighborCentroid);

                        graph.Connect(iID, xID, (int)Math.Ceiling(dist), "");
                    }
                }
            }
        }

        private uint? GetNearestNode(PointStruct point)
        {
            PointStruct? closestPoint = null;
            double closestDistance = 0;

            var sortedKeys = pointMapping.Keys.OrderBy(e => e.Distance(point));

            foreach (var key in sortedKeys)
            {

                var crossesEdge = LineCrossesEdge(new Line(new Point(point.X, point.Y), new Point(key.X, key.Y)));
                if (!crossesEdge)
                {
                    return pointMapping[key];
                }
            }

            return null;
        }

        private bool LineCrossesEdge(Line line)
        {
            var crosses = false;

            foreach (var edge in edges)
            {
                if (edge.Intersects(line))
                {
                    crosses = true;
                }
            }

            return crosses;
        }

        public Point[] GetPath(Point from, Point to)
        {
            var nodeFrom = GetNearestNode(new PointStruct(from.X + xOffset, from.Y + yOffset));
            var nodeTo = GetNearestNode(new PointStruct(to.X + xOffset, to.Y + yOffset));

            if (nodeFrom != null && nodeTo != null)
            {
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

                timer.Start();
                var shortestPath = graph.Dijkstra(nodeFrom.Value, nodeTo.Value);
                timer.Stop();

                var path = shortestPath.GetPath();
                List<Point> pathPoints = new List<Point>();
                pathPoints.Add(from);
                foreach (uint node in path)
                {
                    var point = graph[node].Item;

                    pathPoints.Add(new Point(point.X - xOffset, point.Y - yOffset));
                }
                pathPoints.Add(to);

                return pathPoints.ToArray();
            }
            else
            {
                return new Point[0];
            }
        }
    }
}
