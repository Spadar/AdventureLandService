using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdventureLandLibrary.Enumerations;
using AdventureLandLibrary.ImageManipulation;

namespace AdventureLandLibrary.Geometry
{
    public class PointMap
    {
        PointType[,] points;

        public int Width
        {
            get
            {
                return points.GetLength(0);
            }
        }

        public int Height
        {
            get
            {
                return points.GetLength(1);
            }
        }

        //The offsets required to go from game coordinates to pointmap coordinates
        int xOffset;
        int yOffset;

        public PointMap(int width, int height, int xOffset, int yOffset)
        {
            points = new PointType[width + 1, height + 1];

            this.xOffset = xOffset;
            this.yOffset = yOffset;
        }

        public void RefillInterior(List<Point> interiorPoints)
        {
            int width = points.GetLength(0);
            int height = points.GetLength(1);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if(points[x,y] == PointType.Interior)
                    {
                        points[x, y] = PointType.Undefined;
                    }
                }
            }

            FillInterior(interiorPoints);

            FillExterior();
        }

        public void ErodeMap()
        {
            var subPolygons = BuildSubPolygons();

            foreach(var subPoly in subPolygons)
            {
                foreach(var line in subPoly.contour.GetSegments())
                {
                    var p1 = new Point(line.GetVertex(0));
                    var p2 = new Point(line.GetVertex(1));

                    var rect = new Rect(new Line(p1, p2), 9, 6);

                    foreach(var point in rect.Points)
                    {
                        if(isOffsetPointWithinBounds(point))
                        {
                            if(points[point.X, point.Y] == PointType.Interior)
                            {
                                points[point.X, point.Y] = PointType.Eroded;
                            }
                        }
                    }
                }
            }
        }

        //public PolygonPart[] BuildErodedSubPolygons()
        //{
        //    var vertices = IdentifyErodedVertices();

        //    List<PolygonPart> parts = new List<PolygonPart>();

        //    List<Point> availableVertices = new List<Point>();
        //    availableVertices.AddRange(vertices);

        //    bool[,] visitedPoints = new bool[points.GetLength(0), points.GetLength(1)];

        //    while (availableVertices.Count > 0)
        //    {
        //        var currentShape = new List<Point>();
        //        var currentPoint = FindTopLeftMostPoint(availableVertices);
        //        currentShape.Add(currentPoint);
        //        availableVertices.Remove(currentPoint);

        //        while (currentPoint != null)
        //        {
        //            var newVertex = FindNextVertex(currentPoint, visitedPoints, availableVertices, PointType.Eroded);

        //            if (newVertex != null)
        //            {
        //                currentShape.Add(newVertex);
        //                availableVertices.Remove(newVertex);
        //            }
        //            currentPoint = newVertex;
        //        }

        //        if (currentShape.Count > 2)
        //        {
        //            var curPart = new PolygonPart(currentShape.ToArray());

        //            parts.Add(curPart);
        //        }
        //    }

        //    for (var i = 0; i < parts.Count; i++)
        //    {
        //        var polyPart = parts[i];

        //        var insideCount = 0;
        //        for (var ix = 0; ix < parts.Count; ix++)
        //        {
        //            if (ix != i)
        //            {
        //                var polyPartCompare = parts[ix];

        //                //No need to check every vertex because of how we generated these vertices
        //                if (TriangleNet.Geometry.Contour.IsPointInPolygon(new TriangleNet.Geometry.Point(polyPart.Vertices[0].X, polyPart.Vertices[0].Y), polyPartCompare.contour.Points))
        //                {
        //                    insideCount++;
        //                    break;
        //                }
        //            }
        //        }

        //        //0 would be outer polygons. >0 would be innner... Nested polygons would have counts >1
        //        if (insideCount > 0)
        //        {
        //            polyPart.IsHole = true;
        //        }

        //        polyPart.Refresh();
        //    }

        //    foreach (var part in parts)
        //    {
        //        if (part.Vertices.Length < 4)
        //        {
        //            var test = true;
        //        }
        //    }

        //    return parts.ToArray();
        //}

        //public Polygon BuildErodedPolygon()
        //{
        //    var polygon = new Polygon(BuildErodedSubPolygons());

        //    return polygon;
        //}

        //public TriangleNet.Mesh BuildErodedMesh()
        //{
        //    var polygon = BuildErodedPolygon();

        //    var constraintOptions = new TriangleNet.Meshing.ConstraintOptions();

        //    var qualityOptions = new TriangleNet.Meshing.QualityOptions();
        //    qualityOptions.MinimumAngle = 20;
        //    qualityOptions.MaximumAngle = 180;


        //    return (TriangleNet.Mesh)TriangleNet.Geometry.ExtensionMethods.Triangulate(polygon.polygon, constraintOptions, qualityOptions);

        //}

        public PolygonPart[] BuildSubPolygons()
        {
            var vertices = IdentifyVertices();

            List<PolygonPart> parts = new List<PolygonPart>();

            List<Point> availableVertices = new List<Point>();
            availableVertices.AddRange(vertices);

            bool[,] visitedPoints = new bool[points.GetLength(0), points.GetLength(1)];

            while (availableVertices.Count > 0)
            {
                var currentShape = new List<Point>();
                var currentPoint = FindTopLeftMostPoint(availableVertices);
                currentShape.Add(currentPoint);
                availableVertices.Remove(currentPoint);

                while (currentPoint != null)
                {
                    var newVertex = FindNextVertex(currentPoint, visitedPoints, availableVertices, PointType.Eroded);

                    if (newVertex != null)
                    {
                        currentShape.Add(newVertex);
                        availableVertices.Remove(newVertex);
                    }
                    currentPoint = newVertex;
                }

                if (currentShape.Count > 2)
                {
                    var curPart = new PolygonPart(currentShape.ToArray());

                    parts.Add(curPart);
                }
            }

            for (var i = 0; i < parts.Count; i++)
            {
                var polyPart = parts[i];

                var insideCount = 0;
                for (var ix = 0; ix < parts.Count; ix++)
                {
                    if (ix != i)
                    {
                        var polyPartCompare = parts[ix];

                        //No need to check every vertex because of how we generated these vertices
                        if (TriangleNet.Geometry.Contour.IsPointInPolygon(new TriangleNet.Geometry.Point(polyPart.Vertices[0].X, polyPart.Vertices[0].Y), polyPartCompare.contour.Points))
                        {
                            insideCount++;
                            break;
                        }
                    }
                }

                var interiorPoint = polyPart.contour.FindInteriorPoint();

                //0 would be outer polygons. >0 would be innner... Nested polygons would have counts >1
                if (insideCount > 0 && points[(int)interiorPoint.X, (int)interiorPoint.Y] == PointType.Interior)
                {
                    polyPart.IsHole = true;
                }

                polyPart.Refresh();
            }

            foreach(var part in parts)
            {
                if(part.Vertices.Length < 4)
                {
                    var test = true;
                }
            }

            return parts.ToArray();
        }

        public Polygon BuildPolygon()
        {
            var polygon = new Polygon(BuildSubPolygons());

            return polygon;
        }
        
        public TriangleNet.Mesh BuildMesh()
        {
            var polygon = BuildPolygon();

            var constraintOptions = new TriangleNet.Meshing.ConstraintOptions();

            var qualityOptions = new TriangleNet.Meshing.QualityOptions();
            qualityOptions.MinimumAngle = 20;
            qualityOptions.MaximumAngle = 180;


            return (TriangleNet.Mesh)TriangleNet.Geometry.ExtensionMethods.Triangulate(polygon.polygon, constraintOptions, qualityOptions);

        }

        public static Point FindTopLeftMostPoint(List<Point> vertices)
        {
            Point topLeft = null;

            foreach (var vertex in vertices)
            {
                if (topLeft == null || vertex.X < topLeft.X && vertex.Y < topLeft.Y)
                {
                    topLeft = vertex;
                }
            }

            return topLeft;
        }

        public Point FindNextVertex(Point curVertex, bool[,] visited, List<Point> vertices, PointType vertexType)
        {
            var neighbors = FindPointNeighbors(curVertex);

            var foundDirection = false;
            var dirX = 0;
            var dirY = 0;


            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (neighbors[x, y] == vertexType)
                    {
                        if (Math.Abs(x - 1) + Math.Abs(y - 1) == 1)
                        {
                            var neighborPoint = new Point(curVertex.X + (x - 1), curVertex.Y + (y - 1));

                            if (isOffsetPointWithinBounds(neighborPoint))
                            {
                                if (!visited[neighborPoint.X, neighborPoint.Y])
                                {
                                    var neighborNeighbors = FindPointNeighbors(neighborPoint);

                                    var wallCount = 0;

                                    for (int nnx = 0; nnx < 3; nnx++)
                                    {
                                        for (int nny = 0; nny < 3; nny++)
                                        {
                                            if (neighborNeighbors[nnx, nny] != vertexType)
                                            {
                                                wallCount++;
                                                break;
                                            }
                                        }

                                        if (wallCount > 0)
                                        {
                                            break;
                                        }
                                    }

                                    if (wallCount > 0)
                                    {
                                        dirX = curVertex.X - neighborPoint.X;
                                        dirY = curVertex.Y - neighborPoint.Y;
                                        foundDirection = true;
                                        visited[neighborPoint.X, neighborPoint.Y] = true;
                                        break;
                                    }

                                }
                            }
                        }
                    }
                }

                if (foundDirection)
                {
                    break;
                }
            }

            var angle = FindAngleBetweenPoints(curVertex, new Point(curVertex.X - dirX, curVertex.Y - dirY));

            var next = FindClosestVertexOnAngle(vertices, angle, curVertex);

            if (next != null)
            {
                var minX = curVertex.X;
                var maxX = next.X;
                var minY = curVertex.Y;
                var maxY = next.Y;

                if (next.X < curVertex.X)
                {
                    minX = next.X;
                    maxY = curVertex.Y;
                }

                if (next.Y < curVertex.Y)
                {
                    minY = next.Y;
                    maxY = curVertex.Y;
                }

                for (var x = minX; x <= maxX; x++)
                {
                    for (var y = minY; y <= maxY; y++)
                    {
                        visited[x, y] = true;
                    }
                }
            }

            return next;
        }

        public Point FindClosestVertexOnAngle(List<Point> vertices, double angle, Point vertex)
        {
            Point closestVertex = null;

            double closestDistance = 0;

            foreach (var vert in vertices)
            {
                var distance = FindDistance(vertex, vert);
                if (distance > 0)
                {
                    var vertAngle = FindAngleBetweenPoints(vertex, vert);

                    if (vertAngle == angle)
                    {
                        if (closestVertex == null || distance < closestDistance)
                        {
                            closestVertex = vert;
                            closestDistance = distance;
                        }
                    }
                }
            }

            return closestVertex;
        }

        private double FindDistance(Point v1, Point v2)
        {
            return Math.Sqrt(Math.Pow((v2.X - v1.X), 2) + Math.Pow((v2.Y - v1.Y), 2));
        }

        private double FindAngleBetweenPoints(Point v1, Point v2)
        {
            float xDiff = v2.X - v1.X;
            float yDiff = v2.Y - v1.Y;
            return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
        }

        public Point[] IdentifyVertices()
        {
            List<Point> vertices = new List<Point>();

            int width = points.GetLength(0);
            int height = points.GetLength(1);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var pointType = points[x, y];
                    if (pointType == PointType.Wall || pointType == PointType.Eroded)
                    {
                        var neighbors = FindPointNeighbors(new Point(x, y));

                        int numWalls = 0;
                        bool isCorner = false;
                        Point cornerDir = new Point(0, 0);

                        for (int nx = 0; nx < 3; nx++)
                        {
                            for (int ny = 0; ny < 3; ny++)
                            {
                                if (neighbors[nx, ny] == PointType.Interior)
                                {
                                    numWalls++;

                                    if (Math.Abs(nx - 1) + Math.Abs(ny - 1) == 2)
                                    {
                                        cornerDir = new Point(cornerDir.X + (nx - 1), cornerDir.Y + (ny - 1));
                                        isCorner = true;
                                    }
                                }
                            }
                        }

                        if (numWalls == 1 && isCorner)
                        {
                            var point = new Point(x, y);
                            vertices.Add(point);
                        }
                        else if (numWalls > 3)
                        {
                            var point = new Point(x, y);
                            vertices.Add(point);
                        }
                    }
                }
            }

            return vertices.ToArray();
        }

        //public Point[] IdentifyErodedVertices()
        //{
        //    List<Point> vertices = new List<Point>();

        //    int width = points.GetLength(0);
        //    int height = points.GetLength(1);

        //    for (var x = 0; x < width; x++)
        //    {
        //        for (var y = 0; y < height; y++)
        //        {
        //            if (points[x, y] == PointType.Eroded)
        //            {
        //                var neighbors = FindPointNeighbors(new Point(x, y));

        //                int numWalls = 0;
        //                bool isCorner = false;
        //                Point cornerDir = new Point(0, 0);

        //                for (int nx = 0; nx < 3; nx++)
        //                {
        //                    for (int ny = 0; ny < 3; ny++)
        //                    {
        //                        if (neighbors[nx, ny] == PointType.Interior)
        //                        {
        //                            numWalls++;

        //                            if (Math.Abs(nx - 1) + Math.Abs(ny - 1) == 2)
        //                            {
        //                                cornerDir = new Point(cornerDir.X + (nx - 1), cornerDir.Y + (ny - 1));
        //                                isCorner = true;
        //                            }
        //                        }
        //                    }
        //                }

        //                if (numWalls == 1 && isCorner)
        //                {
        //                    var point = new Point(x, y);
        //                    vertices.Add(point);
        //                }
        //                else if (numWalls > 3)
        //                {
        //                    var point = new Point(x, y);
        //                    vertices.Add(point);
        //                }
        //            }
        //        }
        //    }

        //    return vertices.ToArray();
        //}

        public void FillExterior()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (points[x, y] == PointType.Undefined)
                    {
                        points[x, y] = PointType.Exterior;
                    }
                }
            }
        }

        /// <summary>
        /// Fills the interior of the pointmap using a list of example points (Spawns for example)
        /// </summary>
        /// <param name="interiorPoints"></param>
        public void FillInterior(List<Point> interiorPoints)
        {
            foreach (var point in interiorPoints)
            {
                var offsetPoint = new Point(point.X + xOffset, point.Y + yOffset);

                FloodFillFromOffsetPoint(offsetPoint);
            }
        }

        private void FloodFillFromOffsetPoint(Point point)
        {
            Stack<Point> pointsToVisit = new Stack<Point>();

            pointsToVisit.Push(point);
            if (points[point.X, point.Y] == PointType.Undefined)
            {
                while (pointsToVisit.Count > 0)
                {
                    var curPoint = pointsToVisit.Pop();

                    points[curPoint.X, curPoint.Y] = PointType.Interior;

                    for (int nx = -1; nx <= 1; nx++)
                    {
                        for (int ny = -1; ny <= 1; ny++)
                        {
                            if (Math.Abs(nx) + Math.Abs(ny) == 1)
                            {
                                var neighbor = new Point(curPoint.X + nx, curPoint.Y + ny);

                                if (isOffsetPointWithinBounds(neighbor))
                                {
                                    if (points[neighbor.X, neighbor.Y] == PointType.Undefined)
                                    {
                                        if (points[neighbor.X, neighbor.Y] != PointType.Wall)
                                        {
                                            points[neighbor.X, neighbor.Y] = PointType.Interior;
                                            pointsToVisit.Push(neighbor);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DrawWall(Line line, int xBuffer, int yBuffer)
        {
            foreach (var point in line.Points)
            {
                var offsetPoint = new Point(point.X + xOffset, point.Y + yOffset);
                if (isOffsetPointWithinBounds(offsetPoint))
                {
                    points[offsetPoint.X, offsetPoint.Y] = PointType.Wall;
                }
            }
            var rect = new Rect(line, xBuffer, yBuffer);
            foreach (var point in rect.Points)
            {
                var offsetPoint = new Point(point.X + xOffset, point.Y + yOffset);
                if (isOffsetPointWithinBounds(offsetPoint))
                {
                    if (points[offsetPoint.X, offsetPoint.Y] == PointType.Undefined)
                    {
                        points[offsetPoint.X, offsetPoint.Y] = PointType.Eroded;
                    }
                }
            }

            
        }

        /// <summary>
        /// Is a point that has already been offset to match the array coordinate system within the bounds of the array?
        /// </summary>
        /// <param name="offsetPoint"></param>
        /// <returns></returns>
        private bool isOffsetPointWithinBounds(Point offsetPoint)
        {
            if (offsetPoint.X >= 0 && offsetPoint.X < Width && offsetPoint.Y >= 0 && offsetPoint.Y < Height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Is a point which is in the world coordinate system within the bounds of the array?
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool isPointWithinBounds(Point point)
        {
            var offsetPoint = new Point(xOffset + point.X, yOffset + point.Y);

            return isOffsetPointWithinBounds(offsetPoint);
        }

        public bool IsInsideMap(Point point)
        {
            var offsetPoint = new Point(xOffset + point.X, yOffset + point.Y);

            if (isOffsetPointWithinBounds(offsetPoint))
            {
                if (points[point.X, point.Y] == PointType.Interior)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private PointType[,] FindPointNeighbors(Point point)
        {
            PointType[,] neighbors = new PointType[3, 3];

            for (int nx = -1; nx <= 1; nx++)
            {
                for (int ny = -1; ny <= 1; ny++)
                {
                    var neighborPoint = new Point(point.X + nx, point.Y + ny);
                    PointType neighborType = PointType.Undefined;

                    if (isOffsetPointWithinBounds(neighborPoint))
                    {
                        neighborType = points[neighborPoint.X, neighborPoint.Y];
                    }

                    neighbors[nx + 1, ny + 1] = neighborType;
                }
            }

            return neighbors;
        }

        public System.Drawing.Bitmap ToBitmap()
        {
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(Width, Height);

            var lockbitmap = new LockBitmap(bitmap);

            lockbitmap.LockBits();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var pointType = points[x, y];

                    switch (pointType)
                    {
                        case PointType.Eroded:
                            lockbitmap.SetPixel(x, y, System.Drawing.Color.Green);
                            break;
                        case PointType.Exterior:
                            lockbitmap.SetPixel(x, y, System.Drawing.Color.Black);
                            break;
                        case PointType.Wall:
                            lockbitmap.SetPixel(x, y, System.Drawing.Color.Black);
                            break;
                        case PointType.Interior:
                            lockbitmap.SetPixel(x, y, System.Drawing.Color.White);
                            break;
                        case PointType.Undefined:
                            lockbitmap.SetPixel(x, y, System.Drawing.Color.Yellow);
                            break;
                    }
                }
            }

            var interiorVertices = IdentifyVertices();

            foreach (var vertex in interiorVertices)
            {
                lockbitmap.SetPixel(vertex.X, vertex.Y, System.Drawing.Color.Red);
            }

            var mesh = BuildMesh();

            var meshLines = new List<Line>();

            foreach (var tri in mesh.Triangles)
            {
                var v1 = new Point(tri.GetVertex(0));
                var v2 = new Point(tri.GetVertex(1));
                var v3 = new Point(tri.GetVertex(2));

                var edge1 = new Line(v1, v2);
                var edge2 = new Line(v2, v3);
                var edge3 = new Line(v3, v1);

                meshLines.Add(edge1);
                meshLines.Add(edge2);
                meshLines.Add(edge3);
            }

            foreach (var line in meshLines)
            {
                foreach (var point in line.Points)
                {
                    lockbitmap.SetPixel(point.X, point.Y, System.Drawing.Color.Red);
                }
            }

            lockbitmap.UnlockBits();

            return bitmap;
        }
    }
}
