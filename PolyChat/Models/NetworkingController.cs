using System;
using System.Diagnostics;
using System.Collections.Generic;
using SocketIOSharp.Client;
using EngineIOSharp.Common.Enum;
using System.Net;
using PolyChat.Models.Exceptions;

//dependencies for server functionality
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;
using Newtonsoft.Json.Linq;

namespace PolyChat.Models
{
    class NetworkingController
    {
        public List<Client> clients = new List<Client>();
        private String ownName = "";
        private IPAddress ownIP;
        private readonly ushort Port;
        private SocketIOServer Server;
        private readonly MainPage uiController;

        public NetworkingController (MainPage uiController, ushort Port = 8050)
        {
            this.uiController = uiController;
            this.Port = Port;
            ownIP = getIP();
            Server = new SocketIOServer(new SocketIOServerOption(Port));
            Server.OnConnection((socket) => connectNewClient(socket));
            Server.Start();
            Debug.WriteLine($"Server started, binding to port {Port}, waiting for connection...");
        }

        //EXTERNAL METHODS
        //=========================================================================================================================================================================================

        /// <summary>
        /// connects self to server with given ip
        /// </summary>
        /// <param name="ip"> server to connect to </param>
        public void connectNewClient(String ip) 
        {
            SocketIOClient connection = new SocketIOClient(new SocketIOClientOption(EngineIOScheme.http, ip, 8050));
            connection.Connect();
            clients.Add(new Client(connection, ip));
        }

        /// <summary>
        /// handle incomming connection 
        /// </summary>
        /// <param name="ip"> server to connect to </param>
        private void connectNewClient(SocketIOSocket socket)
        {
            socket.On(SendCode.Initial.ToString(), (JToken[] Data) =>
            {
                Message m = new Message(Data[0]);
                clients.Add(new Client(socket,m.Ip));
            });
        }

        /// <summary>
        /// sends Message to given ip
        /// </summary>
        /// <param name="ip"> partner to send to </param>
        /// <param name="msg"> to send </param>
        public void sendMessage(String ip, String msg) 
        {
            this.getClient(ip).sendMessage(SendCode.Message, msg);
        }

        /// <summary>
        /// returns own ip adress
        /// </summary>
        /// <returns></returns>
        public IPAddress getIP()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addrList = ipEntry.AddressList;

            for (short i = 0; i < addrList.Length; i++)
            {
                if (addrList[i].ToString().Substring(0, 3).Equals("10."))
                {
                    return addrList[i];
                }
            }
            return null;
        }

        /*
        /// <summary>
        /// changes name of self and sends new name to all chats
        /// </summary>
        /// <param name="newName"></param>
        public void changeName(String newName) 
        {
            this.ownName = newName;
            foreach(Client cl in clients)
            {
                cl.sendNameChange(SendCode.NameChange, newName);
            }
        }
        */

        //=========================================================================================================================================================================================
        //INTERNAL METHODS
        //========================================================================================================================================================================================= 

        /// <summary>
        /// returns client that fit to ip address
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private Client getClient(String ip)
        {
            foreach (Client cl in clients)
            {
                if (cl.getIP().Equals(ip))
                {
                    return cl;
                }
            }
            return null;
        }
    }
}
