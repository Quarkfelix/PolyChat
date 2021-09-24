using System;
using System.Diagnostics;
using System.Text.Json;
using Windows.UI.Xaml;

namespace PolyChat.Models
{
    public class ChatMessage
    {
        public string Origin;
        public string Type;
        public string Content;
        public DateTime TimeStamp;
        public HorizontalAlignment Align;
        private bool Foreign;

        /// <summary>
        /// Create Message
        /// </summary>
        /// <param name="origin">Origin IP</param>
        /// <param name="type">Message Type, usually "message"</param>
        /// <param name="content">Message Content, usually plain text</param>
        /// <param name="timeStamp">Parsed DateTime</param>
        public ChatMessage(string origin, string type, string content, DateTime timeStamp, bool foreign)
        {
            Origin = origin;
            TimeStamp = timeStamp;
            Type = type;
            Content = content;
            Align = foreign ? HorizontalAlignment.Right : HorizontalAlignment.Left;
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