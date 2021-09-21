using System;
using System.Net;
using EngineIOSharp.Common.Enum;
using PolyChat.Models;
using PolyChat.Models.Exceptions;
using SocketIOSharp.Client;
using SocketIOSharp.Common;
using SocketIOSharp.Common.Packet;
using SocketIOSharp.Server.Client;

namespace PolyChat
{
    public class Controller
    {
        public static string ip;
        private MainPage UIController;

        private ClientHandler clientHandler;
        public Controller(MainPage uiController)
        {
            UIController = uiController;
            clientHandler = new ClientHandler();
            Socket s = new Socket(this);
        }

        public void Connect(string ip)
        {
            clientHandler.connectNewClient(ip);
        }

        public void sendMessage(String ip, String name, String msg)
        {
            clientHandler.getClient(ip).sendMessage(SendCode.Message, name, msg, DateTime.Now);
        }

        /// <summary>
        /// prints out ip. on server side automatticaly finds 10.... ip (which is the correct one)
        /// </summary>
        /// <returns>users ip</returns>
        private String getIP()
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (input.Equals("server"))
                {
                    Console.WriteLine("starting as server...");
                    for (short i = 0; i < GetIPs().Length; i++)
                    {
                        if (GetIPs()[i].ToString().Substring(0, 3).Equals("10."))
                        {
                            Controller.ip = GetIPs()[i];
                            Console.WriteLine(GetIPs()[i]);
                            //get ip as byte array
                            byte[] ba = System.Text.Encoding.ASCII.GetBytes(GetIPs()[i].ToString());
                            foreach (var item in ba)
                            {
                                Console.Write(item.ToString() + ",");
                            }
                            break;
                        }
                    }
                    Socket s = new Socket(this);
                }
                else if (input.Equals("client"))
                {
                    ClientHandler cl = new ClientHandler();
                    Console.WriteLine("Enter IP:");
                    cl.connectNewClient(Console.ReadLine());
                }
            }
        }

        static void InitEventHandlers(SocketIOClient client)
        {
            client.On(SocketIOEvent.CONNECTION, () =>
            {
                Console.WriteLine("Connected!");
                client.Emit("Message", "This is a Message Body!");
            });

            client.On(SocketIOEvent.DISCONNECT, () =>
            {
                Console.WriteLine();
                Console.WriteLine("Disconnected!");
            });
        }



        public void OnMessageCallback(SocketIOSocket socket, SocketIOAckEvent message)
        {
            Console.WriteLine($"Message received from {socket.GetHashCode()}:{message.Data[0]}");
        }

        static string[] GetIPs()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            string[] ips = new string[addr.Length];
            for (int i=0; i<addr.Length; i++)
            {
                ips[i] = addr.ToString();
            }
            return ips;
        }

    }
}
