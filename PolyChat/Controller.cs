using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Net;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;
using PolyChat.Models;
using System.Text.Json;

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
                socket.On("initial", async (JToken[] data) =>
                {
                    Debug.WriteLine("--- initial packet received ---");
                    string ForeignIp = data.ToString();
                    //Todo deserialize inital packet and extract ip address
                    Connections.Add(ForeignIp, new Connection(socket, Data => OnMessage(Data)));
                    UIController.OnIncomingConnection(ForeignIp);
                });
            });
        }

        public void SendMessage(string ip, string type, string content)
        {
            Debug.WriteLine("--- Controller.SendMessage ---");
            Debug.WriteLine($"{type} -> {ip} content: {content}");
            string json = $"{{ type: {type}, content: {content} }}";
            Debug.WriteLine($"json: {json}");
            Connections[ip].SendMessage(json);
        }

        private void OnMessage(JToken[] data)
        {
            Debug.WriteLine("--- Controller.OnMessage ---");
            if (data != null && data.Length > 0 && data[0] != null)
            {
                Debug.WriteLine("Message: " + data[0]);
                Debug.WriteLine($"DATA: {data[0].ToString()}");
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
