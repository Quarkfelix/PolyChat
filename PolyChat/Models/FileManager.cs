using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace PolyChat.Models
{
    class FileManager
    {
        // Controller
        private readonly Controller controller;


        //===============================================================================================================================================
        //Constructor
        //===============================================================================================================================================
        public FileManager(Controller controller)
        {
            this.controller = controller;
        }

        //===============================================================================================================================================
        // editing save files
        //===============================================================================================================================================

        /// <summary>
        /// deletes chatlog of one speciffic user
        /// </summary>
        /// <param name="ip"></param>
        public void deleteChat(String ip)
        {
            if (Directory.Exists("U:\\PolyChat\\Saves"))
            {
                Debug.WriteLine("--Path exists.--");
                //go through all files and send ip and json array to ui
                String[] filepaths = Directory.GetFiles("U:\\PolyChat\\Saves");
                if (filepaths.Length > 0)
                {
                    foreach (String path in filepaths)
                    {
                        if (Path.GetFileName(path).Equals(ip+".txt"))
                        {
                            File.Delete(path);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// loads one chatlog probably when someone tries to connect
        /// </summary>
        /// <param name="ip"></param>
        public void loadChat(String ip)
        {
            //load dir and create if non existant
            if (Directory.Exists("U:\\PolyChat\\Saves"))
            {
                Debug.WriteLine("--Path exists.--");
                if (File.Exists($"U:\\PolyChat\\Saves\\{ip}.txt"))
                {
                    String jsonArr = decrypt(File.ReadAllText($"U:\\PolyChat\\Saves\\{ip}.txt"));
                    controller.SendIncomingMessageUI(ip, jsonArr);
                }
            }
        }

        /// <summary>
        /// sends chatlogs as json array to uiController with corrosponding ip
        /// 
        /// in ui when chat is clicked connection gets established
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
                    String jsonArr = decrypt(File.ReadAllText(path));
                    String ip = Path.GetFileName(path);
                    ip = ip.Substring(0, ip.Length - 4);
                    Debug.WriteLine($"-{ip}");
                    Debug.WriteLine(jsonArr);
                    controller.SendIncomingMessageUI(ip, jsonArr);
                }
            }
        }

        /// <summary>
        /// Saves incoming chat message to U:\PolyChat\Saves\ip.txt
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="json"></param>
        public void saveChats(string ip, string json, DateTime timeStamp)
        {
            new Thread(() =>
            {
                //breaking if namechange
                JObject obj = JObject.Parse(json);
                if (!obj["type"].ToString().Equals("username"))
                {
                    //adding timestamp
                    obj.Add(new JProperty("timestamp", timeStamp));
                    json = obj.ToString();

                    if (File.Exists($"U:\\PolyChat\\Saves\\{ip}.txt"))
                    {
                        Debug.WriteLine("--File allready exists--");

                        //check for integraty of file
                        string output = decrypt(File.ReadAllText($"U:\\PolyChat\\Saves\\{ip}.txt"));
                        Debug.WriteLine($"---{output}---");
                        if (output.Substring(0, 1).Equals("[") && output.Substring(output.Length - 1, 1).Equals("]"))
                        {
                            //structure intact
                            //save new chat
                            Debug.WriteLine("--adding new chatmessage--");
                            output = output.Substring(0, output.Length - 1) + ", " + json + " ]"; //rip appart and put file back together
                            File.Delete($"U:\\PolyChat\\Saves\\{ip}.txt");
                            File.WriteAllText($"U:\\PolyChat\\Saves\\{ip}.txt", encrypt(output)); //encrypt and save to textfile
                        }
                        else
                        {
                            //structure not intact
                            //redo file
                            Debug.WriteLine("--Structure not intact--");
                            Debug.WriteLine("--redoing file--");
                            File.Delete($"U:\\PolyChat\\Saves\\{ip}.txt");
                            File.WriteAllText($"U:\\PolyChat\\Saves\\{ip}.txt", encrypt($"[ {json} ]")); //encrypt and write to file
                        }
                    }
                    else
                    {
                        //setup file
                        Debug.WriteLine("--Creating new File--");
                        File.WriteAllText($"U:\\PolyChat\\Saves\\{ip}.txt", encrypt($"[ {json} ]"));
                    }
                }
            }).Start();
        }


        //===============================================================================================================================================
        // Encryption
        //===============================================================================================================================================

        /// <summary>
        /// generates keypair [public, privte] (100% secure, trust me)
        /// </summary>
        /// <returns></returns>
        private String[] genKeys()
        {
            return new String[] {"12345678", "12345678" };
        }
        

        /// <summary>
        /// does exactly what it says. XD
        /// 
        /// encrypts given string and returns unreadable string
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <returns></returns>
        private String encrypt(String toEncrypt)
        {
            try
            {
                string textToEncrypt = toEncrypt;
                string ToReturn = "";
                string publickey = genKeys()[0];
                string secretkey = genKeys()[1];
                byte[] secretkeyByte = { };
                secretkeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = System.Text.Encoding.UTF8.GetBytes(textToEncrypt);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    ToReturn = Convert.ToBase64String(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        /// <summary>
        /// does exactly what it says. XD
        /// 
        /// takes in unreadable string and returns infirmation
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <returns></returns>
        private String decrypt(String toDecrypt)
        {
            try
            {
                string textToDecrypt = toDecrypt;
                string ToReturn = "";
                string publickey = genKeys()[0];
                string privatekey = genKeys()[1];
                byte[] privatekeyByte = { };
                privatekeyByte = System.Text.Encoding.UTF8.GetBytes(privatekey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = new byte[textToDecrypt.Replace(" ", "+").Length];
                inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    Encoding encoding = Encoding.UTF8;
                    ToReturn = encoding.GetString(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ae)
            {
                throw new Exception(ae.Message, ae.InnerException);
            }
        }
    }
}
