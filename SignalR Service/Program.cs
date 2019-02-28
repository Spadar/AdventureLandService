using AdventureLandLibrary.GameObjects;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR_Service
{
    class Program
    {
        private static Timer updater;
        private static object timerLock = new object();

        private class StateObjClass
        {
            public DateTime lastCheck;
            public Timer timerReference;
            public bool timerCanceled;
        }

        private static void StartUpdater()
        {
            var interval = 60000;
            StateObjClass stateObj = new StateObjClass();
            stateObj.timerCanceled = false;
            stateObj.lastCheck = DateTime.Now;

            TimerCallback timerDelegate = new TimerCallback(Update);

            updater = new Timer(timerDelegate, stateObj, 0, interval);

            stateObj.timerReference = updater;
        }

        private static void Update(object StateObj)
        {
            bool lockTaken = false;

            try
            {
                lockTaken = Monitor.TryEnter(timerLock);

                if(lockTaken)
                {
                    Maps.TryUpdateData();
                }
            }
            catch
            {
                Console.WriteLine("An issue has occurred in the update routine...");
            }
            finally
            {
                if(lockTaken)
                {
                    Monitor.Exit(timerLock);
                }
            }
        }

        static void Main(string[] args)
        {
            Maps.Load();
            StartUpdater();
            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/library/system.net.httplistener.aspx 
            // for more information.
            string url = "http://localhost:8080";
            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.ReadLine();
            }
        }

    }

    //public class AdventurelandHub : Hub
    //{
    //    public void Send(string name, string message)
    //    {
    //        Clients.All.addMessage(name, message);
    //    }

    //    public void Ping()
    //    {
    //        Clients.Caller.Pong(DateTime.Now);
    //    }
    //}
}
