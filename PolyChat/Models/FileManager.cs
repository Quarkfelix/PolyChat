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
        private readonly MainPage UIController;

        public FileManager(MainPage uiController)
        {
            UIController = uiController;
        }

        /// <summary>
        /// sends chatlogs as json array to uiController wit corrosponding ip
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
        public void saveChats(string ip, string json, DateTime timeStamp)
        {
            //Vielleicht noch so machen dass die mit gleicher ip nacheinander gemacht
            //werden damit es nicht zu überschreibungen kommt vielleicth auch ganz oben oder am ende ne
            //writing flag setzen oder auch in der datei selbst ne flag setzen
            new Thread(() =>
            {
                //breaking if namechange
                JObject obj = JObject.Parse(json);
                if (!obj["type"].ToString().Equals("username"))
                {
                    //adding timestamp
                    obj = JObject.Parse(json);
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
                            Debug.WriteLine("--adding new chatmessage--");
                            //structure intact
                            //save new chat
                            String saved = output.Substring(0, output.Length - 1);
                            output = saved + ", " + json + " ]";
                            File.Delete($"U:\\PolyChat\\Saves\\{ip}.txt");
                            File.WriteAllText($"U:\\PolyChat\\Saves\\{ip}.txt", encrypt(output));
                        }
                        else
                        {
                            Debug.WriteLine("--Structure not intact--");
                            Debug.WriteLine("--redoing file--");
                            //structure not intact
                            //redo file
                            File.Delete($"U:\\PolyChat\\Saves\\{ip}.txt");
                            File.WriteAllText($"U:\\PolyChat\\Saves\\{ip}.txt", encrypt($"[ {json} ]"));
                        }
                    }
                    else
                    {
                        Debug.WriteLine("--Creating new File--");
                        //setup file
                        File.WriteAllText($"U:\\PolyChat\\Saves\\{ip}.txt", encrypt($"[ {json} ]"));
                    }
                }
            }).Start();
        }


        //---------------------------------------------------------------------------------------------------
        //security
        //---------------------------------------------------------------------------------------------------
        private void genKeys() 
        { 
            
        }


        /// <summary>
        /// does exactly what it says. XD
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <returns></returns>
        public static String encrypt(String toEncrypt)
        {
            try {
                string textToEncrypt = toEncrypt;
                string ToReturn = "";
                string publickey = "santhosh";
                string secretkey = "engineer";
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
            } catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

        }

        /// <summary>
        /// does exactly what it says. XD
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <returns></returns>
        public static String decrypt(String toDecrypt)
        {
            try
            {
                string textToDecrypt = toDecrypt;
                string ToReturn = "";
                string publickey = "santhosh";
                string privatekey = "engineer";
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
