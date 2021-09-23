using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Net;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;
using PolyChat.Models;
using System.Text.Json;
using Newtonsoft.Json;

namespace PolyChat
{

    // 10.1.211.26 Marc
    // 10.1.218.90 Felix
    // 10.4.141.77 Pat
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
            if (isInConnections(ip))
            {
                Debug.WriteLine("---- We have an active connection to this client. ABORT! ----");
                CloseChatUI(ip);
                //Todo show error!
            }
            else
                Connections.Add(ip, new Connection(ip, PORT, Data => OnMessage(ip, Data), CloseChat));
        }

        private void Serve()
        {
            Debug.WriteLine("--- Controller.Serve ---");
            SocketIOServer server = new SocketIOServer(new SocketIOServerOption(PORT));
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
                    string ForeignIp = data[0].ToString();
                    Debug.WriteLine($"--- this ip was in the inital packet: {ForeignIp} ---");
                    if (isInConnections(ForeignIp))
                    {
                        Debug.WriteLine("---- We have an active connection to this client. ABORT! ----");//Todo show error!
                        CloseChatUI(ForeignIp);
                    }
                    else
                    {
                        Connections.Add(ForeignIp, new Connection(socket, Data => OnMessage(ForeignIp, Data), CloseChat));
                        UIController.OnIncomingConnection(ForeignIp);
                    }
                });
            });
        }

        public void SendMessage(string ip, string type, string content)
        {
            Debug.WriteLine("--- Controller.SendMessage ---");
            Debug.WriteLine($"{type} -> {ip} content: {content}");
            JObject json = new JObject(
                new JProperty("type", type),
                new JProperty("content", content)
            );
            Debug.WriteLine($"json: {json.ToString()}");
            Connections[ip].SendMessage(json.ToString());
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

        public void CloseChat(string IP, bool wasConnected = true)
        {
            Debug.WriteLine($"Deleting connection with IP:{IP}");
            if (IP != null && Connections.ContainsKey(IP))
            {
                Connections[IP].Close();
                Connections.Remove(IP);
            }
            CloseChatUI(IP,wasConnected);
        }

        private void CloseChatUI(string IP, bool wasConnected = true)
        {
            UIController.OnChatPartnerDeleted(IP);
            if (!wasConnected)
                UIController.ShowConnectionError("Connection close", IP, $"Connection to {IP} failed...");
        }

        private bool isInConnections(string IP)
        {
            if(Connections.ContainsKey(IP))
                return true;
            return false;
        }

        public static string getIP()
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
