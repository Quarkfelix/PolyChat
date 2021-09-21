namespace PolyChat.Models
{
    public class ChatMessage
    {
        public string Text;
        public string Date;
        public bool Foreign;

        public ChatMessage(string text, string date, bool foreign)
        {
            Text = text;
            Date = date;
            Foreign = foreign;
        }

        override
        public string ToString()
        {
            string prefix = Foreign ? "Other" : "Me";
            return $"{prefix}: Text";
        }
    }
}