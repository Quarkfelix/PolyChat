using System;
using System.Diagnostics;
using EngineIOSharp.Common.Enum;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Client;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;

namespace PolyChat
{
    public class Connection
    {
        private SocketIOClient Client;
        private SocketIOSocket Socket;
        private bool Connected = false;

        public Connection(string ip, ushort port, Action<JToken[]> onMessage)
        {
            Debug.WriteLine("! CONNECTING TO SERVER !");
            // establish connection
            Client = new SocketIOClient(new SocketIOClientOption(EngineIOScheme.http, ip, port));
            Client.Connect();
            // setup event listeners
            Client.On(SocketIOEvent.CONNECTION, OnConnect);
            Client.On(SocketIOEvent.DISCONNECT, OnDisconnect);
            Client.On(SocketIOEvent.ERROR, (JToken[] Data) => OnError(Data));
            Client.On("message", (Action<JToken[]>) onMessage);
        }

        public Connection(ushort port, Action<JToken[]> onMessage)
        {
            Debug.WriteLine("! SERVER STARTING !");
            SocketIOServer server = new SocketIOServer(new SocketIOServerOption(
                port
            ));
            server.Start();
            Debug.WriteLine("Port " + server.Option.Port);
            Debug.WriteLine("Path " + server.Option.Path);
            // listen for connection
            server.OnConnection((SocketIOSocket socket) =>
            {
                Console.WriteLine("--- Client connected! ---");
                Socket = socket;
                Connected = true;
                // setup event listeners
                Socket.On("input", (JToken[] data) =>
                {
                    Debug.WriteLine("--- Incoming input ---");
                    onMessage(data);
                    socket.Emit("echo", data);
                });
                Socket.On("message", (JToken[] data) =>
                {
                    Debug.WriteLine("--- Incoming message ---");
                    onMessage(data);
                    socket.Emit("echo", data);
                });
                Socket.On(SocketIOEvent.DISCONNECT, OnDisconnect);
                Socket.On(SocketIOEvent.ERROR, (JToken[] Data) => OnError(Data));
            });
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
            Debug.WriteLine("--- Connection successfull ---");
            Connected = true;
        }
        private void OnDisconnect()
        {
            Debug.WriteLine("--- Disconnected! ---");
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
        }

        // Getters

        public bool IsConnected()
        {
            return Connected;
        }
    }
}
