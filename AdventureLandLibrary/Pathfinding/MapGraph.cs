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
        //Graph<Point, string> graph;
        Dictionary<PointStruct, uint> pointMapping;
        Graph<GraphNode, string> triGraph;
        Line[] exteriorEdges;
        Line[] interiorEdges;
        PointMap pointMap;

        int xOffset;
        int yOffset;

        public MapGraph(TriangleNet.Mesh mesh, PointMap pointMap, int xOffset, int yOffset)
        {
            this.pointMap = pointMap;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            CreateTriangleGraph(mesh);

            List<Line> edges = new List<Line>();
            foreach (var seg in mesh.Segments)
            {
                var p1 = new Point(seg.GetVertex(0));
                var p2 = new Point(seg.GetVertex(1));

                edges.Add(new Line(p1, p2));
            }

            this.interiorEdges = edges.ToArray();

            this.exteriorEdges = pointMap.GetEdges();
        }

        private void CreateTriangleGraph(TriangleNet.Mesh mesh)
        {
            triGraph = new Graph<GraphNode, string>();
            pointMapping = new Dictionary<PointStruct, uint>();

            foreach (var tri in mesh.Triangles)
            {
                GraphNode node = new GraphNode(tri);


                if (!pointMapping.ContainsKey(node.center))
                {
                    var id = triGraph.AddNode(node);
                    pointMapping.Add(node.center, id);
                }

                var cID = pointMapping[node.center];

                for (var i = 0; i < 3; i++)
                {
                    var neighbor = (TriangleNet.Topology.Triangle)tri.GetNeighbor(i);

                    if (neighbor != null)
                    {
                        var neighborNode = new GraphNode(neighbor);

                        if (!pointMapping.ContainsKey(neighborNode.center))
                        {
                            var id = triGraph.AddNode(neighborNode);
                            pointMapping.Add(neighborNode.center, id);
                        }


                        var nID = pointMapping[neighborNode.center];

                        var dist = node.center.Distance(neighborNode.center);

                        var portal = node.GetPortal(neighborNode);

                        triGraph.Connect(cID, nID, (int)Math.Ceiling(dist), "");
                    }
                }
            }
        }

        public List<Point> TunnelSmooth(List<GraphNode> path)
        {
            if (path.Count > 6)
            {
                List<Point> newPath = new List<Point>();

                List<LineD> portals = new List<LineD>();

                for (var i = 0; i < path.Count - 2; i++)
                {
                    portals.Add(path[i].GetPortal(path[i + 1]));
                }

                PointD currentNode = new PointD(path[0].center.X, path[0].center.Y);

                newPath.Add(new Point((int)currentNode.X - xOffset, (int)currentNode.Y - yOffset));

                int funnelLeftIndex = 1;
                int funnelRightIndex = 1;

                int currentPortal = 0;

                LineD funnelLeft = new LineD(currentNode, portals[0].P2);
                LineD funnelRight = new LineD(currentNode, portals[0].P1);

                while (funnelLeftIndex < portals.Count && funnelRightIndex < portals.Count)
                {
                    var leftPortal = portals[funnelLeftIndex];
                    var rightPortal = portals[funnelRightIndex];

                    var insideFunnel = false;

                    var leftIsRightOfLeft = funnelLeft.Direction(leftPortal.P2) <= 0;
                    var rightIsLeftOfRight = funnelRight.Direction(rightPortal.P1) >= 0;

                    var leftIsLeftOfRight = funnelRight.Direction(leftPortal.P2) >= 0;
                    var rightIsRightOfLeft = funnelLeft.Direction(rightPortal.P1) <= 0;

                    if (leftIsRightOfLeft && rightIsLeftOfRight && leftIsLeftOfRight && rightIsRightOfLeft)
                    {
                        insideFunnel = true;
                    }

                    if (!insideFunnel)
                    {
                        funnelLeftIndex += 1;
                        funnelRightIndex += 1;
                    }
                    else
                    {
                        funnelLeftIndex += 1;
                        funnelRightIndex += 1;

                        funnelLeft = new LineD(currentNode, leftPortal.P2);
                        funnelRight = new LineD(currentNode, rightPortal.P1);
                    }

                }

                newPath.Add(new Point(path.Last().center.X - xOffset, path.Last().center.Y - yOffset));

                return newPath;
            }
            else
            {
                List<Point> newPath = new List<Point>();

                foreach (var node in path)
                {
                    newPath.Add(new Point(node.center));
                }

                return newPath;
            }
        }

        //private void CreateCentroidVertexGraph(TriangleNet.Mesh mesh)
        //{
        //    pointMapping = new Dictionary<PointStruct, uint>();
        //    graph = new Graph<Point, string>();

        //    pointMapping = new Dictionary<PointStruct, uint>();
        //    graph = new Graph<Point, string>();

        //    foreach (var tri in mesh.Triangles)
        //    {
        //        var vertices = new List<PointStruct>();

        //        var centroid = new PointStruct(tri.GetCentroid());

        //        if (!pointMapping.ContainsKey(centroid))
        //        {
        //            var id = graph.AddNode(new Point(centroid.X, centroid.Y));
        //            pointMapping.Add(centroid, id);
        //        }

        //        var cID = pointMapping[centroid];

        //        for (var i = 0; i < 3; i++)
        //        {
        //            var neighbor = tri.GetNeighbor(i);

        //            if (neighbor != null)
        //            {
        //                var neighborCentroid = new PointStruct(((TriangleNet.Topology.Triangle)neighbor).GetCentroid());

        //                if (!pointMapping.ContainsKey(neighborCentroid))
        //                {
        //                    var id = graph.AddNode(new Point(neighborCentroid.X, neighborCentroid.Y));
        //                    pointMapping.Add(neighborCentroid, id);
        //                }


        //                var nID = pointMapping[neighborCentroid];

        //                var dist = centroid.Distance(neighborCentroid);

        //                graph.Connect(cID, nID, (int)Math.Ceiling(dist), "");
        //            }

        //            for (var x = 0; x < 3; x++)
        //            {
        //                var iPoint = new PointStruct(tri.GetVertex(i));
        //                if (!pointMapping.ContainsKey(iPoint))
        //                {
        //                    var id = graph.AddNode(new Point(iPoint.X, iPoint.Y));
        //                    pointMapping.Add(iPoint, id);
        //                }
        //                var iID = pointMapping[iPoint];
        //                var cdist = iPoint.Distance(centroid);
        //                graph.Connect(cID, iID, (int)cdist, "");
        //                graph.Connect(iID, cID, (int)cdist, "");
        //                if (i != x)
        //                {
        //                    var xPoint = new PointStruct(tri.GetVertex(x));



        //                    if (!pointMapping.ContainsKey(xPoint))
        //                    {
        //                        var id = graph.AddNode(new Point(xPoint.X, xPoint.Y));
        //                        pointMapping.Add(xPoint, id);
        //                    }


        //                    var xID = pointMapping[xPoint];

        //                    var dist = iPoint.Distance(xPoint);



        //                    graph.Connect(iID, xID, (int)Math.Ceiling(dist), "");

        //                }
        //            }
        //        }
        //    }
        //}

        //private void CreateVertexGraph(TriangleNet.Mesh mesh)
        //{
        //    pointMapping = new Dictionary<PointStruct, uint>();
        //    graph = new Graph<Point, string>();


        //    foreach (var tri in mesh.Triangles)
        //    {
        //        var vertices = new List<PointStruct>();

        //        for (var i = 0; i < 3; i++)
        //        {
        //            for (var x = 0; x < 3; x++)
        //            {
        //                if (i != x)
        //                {
        //                    var iPoint = new PointStruct(tri.GetVertex(i));
        //                    var xPoint = new PointStruct(tri.GetVertex(x));

        //                    if (!pointMapping.ContainsKey(iPoint))
        //                    {
        //                        var id = graph.AddNode(new Point(iPoint.X, iPoint.Y));
        //                        pointMapping.Add(iPoint, id);
        //                    }

        //                    if (!pointMapping.ContainsKey(xPoint))
        //                    {
        //                        var id = graph.AddNode(new Point(xPoint.X, xPoint.Y));
        //                        pointMapping.Add(xPoint, id);
        //                    }

        //                    var iID = pointMapping[iPoint];
        //                    var xID = pointMapping[xPoint];

        //                    var dist = iPoint.Distance(xPoint);

        //                    graph.Connect(iID, xID, (int)Math.Ceiling(dist), "");
        //                }
        //            }
        //        }
        //    }
        //}

        //private void CreateCentroidGraph(TriangleNet.Mesh mesh)
        //{
        //    pointMapping = new Dictionary<PointStruct, uint>();
        //    graph = new Graph<Point, string>();


        //    foreach (var tri in mesh.Triangles)
        //    {
        //        var centroid = new PointStruct(tri.GetCentroid());

        //        for (var i = 0; i < 3; i++)
        //        {
        //            var neighbor = tri.GetNeighbor(i);

        //            if (neighbor != null)
        //            {
        //                var neighborCentroid = new PointStruct(((TriangleNet.Topology.Triangle)neighbor).GetCentroid());

        //                if (!pointMapping.ContainsKey(centroid))
        //                {
        //                    var id = graph.AddNode(new Point(centroid.X, centroid.Y));
        //                    pointMapping.Add(centroid, id);
        //                }

        //                if (!pointMapping.ContainsKey(neighborCentroid))
        //                {
        //                    var id = graph.AddNode(new Point(neighborCentroid.X, neighborCentroid.Y));
        //                    pointMapping.Add(neighborCentroid, id);
        //                }

        //                var iID = pointMapping[centroid];
        //                var xID = pointMapping[neighborCentroid];

        //                var dist = centroid.Distance(neighborCentroid);

        //                graph.Connect(iID, xID, (int)Math.Ceiling(dist), "");
        //            }
        //        }
        //    }
        //}

        private uint? GetNearestNode(PointStruct point)
        {
            var sortedKeys = pointMapping.Keys.OrderBy(e => e.Distance(point));

            foreach (var key in sortedKeys)
            {

                if (!pointMap.IsOffsetInsideMap(new Point(point.X, point.Y)))
                {
                    return pointMapping[key];
                }
                else
                {
                    //var crossesEdge = LineCrossesEdge(new Line(new Point(point.X, point.Y), new Point(key.X, key.Y)));
                    var isInterior = pointMap.IsOffsetInterior(new Line(new Point(point.X, point.Y), new Point(key.X, key.Y)));
                    if (isInterior)
                    {
                        return pointMapping[key];
                    }
                }
            }

            return null;
        }

        private PointStruct GetClosestPointOnEdge(Point point)
        {
            var offsetPoint = new Point(point.X + xOffset, point.Y + yOffset);

            var closestPoints = new List<Point>();
            foreach (var edge in interiorEdges)
            {
                var closest = edge.FindClosestPoint(offsetPoint);
                closestPoints.Add(closest);
            }

            var orderedPoints = closestPoints.OrderBy(p => p.Distance(offsetPoint));

            var nearestPoint = orderedPoints.First();

            return new PointStruct(nearestPoint.X - xOffset, nearestPoint.Y - yOffset);

        }

        private bool LineCrossesEdge(Line line)
        {
            var crosses = false;

            foreach (var edge in exteriorEdges)
            {
                var intersects = line.doIntersect(edge);

                if (intersects)
                {
                    return true;
                }
            }

            return crosses;
        }

        public GraphNode[] GetPathDebug(Point from, Point to)
        {
            var toStruct = new PointStruct(to);
            if (!pointMap.IsInsideMap(to))
            {
                toStruct = GetClosestPointOnEdge(to);
            }

            var fromStruct = new PointStruct(from);
            if (!pointMap.IsInsideMap(from))
            {
                fromStruct = GetClosestPointOnEdge(from);
            }

            var nodeFrom = GetNearestNode(new PointStruct(fromStruct.X + xOffset, fromStruct.Y + yOffset));

            var nodeTo = GetNearestNode(new PointStruct(toStruct.X + xOffset, toStruct.Y + yOffset));

            if (nodeFrom != null && nodeTo != null)
            {
                var shortestPath = triGraph.Dijkstra(nodeFrom.Value, nodeTo.Value);

                var path = shortestPath.GetPath();

                List<GraphNode> pathNodes = new List<GraphNode>();
                //pathNodes.Add(triGraph[nodeFrom.Value].Item);
                foreach (uint node in path)
                {
                    var tri = triGraph[node].Item;
                    var point = tri.center;

                    //pathPoints.Add(new Point(point.X - xOffset, point.Y - yOffset));

                    pathNodes.Add(tri);
                }
                //pathNodes.Add(triGraph[nodeTo.Value].Item);

                var test = TunnelSmooth(pathNodes);

                return pathNodes.ToArray();
            }
            else
            {
                return new GraphNode[0];
            }
        }

        public Point[] GetPath(Point from, Point to)
        {

            var toStruct = new PointStruct(to);
            if (!pointMap.IsInsideMap(to))
            {
                toStruct = GetClosestPointOnEdge(to);
            }

            var fromStruct = new PointStruct(from);
            if (!pointMap.IsInsideMap(from))
            {
                fromStruct = GetClosestPointOnEdge(from);
            }

            var nodeFrom = GetNearestNode(new PointStruct(fromStruct.X + xOffset, fromStruct.Y + yOffset));

            var nodeTo = GetNearestNode(new PointStruct(toStruct.X + xOffset, toStruct.Y + yOffset));

            if (nodeFrom != null && nodeTo != null)
            {
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

                timer.Start();
                var shortestPath = triGraph.Dijkstra(nodeFrom.Value, nodeTo.Value);
                timer.Stop();

                var path = shortestPath.GetPath();
                List<Point> pathPoints = new List<Point>();
                if (pointMap.IsOffsetInsideMap(new Point(fromStruct.X + xOffset, fromStruct.Y + yOffset)))
                {
                    pathPoints.Add(new Point(fromStruct));
                }

                List<GraphNode> pathNodes = new List<GraphNode>();

                foreach (uint node in path)
                {
                    var tri = triGraph[node].Item;
                    var point = tri.center;

                    //pathPoints.Add(new Point(point.X - xOffset, point.Y - yOffset));

                    pathNodes.Add(tri);
                }
                pathNodes.Add(triGraph[nodeTo.Value].Item);

                var test = TunnelSmooth(pathNodes);

                pathPoints.AddRange(test);

                if (pointMap.IsOffsetInsideMap(new Point(toStruct.X + xOffset, toStruct.Y + yOffset)))
                {
                    pathPoints.Add(new Point(toStruct));
                }

                return pathPoints.ToArray();
            }
            else
            {
                return new Point[0];
            }
        }
    }
}
