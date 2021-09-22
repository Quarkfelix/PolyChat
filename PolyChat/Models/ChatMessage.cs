using System;
using System.Diagnostics;
using System.Text.Json;

namespace PolyChat.Models
{
    public class ChatMessage
    {
        private string Origin;
        private string Type;
        private string Content;
        private DateTime TimeStamp;
        public readonly bool Foreign;
        //
        public readonly string Ip;

        public ChatMessage(string content = "", string origin = "Unknown", string ip = "127.0.0.1")
        {
            Origin = origin;
            TimeStamp = DateTime.Now;
            Content = content;
            Ip = ip;
            // no json = my messages
            Foreign = false;
            Debug.WriteLine("Created Message: " + ToString());
        }

        public ChatMessage(string origin, string json)
        {
            Origin = origin;
            // parse and save to object
            var obj = JsonDocument.Parse(json).RootElement;
            Type = obj.GetProperty("type").GetString();
            Content = obj.GetProperty("content").GetString();
            TimeStamp = DateTime.Now;
            // json = foreign
            Foreign = true;
            Debug.WriteLine("Created Message: " + ToString());
        }

        override
        public string ToString()
        {
            string prefix = Foreign ? "Other" : "Me";
            return $"{Type} from {prefix}: {Content}({Origin})";
        }
    }
}