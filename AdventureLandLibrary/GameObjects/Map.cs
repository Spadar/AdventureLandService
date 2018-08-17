using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdventureLandLibrary.Geometry;
using AdventureLandLibrary.Global;
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

        private PointMap _originalPointMap;
        public PointMap PointMap;

        public Point[] Vertices;

        public Line[] Lines;

        public Polygon[] Polygons;

        public Map(string MapID)
        {
            this.MapID = MapID;
            var mapData = (JObject)(Loader.data.geometry)[MapID];

            if(mapData != null)
            {
                MinX = (int)((dynamic)mapData["min_x"]);
                MinY = (int)((dynamic)mapData["min_y"]);

                Width = Math.Abs((int)((dynamic)mapData["max_x"])) + Math.Abs((int)((dynamic)mapData["min_x"]));
                Height = Math.Abs((int)((dynamic)mapData["max_y"])) + Math.Abs((int)((dynamic)mapData["min_y"]));

                int xOffset = Math.Abs((int)((dynamic)mapData["min_x"]));
                int yOffset = Math.Abs((int)((dynamic)mapData["min_y"]));

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

                Lines = lines.ToArray();

                PointMap = new PointMap(Width, Height, xOffset, yOffset);

                foreach (var line in Lines)
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

                //PointMap.ErodeMap();

                //PointMap.RefillInterior(spawnPoints);

                //var poly = PointMap.BuildPolygon();

                SaveBitmap();
            }
            else
            {
                throw new Exception("Map Doesn't Exist.");
            }
        }

        public void SaveBitmap()
        {
            var mapDirectory = new DirectoryInfo(Loader.GetCurrentVersionDirectory() + @"\maps\");

            if (!mapDirectory.Exists)
            {
                mapDirectory.Create();
            }

            var filename = mapDirectory.FullName + MapID + ".png";
            var test = PointMap.ToBitmap();

            test.Save(filename);
        }
    }
}
