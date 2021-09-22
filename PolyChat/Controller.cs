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

            //Connect("10.1.211.26"); // Marc
            //Connect("10.1.218.90"); // Felix
            //Connect("10.4.141.77"); // Pat
        }

        public void Connect(string ip)
        {
            Debug.WriteLine("--- Controller.Connect ---");
            Connections.Add(ip, new Connection(ip, PORT, Data => OnMessage(ip, Data)));
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
                    Connections.Add(ForeignIp, new Connection(socket, Data => OnMessage(ForeignIp, Data)));
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

        private void OnMessage(string ip, JToken[] data)
        {
            Debug.WriteLine("--- Controller.OnMessage ---");
            if (data != null && data.Length > 0 && data[0] != null)
            {
                Debug.WriteLine("Message: " + data[0]);
                Debug.WriteLine($"RAW: {data[0].ToString()}");
                UIController.OnIncomingMessage(ip, data[0].ToString());
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
