using SocketIOSharp.Server.Client;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PolyChat.Models
{
    public class ChatPartner
    {
        public string Name;
        public string Code;
        public ObservableCollection<ChatMessage> Messages;
        private SocketIOSocket socketIOSocket;

        public ChatPartner(string name, string code, ObservableCollection<ChatMessage> messages = null)
        {
            Name = name;
            Code = code;
            if (messages == null) Messages = new ObservableCollection<ChatMessage>();
            else Messages = messages;
        }

        public SocketIOSocket SocketIOSocket { get => socketIOSocket; set => socketIOSocket = value; }

        public void AddMessage(ChatMessage message)
        {
            Messages.Add(message);
        }
    }
}