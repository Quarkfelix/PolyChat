using System;
using System.Diagnostics;
using System.Text.Json;

namespace PolyChat.Models
{
    public class ChatMessage
    {
        public string Origin;
        public string Type;
        public string Content;
        public DateTime TimeStamp;
        public readonly bool Foreign;
        //
        public ChatMessage(string origin, string type, string content)
        {
            Origin = origin;
            TimeStamp = DateTime.Now;
            Type = type;
            Content = content;
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