using AdventureLandLibrary.GameObjects;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdventureLandLibrary.Geometry;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SignalR_Service
{
    public class AdventurelandHub : Hub
    {
        static Dictionary<string, Map> maps = new Dictionary<string, Map>();
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }

        public void Ping()
        {
            Clients.Caller.Pong(DateTime.Now);
        }

        public void GetMesh(string mapID)
        {
            if (!maps.ContainsKey(mapID))
            {
                var newMap = new Map(mapID);
                maps.Add(mapID, newMap);
            }

            var map = maps[mapID];

            var mesh = map.Mesh;

            var meshLines = new List<Line>();

            foreach (var tri in mesh.Triangles)
            {
                //var centroid = new Point(tri.GetCentroid(), map.OffsetX, map.OffsetY);

                //for (var i = 0; i < 3; i++)
                //{
                //    var neighbor = tri.GetNeighbor(i);

                //    if (neighbor != null)
                //    {
                //        var ncentroid = new Point(((TriangleNet.Topology.Triangle)neighbor).GetCentroid(), map.OffsetX, map.OffsetY);

                //        var centroidLine = new Line(centroid, ncentroid);
                //        meshLines.Add(centroidLine);
                //    }
                //}

                var v1 = new Point(tri.GetVertex(0), map.OffsetX, map.OffsetY);
                var v2 = new Point(tri.GetVertex(1), map.OffsetX, map.OffsetY);
                var v3 = new Point(tri.GetVertex(2), map.OffsetX, map.OffsetY);

                var edge1 = new Line(v1, v2);
                var edge2 = new Line(v2, v3);
                var edge3 = new Line(v3, v1);

                //var c1 = new Line(v1, centroid);
                //var c2 = new Line(v2, centroid);
                //var c3 = new Line(v3, centroid);

                meshLines.Add(edge1);
                meshLines.Add(edge2);
                meshLines.Add(edge3);

                //meshLines.Add(c1);
                //meshLines.Add(c2);
                //meshLines.Add(c3);
            }

            Clients.Caller.GetMesh(meshLines);
        }

        public void FindPath(string args)
        {
            try
            {
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Start();
                object parsed = JsonConvert.DeserializeObject(args);

                dynamic obj = JObject.FromObject(parsed);

                var to = new Point((int)obj.To.X, (int)obj.To.Y);
                var from = new Point((int)obj.From.X, (int)obj.From.Y);
                string mapID = obj.map;

                //Console.WriteLine("Finding path!");
                if (!maps.ContainsKey(mapID))
                {
                    var newMap = new Map(mapID);
                    maps.Add(mapID, newMap);
                }

                var map = maps[mapID];

                var path = map.FindPath(from, to);
                var smoothed = map.SmoothPath(path);

                Clients.Caller.PathFound(smoothed);
                timer.Stop();
                Console.WriteLine("Path found in {0} ms", timer.ElapsedMilliseconds);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
