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
            // TODO
            Foreign = false;
            Debug.WriteLine("Created Message: " + ToString());
        }

        /// <summary>
        /// Create Message loaded with timestamp
        /// </summary>
        /// <param name="origin">Origin IP</param>
        /// <param name="type">Message Type, usually "message"</param>
        /// <param name="content">Message Content, usually plain text</param>
        /// <param name="timeStamp">Parsed DateTime</param>
        public ChatMessage(string origin, string type, string content, DateTime timeStamp, bool foreign = false)
        {
            Origin = origin;
            TimeStamp = timeStamp;
            Type = type;
            Content = content;
            Foreign = foreign;
            Debug.WriteLine("Created Loaded Message: " + ToString());
        }

        override
        public string ToString()
        {
            string prefix = Foreign ? "Other" : "Me";
            return $"{Type} from {prefix}: {Content}({Origin})";
        }
    }
}