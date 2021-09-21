using SocketIOSharp.Server.Client;
using System.Collections.ObjectModel;

namespace PolyChat.Models
{
    public class ChatPartner
    {
        public string Name;
        public string Code;
        public ObservableCollection<Message> Messages;
        private SocketIOSocket socketIOSocket;

        public ChatPartner(string name, string code, ObservableCollection<Message> messages = null)
        {
            Name = name;
            Code = code;
            if (messages == null) Messages = new ObservableCollection<Message>();
            else Messages = messages;
        }

        public SocketIOSocket SocketIOSocket { get => socketIOSocket; set => socketIOSocket = value; }

        public void AddMessage(Message message)
        {
            Messages.Add(message);
        }
    }
}