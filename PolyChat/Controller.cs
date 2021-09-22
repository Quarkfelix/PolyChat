using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Net;
using PolyChat.Models;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;

namespace PolyChat
{
    class Controller
    {
        // Constants
        private const ushort PORT = 8050;
        // Controller
        private readonly MainPage UIController;
        // Props
        private Dictionary<string, Connection> Connections = new Dictionary<string, Connection>();
        private string OwnName = "";
        private string OwnIP;

        /// <summary>
        /// Initializes Controller with UI access
        /// </summary>
        /// <param name="uiController">UWP UI Controller</param>
        public Controller(MainPage uiController)
        {
            UIController = uiController;
            OwnIP = getIP();
            Serve();
        }

        public void Connect(string ip)
        {
            Debug.WriteLine("--- Controller.Connect ---");
            new Connection(ip, PORT, Data => OnMessage(Data));
        }

        private void Serve()
        {
            Debug.WriteLine("! SERVER STARTING !");
            SocketIOServer server = new SocketIOServer(new SocketIOServerOption(
                PORT
            ));
            server.Start();
            Debug.WriteLine("Port " + server.Option.Port);
            Debug.WriteLine("Path " + server.Option.Path);
            // listen for connection
            server.OnConnection((SocketIOSocket socket) =>
            {
                Debug.WriteLine("--- Client connected! ---");
                // setup event listeners
                socket.On("initial", (JToken[] data) =>
                {
                    Debug.WriteLine("--- initial packet received ---");
                    string ForeignIp = data.ToString();
                    //Todo deserialize inital packet and extract ip address
                    Connections.Add(ForeignIp, new Connection(socket, Data => OnMessage(Data)));
                });
            });
        }

        public void SendMessage(string ip, string message)
        {
            Debug.WriteLine("--- Controller.SendMessage ---");
            Connections[ip].SendMessage(message);
        }

        private void OnMessage(JToken[] data)
        {
            Debug.WriteLine("--- Controller.OnMessage ---");
            if (data != null && data.Length > 0 && data[0] != null)
            {
                Debug.WriteLine("Message: " + data[0]);
            }
            else Debug.WriteLine("Undefined: " + data);
        }

        public string getIP()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addrList = ipEntry.AddressList;
            for (short i = 0; i < addrList.Length; i++)
            {
                if (addrList[i].ToString().Substring(0, 3).Equals("10."))
                {
                    return addrList[i].ToString();
                }
            }
            return null;
        }
    }
}
