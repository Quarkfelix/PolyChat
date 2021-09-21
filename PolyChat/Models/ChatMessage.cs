using System;

namespace PolyChat.Models
{
    public class ChatMessage
    {
        public readonly string Sender;
        public readonly DateTime Timestamp = new DateTime(1970, 01, 01);
        public readonly string Content;
        public readonly string Ip;
        public readonly bool Foreign;

        public ChatMessage(string Content = "", bool Foreign = true, string Sender= "Unknown", string Ip = "127.0.0.1")
        {
            this.Sender = Sender;
            this.Timestamp = DateTime.Now;
            this.Content = Content;
            this.Foreign = Foreign;
            this.Ip = Ip;
        }

        override
        public string ToString()
        {
            string prefix = Foreign ? "Other" : "Me";
            return $"{prefix}: {Content}({Sender})";
        }
    }
}