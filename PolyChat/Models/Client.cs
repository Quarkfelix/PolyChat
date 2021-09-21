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
using System.Net;
using SocketIOSharp.Client;
using SocketIOSharp.Common;
using SocketIOSharp.Common.Packet;
using System;
using System.Net;
using EngineIOSharp.Common.Enum;
using System.Threading;

namespace PolyChat.Models
{
    class Client
    {
        private SocketIOClient connection;
        public Boolean isConnected = false;
        private List<ChatMessage> msgStack = new List<ChatMessage>();
        private Boolean active = true;
        private String ip;

        public Client(SocketIOClient connection, String ip)
        {
            this.ip = ip;
            this.connection = connection;
            InitEventHandlers(this, connection);
        }

        /// <summary>
        /// converts String message into json file and sends it to the server.
        /// </summary>
        /// <remarks>
        /// gets called by gui if someone wants to send Message
        /// </remarks>
        /// <param name="sender">Sender of Message</param>
        /// <param name="chatMessage">the accual text the user wants to send</param>
        /// <param name="timestamp">current time</param>
        public void sendMessage(SendCode code, String sender, String chatMessage, DateTime timestamp)
        {
            new Thread(() =>
            {
                //create msg
                ChatMessage msg = new ChatMessage(timestamp, chatMessage, false, sender, Controller.ip);

                //convert msg
                String petJson = JsonNet.Serialize(msg);

                //send msg
                connection.Emit(code.ToString(), petJson);
            }).Start();
        }

        /*
        private void recieveMessage(String msg)
        {
            // deserialize json string
            MSG pet = JsonNet.Deserialize<MSG>(msg);

            //TODO: send message to GUI
        }
        */

        /// <summary>
        /// handles all events of client server communiation
        /// </summary>
        /// <param name="client">self</param>
        /// <param name="connection"></param>
        private static void InitEventHandlers(Client client, SocketIOClient connection)
        {
            connection.On(SendCode.Message.ToString(), (Data) =>
            {
                ChatMessage pet = JsonNet.Deserialize<ChatMessage>(BitConverter.ToString(Data[0].ToObject<byte[]>()));
                //TODO: send message to GUI
            });
            connection.On(SendCode.Command.ToString(), (Data) =>
            {
                Console.WriteLine("Command recieved!" + Data[0]);
            });
            connection.On(SendCode.test1.ToString(), (Data) =>
            {
                Console.WriteLine("test1 recieved!" + Data[0]);
            });
            connection.On(SendCode.test2.ToString(), (Data) =>
            {
                Console.WriteLine("test2 recieved!" + Data[0]);
            });

            connection.On(SocketIOEvent.CONNECTION, () =>
            {
                Console.WriteLine("Connected!");
                client.isConnected = true;
            });
        }

        public String getIP()
        {
            return this.ip;
        }
    }

}
