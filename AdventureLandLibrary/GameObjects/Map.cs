using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        private TriangleNet.Mesh Mesh;

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
                    PointMap.DrawWall(line, 9, 6);
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

                    PointMap.DrawWall(new Line(p1, p2), 9, 6);
                    PointMap.DrawWall(new Line(p2, p3), 9, 6);
                    PointMap.DrawWall(new Line(p3, p4), 9, 6);
                    PointMap.DrawWall(new Line(p4, p1), 9, 6);
                //}

                Mesh = PointMap.BuildMesh();

                PointMap.FillMeshEdges(Mesh);


                graph = new MapGraph(Mesh, OffsetX, OffsetY);

                System.Diagnostics.Stopwatch test = new System.Diagnostics.Stopwatch();
                test.Start();

                //TestMain
                //var pathPoints = graph.GetPath(new Point(0, 0), new Point(785, -637));
                //Test Level1
                //var pathPoints = graph.GetPath(new Point(0, 9), new Point(-863, 89));

                var pathPoints = graph.GetPath(new Point(1536, -161), new Point(1032, -1802));

                var smooth = SmoothPath(pathPoints);

                test.Stop();
                SaveBitmap(Mesh);
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
