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
using AdventureLandLibrary.Pathfinding;
using System.Collections.Concurrent;

namespace SignalR_Service
{
    public class AdventurelandHub : Hub
    {
        public static ConcurrentDictionary<string, string> IDTONameMapping = new ConcurrentDictionary<string, string>();

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            IDTONameMapping.TryRemove(Context.ConnectionId, out string name);
            if (stopCalled)
            {
                Console.WriteLine(String.Format("Client {0} explicitly closed the connection.", name));
            }
            else
            {
                Console.WriteLine(String.Format("Client {0} timed out.", name));
            }

            return base.OnDisconnected(stopCalled);
        }

        public void Initialize(string name)
        {
            var id = Context.ConnectionId;
            if(!IDTONameMapping.ContainsKey(id))
            {
                IDTONameMapping.TryAdd(id, name);
            }

            Console.WriteLine(string.Format("Client {0} connected.", name));
        }

        public void SyncEntities(Entity[] entities)
        {
            if (IDTONameMapping.ContainsKey(Context.ConnectionId))
            {
                var name = IDTONameMapping[Context.ConnectionId];

                if(Entities.PlayerEntities.ContainsKey(name))
                {
                    Entities.PlayerEntities[name] = entities;
                }
                else
                {
                    Entities.PlayerEntities.TryAdd(name, entities);
                }
            }
            else
            {
                Console.WriteLine("Connection not initialized attempting to sync entities.");
            }
        }

        public void Ping()
        {
            Clients.Caller.Pong(DateTime.Now);
        }

        public void GetMesh(string mapID)
        {
            if (!Maps.MapDictionary.ContainsKey(mapID))
            {
                var newMap = new Map(mapID);
                Maps.MapDictionary.Add(mapID, newMap);
            }

            var map = Maps.MapDictionary[mapID];

            var mesh = map.Mesh;

            var meshLines = new List<Line>();

            foreach (var tri in mesh.Triangles)
            {

                var v1 = new Point(tri.GetVertex(0), map.OffsetX, map.OffsetY);
                var v2 = new Point(tri.GetVertex(1), map.OffsetX, map.OffsetY);
                var v3 = new Point(tri.GetVertex(2), map.OffsetX, map.OffsetY);

                var edge1 = new Line(v1, v2);
                var edge2 = new Line(v2, v3);
                var edge3 = new Line(v3, v1);


                meshLines.Add(edge1);
                meshLines.Add(edge2);
                meshLines.Add(edge3);
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
                string mapFrom = obj.From.Map;
                string mapTo = obj.To.Map;

                var path = Maps.FindPath(from, to, mapFrom, mapTo, false);

                Clients.Caller.PathFound(path);
                timer.Stop();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
