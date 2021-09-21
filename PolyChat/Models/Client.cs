using System;
using SocketIOSharp.Common;
using SocketIOSharp.Client;
using SocketIOSharp.Server.Client;
using Json.Net;
using System.Threading;
using PolyChat.Models.Exceptions;
using System.Diagnostics;

namespace PolyChat.Models
{
    class Client
    {
        private SocketIOClient connection_client = null;
        private SocketIOSocket connection_server = null;
        private Boolean connected = true;
        private String ipSelf;

        public Client(SocketIOClient connection, String ip, MainPage uiController)
        {
            this.ipSelf = ip;
            this.connection_client = connection;
            InitEventHandlers(this, connection, uiController);
        }

        public Client(SocketIOSocket connection, String ip, MainPage uiController)
        {
            this.ipSelf = ip;
            this.connection_server = connection;
            InitEventHandlers(this, connection, uiController);
        }

        //Sending
        //===================================================================================
        /// <summary>
        /// converts String message into json file and sends it to the server.
        /// </summary>
        /// <remarks>
        /// gets called by gui if someone wants to send Message
        /// </remarks>
        /// <param name="sender">Sender of Message</param>
        /// <param name="chatMessage">the accual text the user wants to send</param>
        /// <param name="timestamp">current time</param>
        public void sendMessage(SendCode code, String chatMessage)
        {
            new Thread(() =>
            {
                Debug.WriteLine($"connected is {connected}");
                //create msg
                Message msg = new Message(chatMessage, false, ipSelf);

                //convert msg
                String petJson = JsonNet.Serialize(msg);

                //wait if not connected and send msg
                int i=0;
                int sleeptimer = 2000;
                while(!this.connected)
                {
                    Thread.Sleep(sleeptimer);
                    i++;
                    if(i>=10)
                    {
                        throw new MessageTimedOutException(i*sleeptimer);
                    }
                }
                if (connection_client != null)
                {
                    connection_client.Emit(code.ToString(), petJson);
                }else if (connection_server != null)
                {
                    connection_server.Emit(code.ToString(), petJson);
                }
            }).Start();
        }

        //==================================================================================
        //EventHandeling
        //===================================================================================
        
        /// <summary>
        /// handles all events of client
        /// </summary>
        /// <param name="client">self</param>
        /// <param name="connection"></param>
        private static void InitEventHandlers(Client client, SocketIOClient connection, MainPage uiController)
        {
            connection.On(SendCode.Message.ToString(), (Data) =>
            {
                Message msg = new Message(Data[0]);
                uiController.OnIncomingMessage(msg);

                //TODO: send message to GUI
            });

            connection.On(SendCode.Command.ToString(), (Data) =>
            {
                Console.WriteLine("Command recieved!" + Data[0]);
                
            });

            connection.On(SocketIOEvent.CONNECTION, () =>
            {
                client.connected = true;
            });
        }

        /// <summary>
        /// handles all events of server
        /// </summary>
        /// <param name="client">self</param>
        /// <param name="connection"></param>
        private static void InitEventHandlers(Client client, SocketIOSocket connection, MainPage uiController)
        {
            connection.On(SendCode.Message.ToString(), (Data) =>
            {
                Message msg = new Message(Data[0]);
                uiController.OnIncomingMessage(msg);
                //TODO: send message to GUI
            });

            connection.On(SendCode.Command.ToString(), (Data) =>
            {
                Console.WriteLine("Command recieved!" + Data[0]);
            });

            client.connected = true;
        }
        //==================================================================================
        //Getter and Setter
        //==================================================================================

        public String getIP()
        {
            return this.ipSelf;
        }

        public Boolean isConnected()
        {
            return this.connected;
        }
    }

}
