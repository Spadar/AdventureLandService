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

        public void FindPath(string args)
        {
            try
            {

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
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
