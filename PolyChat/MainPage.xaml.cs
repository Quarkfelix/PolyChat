﻿using PolyChat.Models;
using PolyChat.Util;
using PolyChat.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PolyChat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private NetworkingController networkingController;
        private ObservableCollection<ChatPartner> Partners;
        private ChatPartner selectedPartner = null;
        private string username;
        public MainPage()
        {
            this.InitializeComponent();
            // init controller
            networkingController = new NetworkingController(this);
            // ui variables
            ipAddress.Text = IP.GetCodeFromIP(networkingController.getIP().ToString());
            Partners = new ObservableCollection<ChatPartner>();
            updateNoChatsPlaceholder();
            updateNoUsernamePlaceholder();
            updateNoChatSelected();
            updateSendButtonEnabled();
        }

        public async void ShowConnectionError(string message)
        {
            ConnectionFailedDialog dialog = new ConnectionFailedDialog(message);
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                //retry
            }
            // else abort -> del chat
        }

        // EVENTS

        public void OnChatPartnerSelected(object sender, RoutedEventArgs e)
        {
            string code = ((RadioButton)sender).Tag.ToString();
            selectedPartner = Partners.First(p => p.Code == code);
            listViewMessages.ItemsSource = selectedPartner.Messages;
            selectedPartnerName.Text = selectedPartner.Name;
            updateNoChatSelected();
        }

        public void OnSendMessage(object sender = null, RoutedEventArgs e = null)
        {
            selectedPartner.AddMessage(new ChatMessage(
                inputSend.Text,
                false
            ));
            networkingController.sendMessage(selectedPartner.Code, inputSend.Text);
            // clear input
            inputSend.Text = "";
        }

        public async void OnOpenNewChatDialog(object sender = null, RoutedEventArgs e = null)
        {
            NewChatDialog dialog = new NewChatDialog();
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                string ip = IP.GetIPfromCode(dialog.getValue());
                networkingController.connectNewClient(ip);
                Partners.Add(new ChatPartner(
                    "Connecting...",
                    ip
                ));
                updateNoChatsPlaceholder();
            }
        }

        public async void OnOpenEditUsernameDialog(object sender = null, RoutedEventArgs e = null)
        {
            EditUsernameDialog dialog = new EditUsernameDialog(username);
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                username = dialog.getValue();
                if (username.Length == 0) textUsername.Text = "Unknown";
                else textUsername.Text = username;
            }
            updateNoUsernamePlaceholder();
        }

        public void OnIncomingMessage(ChatMessage message)
        {
            ChatPartner sendingPartner = Partners.First(p => p.Code == message.Ip);
            sendingPartner.AddMessage(new ChatMessage(
                message.Content,
                true,
                message.Sender
            ));
        }

        private void OnDeleteChat(object sender = null, RoutedEventArgs e = null)
        {
            Partners.Remove(selectedPartner);
            updateNoChatsPlaceholder();
        }

        private void OnKeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            updateSendButtonEnabled();
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                OnSendMessage();
            }
        }

        // UPDATE FUNCTIONS FOR UI PLACEHOLDERS

        private void updateNoChatsPlaceholder()
        {
            textNoChats.Visibility = Partners.Count() == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void updateNoUsernamePlaceholder()
        {
            if (username == null)
            {
                textNoUsername.Visibility = Visibility.Visible;
                textUsername.Visibility = Visibility.Collapsed;
            }
            else
            {
                textNoUsername.Visibility = Visibility.Collapsed;
                textUsername.Visibility = Visibility.Visible;
            }
        }

        private void updateNoChatSelected()
        {
            if (selectedPartner != null)
            {
                gridRight.Visibility = Visibility.Visible;
                textNoChatSelected.Visibility = Visibility.Collapsed;
            }
            else
            {
                gridRight.Visibility = Visibility.Collapsed;
                textNoChatSelected.Visibility = Visibility.Visible;
            }
        }

        private void updateSendButtonEnabled()
        {
            buttonSend.IsEnabled = inputSend.Text.Length != 0;
        }
    }
}
