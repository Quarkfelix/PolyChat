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
        private readonly MainPage UIController;

        private ClientHandler clientHandler;
        /*
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
            clientHandler.getClient(ip).send Message(SendCode.Message, msg, DateTime.Now);
        }
        */
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
                }
                else if (input.Equals("client"))
                {
                    ClientHandler cl = new ClientHandler();
                    Console.WriteLine("Enter IP:");
                    cl.connectNewClient(Console.ReadLine());
                }
            }
        }


        static string[] GetIPs()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            string[] ips = new string[addr.Length];
            for (int i = 0; i < addr.Length; i++)
            {
                ips[i] = addr.ToString();
            }
            return ips;
        }

    }
}
