using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Net;
using PolyChat.Models;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;
using System.IO;
using System.Threading;
using System;
using System.Text;
using System.Security.Cryptography;

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
        // Props
        private Dictionary<string, Connection> Connections = new Dictionary<string, Connection>();
        private string OwnName = "";
        private string OwnIP;

        /// <summary>
        /// Initializes Controller with UI access
        /// </summary>
        /// <param name="uiController">UWP UI Controller</param>
        public Controller(MainPage uiController)
        {
            UIController = uiController;
            OwnIP = getIP();
            loadChats();
            //SaveChats("10", "{das ist ein test}");
            Serve();
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
                socket.On("initial", async (JToken[] data) =>
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
            SaveChats(ip, json.ToString(), DateTime.Now);
        }

        private void OnMessage(string ip, JToken[] data)
        {
            Debug.WriteLine("--- Controller.OnMessage ---");
            if (data != null && data.Length > 0 && data[0] != null)
            {
                DateTime now = DateTime.Now;
                Debug.WriteLine("RAW: " + data[0]);
                UIController.OnIncomingMessage(ip, data[0].ToString(), now);
                SaveChats(ip, data[0].ToString(), now);
            }
            else Debug.WriteLine("Undefined: " + data);
        }

        public void CloseChat(string IP, bool wasConnected = true)
        {
            Debug.WriteLine($"Deleting connection with IP:{IP}");
            if (IP != null && Connections.ContainsKey(IP))
            {
                Connections[IP].Close();
                Connections.Remove(IP);
            }
            CloseChatUI(IP, wasConnected);
        }

        private void CloseChatUI(string IP, bool wasConnected = true)
        {
            UIController.OnChatPartnerDeleted(IP);
            string heading = wasConnected ? "Connection Closed" : "Connection Failed";
            if (!wasConnected)
                UIController.ShowConnectionError(IP, heading, $"Connecting to {IP} failed...");
        }

        private bool isInConnections(string IP)
        {
            if (Connections.ContainsKey(IP))
                return true;
            return false;
        }

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

        /// <summary>
        /// sends chatlogs as json array to uiController wit corrosponding ip
        /// </summary>
        /// <param name="ip"></param>
        public void loadChats()
        {
            //load dir and create if non existant
            if (Directory.Exists("U:\\PolyChat\\Saves"))
            {
                Debug.WriteLine("--Path exists.--");
            }
            else
            {
                Directory.CreateDirectory("U:\\PolyChat\\Saves");
                Debug.WriteLine("--Path Created--.");
            }

            //go through all files and send ip and json array to ui
            String[] filepaths = Directory.GetFiles("U:\\PolyChat\\Saves");
            if (filepaths.Length > 0)
            {
                Debug.WriteLine("---Loading Saves");
                foreach (String path in filepaths)
                {
                    Debug.WriteLine($"--{path}");
                    String jsonArr = File.ReadAllText(path);
                    String ip = Path.GetFileName(path);
                    ip = ip.Substring(0, ip.Length - 4);
                    Debug.WriteLine($"-{ip}");
                    Debug.WriteLine(jsonArr);
                    Connect(ip);
                    UIController.OnIncomingConnection(ip);
                    UIController.OnIncomingMessages(ip, jsonArr);
                }
            }

        }

        /// <summary>
        /// Saves incoming chat message to 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="json"></param>
        public void SaveChats(string ip, string json, DateTime timeStamp)
        {
            //Vielleicht noch so machen dass die mit gleicher ip nacheinander gemacht
            //werden damit es nicht zu überschreibungen kommt vielleicth auch ganz oben oder am ende ne
            //writing flag setzen oder auch in der datei selbst ne flag setzen
            new Thread(() =>
            {
                if (File.Exists($"U:\\PolyChat\\Saves\\{ip}.txt"))
                {
                    Debug.WriteLine("--File allready exists--");
                    //check for integraty of file
                    string output = File.ReadAllText($"U:\\PolyChat\\Saves\\{ip}.txt");
                    Debug.WriteLine($"---{output}---");
                    if (output.Substring(0, 1).Equals("[") && output.Substring(output.Length - 1, 1).Equals("]"))
                    {
                        Debug.WriteLine("--adding new chatmessage--");
                        //structure intact
                        JObject obj = JObject.Parse(json);
                        //save new chat
                        String saved = output.Substring(0, output.Length - 1);
                        output = saved + ", " + json + " ]";
                        File.Delete($"U:\\PolyChat\\Saves\\{ip}.txt");
                        File.WriteAllText($"U:\\PolyChat\\Saves\\{ip}.txt", output);
                    }
                    else
                    {
                        Debug.WriteLine("--Structure not intact--");
                        Debug.WriteLine("--redoing file--");
                        //structure not intact
                        //redo file
                        File.Delete($"U:\\PolyChat\\Saves\\{ip}.txt");
                        File.WriteAllText($"U:\\PolyChat\\Saves\\{ip}.txt", $"[ {json} ]");
                    }
                }
                else
                {
                    Debug.WriteLine("--Creating new File--");
                    //setup file
                    File.WriteAllText($"U:\\PolyChat\\Saves\\{ip}.txt", $"[ {json} ]");
                }
            }).Start();
        }

        private void encode(string json) 
        {
            byte[] plaintext = Encoding.UTF8.GetBytes(json);
            byte[] entropy = new byte[20];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(entropy);
            }

            /*byte[] ciphertext = ProtectedData.Protect(plaintext, entropy,
                DataProtectionScope.CurrentUser);*/
        }
    }
}
