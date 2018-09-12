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
        Dictionary<GraphNode, uint> nodeMapping;
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
        }

        private void CreateTriangleGraph(TriangleNet.Mesh mesh)
        {
            triGraph = new Graph<GraphNode, string>();
            pointMapping = new Dictionary<PointStruct, uint>();
            nodeMapping = new Dictionary<GraphNode, uint>();

            foreach (var tri in mesh.Triangles)
            {
                GraphNode node = new GraphNode(tri);


                if (!pointMapping.ContainsKey(node.center))
                {
                    var id = triGraph.AddNode(node);
                    pointMapping.Add(node.center, id);
                    nodeMapping.Add(node, id);
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
                            nodeMapping.Add(neighborNode, id);
                        }


                        var nID = pointMapping[neighborNode.center];

                        var dist = node.center.Distance(neighborNode.center);

                        var portal = node.GetPortal(neighborNode);

                        triGraph.Connect(cID, nID, (int)Math.Ceiling(dist), "");
                    }
                }
            }
        }

        public List<Point> FunnelSmooth(List<GraphNode> path, Point startPoint, Point endPoint)
        {
            if (path.Count > 2)
            {
                var offsetEndpoint = new Point(endPoint.X + xOffset, endPoint.Y + yOffset);

                List<Point> newPath = new List<Point>();

                List<LineD> portals = new List<LineD>();

                for (var i = 0; i < path.Count - 1; i++)
                {
                    portals.Add(path[i].GetPortal(path[i + 1]));
                }

                var ending = path[path.Count - 1].GetNonPortal(path[path.Count - 2], offsetEndpoint);

                portals.AddRange(ending);

                PointD currentNode = new PointD(startPoint.X + xOffset, startPoint.Y + yOffset);

                newPath.Add(new Point((int)currentNode.X - xOffset, (int)currentNode.Y - yOffset));

                int funnelLeftIndex = 1;
                int funnelRightIndex = 1;

                int curFunnelLeftIndex = 1;
                int curFunnelRightIndex = 1;

                LineD funnelLeft = new LineD(currentNode, portals[0].P2);
                LineD funnelRight = new LineD(currentNode, portals[0].P1);

                int leftSide = 0;
                int rightSide = 1;

                PointD lastPoint = new PointD(offsetEndpoint.X, offsetEndpoint.Y);

                int count = 0;
                while (funnelLeftIndex < portals.Count && funnelRightIndex < portals.Count && !(funnelLeft.P2.X == lastPoint.X && funnelLeft.P2.Y == lastPoint.Y) && !(funnelRight.P2.X == lastPoint.X && funnelRight.P2.Y == lastPoint.Y))
                {

                    var prevLeftPoint = portals[funnelLeftIndex].P2;

                    if (leftSide == 0 && funnelLeftIndex > 0)
                    {
                        prevLeftPoint = portals[funnelLeftIndex - 1].P2;
                    }

                    var prevRightPoint = portals[funnelRightIndex].P1;

                    if (rightSide == 0 && funnelRightIndex > 0)
                    {
                        prevRightPoint = portals[funnelRightIndex - 1].P1;
                    }

                    var newFunnelLeftIndex = funnelLeftIndex + leftSide;
                    var newFunnelRightIndex = funnelRightIndex + rightSide;

                    if (newFunnelLeftIndex > portals.Count - 1)
                    {
                        newFunnelLeftIndex = portals.Count - 1;
                    }

                    if (newFunnelRightIndex > portals.Count - 1)
                    {
                        newFunnelRightIndex = portals.Count - 1;
                    }

                    curFunnelLeftIndex = curFunnelLeftIndex + leftSide;
                    curFunnelRightIndex = curFunnelRightIndex + rightSide;

                    if (curFunnelLeftIndex > portals.Count - 1)
                    {
                        curFunnelLeftIndex = portals.Count - 1;
                    }

                    if (curFunnelRightIndex > portals.Count - 1)
                    {
                        curFunnelRightIndex = portals.Count - 1;
                    }

                    var actualLeftPoint = portals[newFunnelLeftIndex].P2;
                    var actualRightPoint = portals[newFunnelRightIndex].P1;

                    var leftPoint = portals[curFunnelLeftIndex].P2;

                    var rightPoint = portals[curFunnelRightIndex].P1;

                    var insideFunnel = false;

                    var dirLeftLeft = funnelLeft.Direction(leftPoint);
                    var dirRightRight = funnelRight.Direction(rightPoint);
                    var leftIsRightOfLeft = funnelLeft.Direction(leftPoint) <= 0;
                    var rightIsLeftOfRight = funnelRight.Direction(rightPoint) >= 0;

                    var dirLeftRight = funnelRight.Direction(leftPoint);
                    var dirRightLeft = funnelLeft.Direction(rightPoint);
                    var leftIsLeftOfRight = funnelRight.Direction(leftPoint) >= 0;
                    var rightIsRightOfLeft = funnelLeft.Direction(rightPoint) <= 0;

                    if (leftIsRightOfLeft && rightIsLeftOfRight && leftIsLeftOfRight && rightIsRightOfLeft)
                    {
                        insideFunnel = true;
                    }

                    if (!insideFunnel)
                    {
                        if (!leftIsLeftOfRight)
                        {
                            currentNode = new PointD(portals[funnelRightIndex].P1.X, portals[funnelRightIndex].P1.Y);

                            newPath.Add(new Point((int)currentNode.X - xOffset, (int)currentNode.Y - yOffset));

                            var minIndex = Math.Min(newFunnelLeftIndex, newFunnelRightIndex);

                            var newFRight = NextRight(minIndex, portals);
                            var newFLeft = NextLeft(minIndex, portals);

                            var maxPortal = Math.Max(newFRight, newFLeft);

                            funnelLeftIndex = maxPortal;
                            funnelRightIndex = maxPortal;
                            funnelLeft = new LineD(currentNode, portals[maxPortal].P2);
                            funnelRight = new LineD(currentNode, portals[maxPortal].P1);
                            curFunnelLeftIndex = maxPortal;
                            curFunnelRightIndex = maxPortal;
                        }
                        else if (!rightIsRightOfLeft)
                        {
                            currentNode = new PointD(portals[funnelLeftIndex].P2.X, portals[funnelLeftIndex].P2.Y);

                            newPath.Add(new Point((int)currentNode.X - xOffset, (int)currentNode.Y - yOffset));

                            var minIndex = Math.Min(newFunnelLeftIndex, newFunnelRightIndex);

                            var newFRight = NextRight(minIndex, portals);
                            var newFLeft = NextLeft(minIndex, portals);

                            var maxPortal = Math.Max(newFRight, newFLeft);

                            funnelLeftIndex = maxPortal;
                            funnelRightIndex = maxPortal;
                            funnelLeft = new LineD(currentNode, portals[maxPortal].P2);
                            funnelRight = new LineD(currentNode, portals[maxPortal].P1);
                            curFunnelLeftIndex = maxPortal;
                            curFunnelRightIndex = maxPortal;
                        }
                        else if (rightIsLeftOfRight)
                        {
                            funnelRight = new LineD(currentNode, rightPoint);
                        }
                        else if (leftIsRightOfLeft)
                        {
                            funnelLeft = new LineD(currentNode, leftPoint);
                        }
                    }
                    else
                    {
                        var newfunnelLeft = new LineD(currentNode, leftPoint);
                        var newfunnelRight = new AdventureLandLibrary.Geometry.LineD(currentNode, rightPoint);

                        funnelLeftIndex = newFunnelLeftIndex;
                        funnelRightIndex = newFunnelRightIndex;
                        funnelLeft = newfunnelLeft;
                        funnelRight = newfunnelRight;
                        curFunnelLeftIndex = funnelLeftIndex;
                        curFunnelRightIndex = funnelRightIndex;
                    }
                    leftSide++;
                    rightSide++;

                    if (leftSide > 1)
                    {
                        leftSide = 0;
                    }

                    if (rightSide > 1)
                    {
                        rightSide = 0;
                    }
                    count++;
                }

                newPath.Add(new Point(endPoint.X, endPoint.Y));
                return newPath;
            }
            else
            {
                List<Point> newPath = new List<Point>();

                newPath.Add(startPoint);
                newPath.Add(endPoint);

                return newPath;
            }
        }

        public int NextLeft(int leftIndex, List<LineD> portals)
        {
            var curPoint = portals[leftIndex].P2;

            if (leftIndex < portals.Count - 1)
            {
                int nextIndex = leftIndex + 1;
                PointD nextPoint = portals[nextIndex].P2;

                //while (curPoint.X == nextPoint.X && curPoint.Y == nextPoint.Y && nextIndex < portals.Count - 1)
                //{
                //    nextIndex++;
                //    nextPoint = portals[nextIndex].P2;
                //}

                return nextIndex;
            }
            else
            {
                return leftIndex;
            }
        }

        public int NextRight(int rightIndex, List<LineD> portals)
        {
            var curPoint = portals[rightIndex].P1;

            if (rightIndex < portals.Count - 1)
            {
                int nextIndex = rightIndex + 1;
                PointD nextPoint = portals[nextIndex].P1;

                //while (curPoint.X == nextPoint.X && curPoint.Y == nextPoint.Y && nextIndex < portals.Count - 1)
                //{
                //    nextIndex++;
                //    nextPoint = portals[nextIndex].P2;
                //}

                return nextIndex;
            }
            else
            {
                return rightIndex;
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
            
            KeyValuePair<GraphNode, uint>? insideNode = null;

            foreach (var node in nodeMapping)
            {
                if (node.Key.PointInNode(new PointD(point.X, point.Y)))
                {
                    insideNode = node;
                    break;
                }
            }

            if (insideNode == null)
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
            }
            else
            {
                return insideNode.Value.Value;
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

                //var test = TunnelSmooth(pathNodes);

                return pathNodes.ToArray();
            }
            else
            {
                return new GraphNode[0];
            }
        }

        public Point[] GetPath(Point from, Point to)
        {

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
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
            timer.Stop();
            timer.Reset();
            var nodeTo = GetNearestNode(new PointStruct(toStruct.X + xOffset, toStruct.Y + yOffset));
            
            if (nodeFrom != null && nodeTo != null)
            {
                timer.Start();
                var shortestPath = triGraph.Dijkstra(nodeFrom.Value, nodeTo.Value);

                timer.Stop();

                timer.Reset();

                timer.Start();
                var path = shortestPath.GetPath();
                timer.Stop();
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
                //pathNodes.Add(triGraph[nodeTo.Value].Item);

                var test = FunnelSmooth(pathNodes, new Point(fromStruct), new Point(toStruct));

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
