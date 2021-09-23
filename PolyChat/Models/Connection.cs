using System;
using System.Diagnostics;
using EngineIOSharp.Common.Enum;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Client;
using SocketIOSharp.Common;
using SocketIOSharp.Server.Client;
using PolyChat.Models;

namespace PolyChat.Models
{
    public class Connection
    {
        private SocketIOClient Client;
        private SocketIOSocket Socket;
        private bool Connected = false;
        private readonly string IP;
        private Action<string, bool> DeleteConnection;

        public Connection(string ip, ushort port, Action<JToken[]> onMessage, Action<string, bool> onClose)
        {
            Debug.WriteLine("! CONNECTING TO SERVER !");
            IP = ip;
            DeleteConnection = onClose;
            // establish connection
            Client = new SocketIOClient(new SocketIOClientOption(EngineIOScheme.http, ip, port));
            Client.Connect();
            // setup event listeners
            Client.On(SocketIOEvent.CONNECTION, OnConnect);
            Client.On(SocketIOEvent.DISCONNECT, OnDisconnect);
            Client.On(SocketIOEvent.ERROR, (JToken[] Data) => OnError(Data));
            Client.On("message", (Action<JToken[]>) onMessage);
        }
        
        public Connection(SocketIOSocket socket, Action<JToken[]> onMessage, Action<string, bool> onClose)
        {
            Socket = socket;
            DeleteConnection = onClose;
            Socket.On(SocketIOEvent.DISCONNECT, OnDisconnect);
            Socket.On(SocketIOEvent.ERROR, (JToken[] Data) => OnError(Data));
            Socket.On("message", (Action<JToken[]>) onMessage);

            //we are connected if we got here, inital packet was already received
            Connected = true;
        }
        public void SendMessage(string message)
        {
            Debug.WriteLine("--- Sending message ---");
            Debug.WriteLine($"Connected {Connected}");
            Debug.WriteLine($"Client {Client}");
            Debug.WriteLine($"Socket {Socket}");
            if (Socket != null) Socket.Emit("message", message);
            else if (Client != null) Client.Emit("message", message);
        }

        // Event Methods

        private void OnConnect()
        {
            Debug.WriteLine("--- Sending initial packet to server ---");
            Client.Emit("initial", IP);
            Debug.WriteLine("--- Connection successfull ---");
            Connected = true;
        }
        private void OnDisconnect()
        {
            Debug.WriteLine("--- Disconnected! ---");
            Debug.WriteLine($"--- Deleting Connection with IP: {IP}---");
            DeleteConnection(IP, IsConnected());
            Connected = false;
        }
        private void OnError(JToken[] data)
        {
            Debug.WriteLine("--- Error: ---");
            if (data != null && data.Length > 0 && data[0] != null)
                Debug.WriteLine(data[0]);
            else
                Debug.WriteLine("Unkown Error");
            Debug.WriteLine("---");
            Close();
        }

        // Getters

        public void Close()
        {
            if (Client != null) Client.Close();
            if (Socket != null) Socket.Close();
        }

        public bool IsConnected()
        {
            return Connected;
        }

        public string getIP()
        {
            return IP;
        }
    }
}
