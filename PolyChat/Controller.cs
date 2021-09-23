using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Net;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;
using PolyChat.Models;
using System.IO;
using System.Threading;
using System;
using System.Text;
using System.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

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
        private readonly FileManager fileManager;
        // Props
        private Dictionary<string, Connection> Connections = new Dictionary<string, Connection>();

        /// <summary>
        /// Initializes Controller with UI access
        /// </summary>
        /// <param name="uiController">UWP UI Controller</param>
        public Controller(MainPage uiController)
        {
            UIController = uiController;
            fileManager = new FileManager(this);
            //OwnIP = getIP();
            fileManager.loadChats();
            Serve();

            // test
            UIController.OnIncomingConnection("localhost");
            /*
            UIController.OnIncomingConnection("1.2.3.4");
            UIController.OnIncomingConnection("1.2.4.8");
            */
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
            {
                Connections.Add(ip, new Connection(ip, PORT, Data => OnMessage(ip, Data), CloseChat));
            }

        }

        /// <summary>
        /// starts server for clients to connect to
        /// </summary>
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
                socket.On("initial", (JToken[] data) =>
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
                        Debug.WriteLine("---- Added new Connection ----");
                        Connections.Add(ForeignIp, new Connection(socket, Data => OnMessage(ForeignIp, Data), CloseChat));
                        UIController.OnIncomingConnection(ForeignIp);
                    }
                });
            });
        }

        public void SendBroadcastMessage(string type, string content)
        {
            Debug.WriteLine("--- Controller.Broadcast ---");
            foreach (KeyValuePair<string, Connection> entry in Connections)
            {
                SendMessage(entry.Key, type, content);
            }
        }

        /// <summary>
        /// Sends message to given ip
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="type"></param>
        /// <param name="content"></param>
        public void SendMessage(string ip, string type, string content)
        {
            Debug.WriteLine("--- Controller.SendMessage ---");
            Debug.WriteLine($"{type} -> {ip} content: {content}");
            JObject json = new JObject(
                new JProperty("type", type),
                new JProperty("content", content)
            );
            Debug.WriteLine($"json: {json.ToString()}");
            // send as json
            Connections[ip].SendMessage(json.ToString());
            // save to logs
            fileManager.saveChats(ip, json.ToString(), DateTime.Now);
        }
        
        /// <summary>
        /// if We recieve a message this method gets triggert
        /// </summary>
        /// <param name="ip">ip from sender</param>
        /// <param name="data">String that is send</param>
        private void OnMessage(string ip, JToken[] data)
        {
            Debug.WriteLine("--- Controller.OnMessage ---");
            if (data != null && data.Length > 0 && data[0] != null)
            {
                DateTime now = DateTime.Now;
                Debug.WriteLine("RAW: " + data[0]);
                UIController.OnIncomingMessage(ip, data[0].ToString(), now);
                fileManager.saveChats(ip, data[0].ToString(), now);
            }
            else Debug.WriteLine("Undefined: " + data);
        }

        /// <summary>
        /// Closes chat connection
        /// </summary>
        /// <param name="IP">ip of user to be closed</param>
        /// <param name="wasConnected"></param>
        /// <param name="delete"></param>
        public void CloseChat(string IP, bool wasConnected = true, bool delete = false)
        {
            Debug.WriteLine($"Deleting connection with IP:{IP}");
            if (IP != null && Connections.ContainsKey(IP))
            {
                Connections[IP].Close();
                Connections.Remove(IP);
            }
            if (delete || !wasConnected)
                CloseChatUI(IP, wasConnected, delete);
            if (delete)
                fileManager.deleteChat(IP);
        }

        /// <summary>
        /// sends incoming message to ui
        /// </summary>
        /// <param name="ip">ip of client that send the message</param>
        /// <param name="jsonArr">the json array that is ti be displayed in th gui</param>
        public void SendIncomingMessageUI(String ip, String jsonArr) 
        {
            UIController.OnIncomingConnection(ip);
            UIController.OnIncomingMessages(ip, jsonArr);
        }

        private void CloseChatUI(string IP, bool wasConnected = true, bool delete = false)
        {
            UIController.OnChatPartnerDeleted(IP);
            string heading = wasConnected ? "Connection Closed" : "Connection Failed";
            if(!delete)
                UIController.ShowConnectionError(IP, heading, $"Connecting to {IP} failed...");
        }

        private bool isInConnections(string IP)
        {
            return Connections.ContainsKey(IP);
        }
        public bool IsConnected(string ip)
        {
            return Connections.ContainsKey(ip) && Connections[ip].IsConnected();
        }

        /// <summary>
        /// returns your own ip that starts with 10. becuase that is our subnet
        /// </summary>
        /// <returns></returns>
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
