using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public List<MapConnection> Connections;

        public TriangleNet.Mesh Mesh;

        public Polygon poly;

        public Line[] edges;

        private MapGraph graph;

        public Map(string MapID)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            

            Console.WriteLine("Initializing Map: {0}", MapID);
            this.MapID = MapID;

            var mapData = (JObject)(Loader.data.geometry)[MapID];
            var mapDef = (JObject)(Loader.data.maps)[MapID];

            if (mapData != null)
            {
                var MinX = (int)(((dynamic)mapData["min_x"]) ?? -1000);
                var MinY = (int)(((dynamic)mapData["min_y"]) ?? -1000);

                var MaxX = (int)(((dynamic)mapData["max_x"]) ?? 1000);
                var MaxY = (int)(((dynamic)mapData["max_y"]) ?? 1000);

                if((dynamic)mapDef["no_bounds"] == true)
                {
                    MinX = -1000;
                    MaxX = 1000;

                    MinY = -1000;
                    MaxY = 1000;
                }

                Width = Math.Abs(MaxX) + Math.Abs(MinX);
                Height = Math.Abs(MaxY) + Math.Abs(MinY);

                OffsetX = Math.Abs(MinX);
                OffsetY = Math.Abs(MinY);


                List<Line> lines = new List<Line>();

                if (mapData.Properties().Where(p => p.Name == "x_lines").FirstOrDefault() != null)
                {
                    foreach (var xLine in mapData["x_lines"])
                    {
                        var line = new Line(new Point((int)xLine[0], (int)xLine[1]), new Point((int)xLine[0], (int)xLine[2]));

                        if (line.Points.Length > 1)
                        {
                            lines.Add(line);
                        }
                    }
                }

                if (mapData.Properties().Where(p => p.Name == "y_lines").FirstOrDefault() != null)
                {
                    foreach (var yLine in mapData["y_lines"])
                    {
                        var line = new Line(new Point((int)yLine[1], (int)yLine[0]), new Point((int)yLine[2], (int)yLine[0]));
                        if (line.Points.Length > 1)
                        {
                            lines.Add(line);
                        }
                    }
                }

                PointMap = new PointMap(Width, Height, OffsetX, OffsetY);

                int xBufferMin = 9;
                int xBufferMax = 9;
                int yBufferMin = 4;
                int yBufferMax = 8;

                foreach (var line in lines)
                {
                    PointMap.DrawWall(line, xBufferMin, xBufferMax, yBufferMin, yBufferMax);
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

                var p1 = new Point(MinX, MinY);
                var p2 = new Point(MinX, MaxY);
                var p3 = new Point(MaxX, MaxY);
                var p4 = new Point(MaxX, MinY);

                PointMap.DrawWall(new Line(p1, p2), xBufferMin, xBufferMax, yBufferMin, yBufferMax);
                PointMap.DrawWall(new Line(p2, p3), xBufferMin, xBufferMax, yBufferMin, yBufferMax);
                PointMap.DrawWall(new Line(p3, p4), xBufferMin, xBufferMax, yBufferMin, yBufferMax);
                PointMap.DrawWall(new Line(p4, p1), xBufferMin, xBufferMax, yBufferMin, yBufferMax);

                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
               
                poly = PointMap.BuildPolygon();
                 
                Mesh = BuildMesh();

                this.edges = PointMap.GetEdges();
                
                PointMap.FillMeshEdges(Mesh);

                graph = new MapGraph(Mesh, PointMap, OffsetX, OffsetY);

                GetMapConnections();

                stopwatch.Stop();

                Console.WriteLine("Initialized Map: {0} in {1}ms", MapID, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                throw new Exception("Map Doesn't Exist.");
            }
        }

        public void GetMapConnections()
        {
            Connections = new List<MapConnection>();
            var mapData = (JObject)(Loader.data.maps)[MapID];

            foreach(var door in mapData["doors"])
            {
                var values = door.Values().ToArray();
                var connection = new MapConnection();
                connection.isRoot = false;
                connection.MapName = MapID;
                connection.SpawnID = (int)values[6].ToObject(typeof(int));
                connection.ConnectedSpawnID = (int)values[5].ToObject(typeof(int));

                //if(connection.ConnectedSpawnID == 4 && connection.MapName == "level3")
                //{
                //    connection.ConnectedSpawnID = 2;
                //}

                connection.ConnectedMap = (string)values[4].ToObject(typeof(string));
                var spawn = mapData["spawns"][connection.SpawnID];
                connection.SpawnPoint = new PointStruct((int)spawn[0].ToObject(typeof(int)), (int)spawn[1].ToObject(typeof(int)));

                var connectedMapData = (JObject)(Loader.data.maps)[connection.ConnectedMap];
                var connectedSpawn = connectedMapData["spawns"][connection.ConnectedSpawnID];
                connection.ConnectedSpawnPoint = new PointStruct((int)connectedSpawn[0].ToObject(typeof(int)), (int)connectedSpawn[1].ToObject(typeof(int)));

                Connections.Add(connection);
            }

            var transportPlaces = (JObject)Loader.data.npcs.transporter.places;

            var transporterMaps = transportPlaces.Properties().Select(p => p.Name);

            if(transporterMaps.Contains(MapID))
            {
                var transportSpawn = (int)transportPlaces[MapID].ToObject(typeof(int));

                foreach(var otherMap in transporterMaps)
                {
                    if(otherMap != MapID)
                    {
                        var otherSpawn = (int)transportPlaces[otherMap].ToObject(typeof(int));

                        var connection = new MapConnection();

                        connection.isRoot = false;
                        connection.MapName = MapID;
                        connection.SpawnID = transportSpawn;
                        connection.ConnectedMap = otherMap;
                        connection.ConnectedSpawnID = otherSpawn;

                        var spawn = mapData["spawns"][connection.SpawnID];
                        connection.SpawnPoint = new PointStruct((int)spawn[0].ToObject(typeof(int)), (int)spawn[1].ToObject(typeof(int)));

                        var connectedMapData = (JObject)(Loader.data.maps)[connection.ConnectedMap];
                        var connectedSpawn = connectedMapData["spawns"][connection.ConnectedSpawnID];
                        connection.ConnectedSpawnPoint = new PointStruct((int)connectedSpawn[0].ToObject(typeof(int)), (int)connectedSpawn[1].ToObject(typeof(int)));

                        Connections.Add(connection);
                    }
                }
            }
        }

        public TriangleNet.Mesh BuildMesh()
        {
            var polygon = poly;

            var constraintOptions = new TriangleNet.Meshing.ConstraintOptions();
            constraintOptions.ConformingDelaunay = true;


            var qualityOptions = new TriangleNet.Meshing.QualityOptions();
            qualityOptions.MinimumAngle = 0;
            qualityOptions.MaximumAngle = 180;
            qualityOptions.MaximumArea = 250000;

            var mesh = (TriangleNet.Mesh)TriangleNet.Geometry.ExtensionMethods.Triangulate(polygon.polygon, constraintOptions, qualityOptions);
            return mesh;

        }

        public Point[] FindPath(Point start, Point end)
        {
            return graph.GetPath(start, end);
        }

        public GraphNode[] FindPathDebug(Point start, Point end)
        {
            return graph.GetPathDebug(start, end);
        }

        public double PathDistance(Point[] path)
        {
            double dist = 0;

            for(var i = 0; i < path.Length - 1; i++)
            {
                var p1 = path[i];
                var p2 = path[i + 1];

                dist += p1.Distance(p2);
            }

            return dist;
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
