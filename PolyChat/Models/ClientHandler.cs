using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using SocketIOSharp.Client;
using SocketIOSharp.Server.Client;
using EngineIOSharp.Common.Enum;
using Json.Net;

namespace PolyChat.Models
{
    class ClientHandler
    {
        public List<Client> clients = new List<Client>();
        public ClientHandler()
        {
        }

        /// <summary>
        /// connects new clients and saves them in list
        /// </summary>
        /// <param name="clientCode">ip adress of parter</param>
        public void connectNewClient(String clientCode)
        {
            //Todo: convert code into ip

            SocketIOClient connection = new SocketIOClient(new SocketIOClientOption(EngineIOScheme.http, clientCode, 8050));
            connection.Connect();
            clients.Add(new Client(connection, clientCode));
        }

        /// <summary>
        /// returns client that fits to ip adress
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public Client getClient(String ip)
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
