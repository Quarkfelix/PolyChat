using Json.Net;
using Newtonsoft.Json.Linq;
using System;

namespace PolyChat.Models
{
    public class Message
    {
        public readonly string Sender;
        public readonly DateTime Timestamp = new DateTime(1970, 01, 01);
        public readonly string Msg = "empty";
        public readonly string Ip;
        public readonly bool Foreign;
        public readonly string StringTimeStamp;

        /// <summary>
        /// create new Message object from parameters
        /// </summary>
        /// <param name="Msg"></param>
        /// <param name="Foreign"></param>
        /// <param name="Sender"></param>
        /// <param name="Ip"></param>
        public Message(string Msg = "", bool Foreign = true, string Sender= "Unknown", string Ip = "127.0.0.1")
        {
            this.Sender = Sender;
            this.Timestamp = DateTime.Now;
            StringTimeStamp = Timestamp.ToString();
            this.Msg = Msg;
            this.Foreign = Foreign;
            this.Ip = Ip;
        }

        /// <summary>
        /// create new Message object from JToken (json)
        /// </summary>
        /// <param name="data"></param>
        public Message(JToken data)
        {
            
            Message m = (Message) data.ToObject<Message>();
            Sender = m.Sender;
            Timestamp = m.Timestamp;
            StringTimeStamp = Timestamp.ToString();
            Msg = m.Msg;
            Ip = m.Ip;
            Foreign = m.Foreign;
        }

        public Message()
        {

        }
        

        override
        public string ToString()
        {
            string prefix = Foreign ? "Other" : "Me";
            return $"{prefix}: {Msg}({Sender})";
        }
    }
}