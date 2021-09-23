using SocketIOSharp.Server.Client;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PolyChat.Models
{
    public class ChatPartner : INotifyPropertyChanged
    {
        public string Name;
        public string Code;
        public ObservableCollection<ChatMessage> Messages;
        private SocketIOSocket socketIOSocket;
        public event PropertyChangedEventHandler PropertyChanged;

        public ChatPartner(string name, string code, ObservableCollection<ChatMessage> messages = null)
        {
            Name = name;
            Code = code;
            if (messages == null) Messages = new ObservableCollection<ChatMessage>();
            else Messages = messages;
        }

        public SocketIOSocket SocketIOSocket { get => socketIOSocket; set => socketIOSocket = value; }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddMessage(ChatMessage message)
        {
            Messages.Add(message);
        }

        public void SetName(string name)
        {
            Name = name;
            NotifyPropertyChanged("Name");
        }
    }
}