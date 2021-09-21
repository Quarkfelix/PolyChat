using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Json.Net;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;

namespace PolyChat.Models
{
    class Socket
    {
        private Controller p;
        private readonly ushort Port;
        private SocketIOServer Server;
        private List<SocketIOSocket> Sockets = new List<SocketIOSocket>();

        /// <summary>
        /// creates server on specified port
        /// </summary>
        /// <param name="Port"></param>
        public Socket(Controller p, ushort Port = 8050)
        {
            this.Port = Port;
            this.p = p;
            Server = new SocketIOServer(new SocketIOServerOption(Port));
            Server.OnConnection((socket) => OnConnect(socket));
            Server.Start();
            Console.WriteLine($"Server started, binding to port {Port}, waiting for connection...");
        }

        private void OnConnect(SocketIOSocket socket)
        {
            Console.WriteLine($"{socket.GetHashCode()} connected to the server");
            Sockets.Add(socket);
            socket.On(SocketIOEvent.DISCONNECT, () => OnDisconnect(socket));
            socket.On("Message", (Data) => p.OnMessageCallback(socket, Data));
        }

        private void OnDisconnect(SocketIOSocket socket)
        {
            Sockets.Remove(socket);
        }

        public bool SendMessage(SocketIOSocket socket)
        {
            
            return false;
        }
    }
}
