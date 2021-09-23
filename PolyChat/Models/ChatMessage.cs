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
        
        /// <summary>
        /// Create own Message (directly sent)
        /// </summary>
        /// <param name="origin">My IP</param>
        /// <param name="type">Message Type</param>
        /// <param name="content">Message Content (not JSON)</param>
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

        /// <summary>
        /// Create Message loaded with timestamp
        /// </summary>
        /// <param name="origin">Origin IP</param>
        /// <param name="type">Message Type</param>
        /// <param name="content">Message Content (not JSON)</param>
        /// <param name="timeStamp">Message Content (not JSON)</param>
        public ChatMessage(string origin, string type, string content, DateTime timeStamp, bool foreign = false)
        {
            Origin = origin;
            TimeStamp = timeStamp;
            Type = type;
            Content = content;
            Foreign = foreign;
            Debug.WriteLine("Created Loaded Message: " + ToString());
        }

        /// <summary>
        /// Create foreign Message (directly incoming)
        /// </summary>
        /// <param name="origin">Foreign IP</param>
        /// <param name="json">Message Content as JSON with type and content</param>
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