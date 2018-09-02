using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AdventureLandLibrary.Geometry;
using AdventureLandLibrary.Global;
using AdventureLandLibrary.Pathfinding;
using Newtonsoft.Json.Linq;

namespace AdventureLandLibrary.GameObjects
{
    public class Map
    {
        public string MapID;

        public int MinX;
        public int MinY;

        public int OffsetX;
        public int OffsetY;

        public int Width;
        public int Height;

        public PointMap PointMap;

        public TriangleNet.Mesh Mesh;

        public Line[] edges;

        private MapGraph graph;

        public Map(string MapID)
        {
            this.MapID = MapID;
            var mapData = (JObject)(Loader.data.geometry)[MapID];

            if(mapData != null)
            {
                var MinX = (int)((dynamic)mapData["min_x"]);
                var MinY = (int)((dynamic)mapData["min_y"]);

                var MaxX = (int)((dynamic)mapData["max_x"]);
                var MaxY = (int)((dynamic)mapData["max_y"]);

                Width = Math.Abs((int)((dynamic)mapData["max_x"])) + Math.Abs((int)((dynamic)mapData["min_x"]));
                Height = Math.Abs((int)((dynamic)mapData["max_y"])) + Math.Abs((int)((dynamic)mapData["min_y"]));

                OffsetX = Math.Abs((int)((dynamic)mapData["min_x"]));
                OffsetY = Math.Abs((int)((dynamic)mapData["min_y"]));

                List<Line> lines = new List<Line>();

                foreach (var xLine in mapData["x_lines"])
                {
                    var line = new Line(new Point((int)xLine[0], (int)xLine[1]), new Point((int)xLine[0], (int)xLine[2]));

                    if (line.Points.Length > 1)
                    {
                        lines.Add(line);
                    }
                }

                foreach (var yLine in mapData["y_lines"])
                {
                    var line = new Line(new Point((int)yLine[1], (int)yLine[0]), new Point((int)yLine[2], (int)yLine[0]));
                    if (line.Points.Length > 1)
                    {
                        lines.Add(line);
                    }
                }

                PointMap = new PointMap(Width, Height, OffsetX, OffsetY);

                foreach (var line in lines)
                {
                    PointMap.DrawWall(line, 9, 9, 4, 8);
                }

                var spawns = ((JObject)(Loader.data.maps))[MapID]["spawns"];

                List<Point> spawnPoints = new List<Point>();
                foreach (var spawn in spawns)
                {
                    var spawnPoint = new Point((int)spawn[0], (int)spawn[1]);
                    spawnPoints.Add(spawnPoint);
                }

                PointMap.FillInterior(spawnPoints);

                PointMap.FillExterior();

                //if(PointMap.IsInsideMap(new Point(MinX, MinY)))
                //{
                    //Draw map edges as walls to contain map.
                    var p1 = new Point(MinX, MinY);
                    var p2 = new Point(MinX, MaxY);
                    var p3 = new Point(MaxX, MaxY);
                    var p4 = new Point(MaxX, MinY);

                    PointMap.DrawWall(new Line(p1, p2), 9, 9, 4, 8);
                    PointMap.DrawWall(new Line(p2, p3), 9, 9, 4, 8);
                    PointMap.DrawWall(new Line(p3, p4), 9, 9, 4, 8);
                    PointMap.DrawWall(new Line(p4, p1), 9, 9, 4, 8);
                //}

                Mesh = PointMap.BuildMesh();

                this.edges = PointMap.GetEdges();

                
                PointMap.FillMeshEdges(Mesh);


                graph = new MapGraph(Mesh, PointMap, OffsetX, OffsetY);

            }
            else
            {
                throw new Exception("Map Doesn't Exist.");
            }
        }

        public Point[] FindPath(Point start, Point end)
        {
            return graph.GetPath(start, end);
        }

        public GraphNode[] FindPathDebug(Point start, Point end)
        {
            return graph.GetPathDebug(start, end);
        }

        public Point[] SmoothPath(Point[] path)
        {
            List<Point> smoothedPath = new List<Point>();

            for (int i = 0; i < path.Length; i++)
            {
                var startPoint = path[i];
                smoothedPath.Add(startPoint);

                    for (int x = i + 1; x < path.Length; x++)
                    {

                        var skippedPoint = path[x];

                        var line = new Line(startPoint, skippedPoint);

                        if (PointMap.IsInterior(line))
                        {
                            i = x - 1;
                        }
                        else
                        {
                            break;
                        }

                    }
            }

            return smoothedPath.ToArray();

        }

        public Point[] SmoothPathReverse(Point[] path)
        {
            List<Point> smoothedPath = new List<Point>();

            for (int i = 0; i < path.Length; i++)
            {
                var startPoint = path[i];
                smoothedPath.Add(startPoint);

                for (int x = path.Length - 1; x > i; x--)
                {
                    if (x > path.Length - 1)
                    {
                        x = path.Length - 1;

                        if(x <= i)
                        {
                            break;
                        }
                    }
                    var skippedPoint = path[x];

                    var line = new Line(startPoint, skippedPoint);

                    if (PointMap.IsInterior(line))
                    {
                        i = x - 1;
                        break;
                    }

                }
            }

            return smoothedPath.ToArray();

        }

        public Point[] SmoothPathDetailed(Point[] path)
        {
            List<Point> smoothedPath = new List<Point>();

            for (int i = 0; i < path.Length; i++)
            {
                var startPoint = path[i];
                smoothedPath.Add(startPoint);

                if (i < path.Length - 2)
                {
                    var prevLine = new Line(path[i], path[i + 1]);
                    var nextLine = new Line(path[i + 1], path[i + 2]);

                    var prevIndex = prevLine.Points.Length - 1;
                    var nextIndex = 0;

                    var decrementPrev = true;
                    var incrementNext = true;

                    Line curLine = null;

                    while(decrementPrev && incrementNext)
                    {
                        if(prevIndex == 0)
                        {
                            decrementPrev = false;
                        }

                        if(nextIndex == nextLine.Points.Length - 1)
                        {
                            incrementNext = false;
                        }

                        var testLine = new Line(prevLine.Points[prevIndex], nextLine.Points[nextIndex]);

                        if(PointMap.IsInterior(testLine))
                        {
                            if(curLine == null || testLine.Points.Length > curLine.Points.Length)
                            {
                                curLine = testLine;
                            }
                        }

                        if (decrementPrev)
                        {
                            prevIndex -= 1;
                        }

                        if (incrementNext)
                        {
                            nextIndex += 1;
                        }

                    }

                    if(curLine != null)
                    {
                        smoothedPath.Add(curLine.P1);
                        smoothedPath.Add(curLine.P2);

                        i = i + 2;
                    }

                }
            }

            return SmoothPath(smoothedPath.ToArray());

        }


        public System.Drawing.Bitmap GetBitmap()
        {
            return PointMap.ToBitmap(Mesh);
        }

        public void SaveBitmap(TriangleNet.Mesh mesh)
        {
            var mapDirectory = new DirectoryInfo(Loader.GetCurrentVersionDirectory() + @"\maps\");

            if (!mapDirectory.Exists)
            {
                mapDirectory.Create();
            }

            var filename = mapDirectory.FullName + MapID + ".png";
            var test = PointMap.ToBitmap(mesh);

            test.Save(filename);
        }
    }
}
