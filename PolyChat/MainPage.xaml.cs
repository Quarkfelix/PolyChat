﻿using PolyChat.Models;
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
        private ChatPartner selectedPartner;
        public MainPage()
        {
            this.InitializeComponent();
            networkingController = new NetworkingController(this);

            Partners = new ObservableCollection<ChatPartner>();
        }

        public void OnChatPartnerSelected(object sender, RoutedEventArgs e)
        {
            string code = ((RadioButton)sender).Tag.ToString();
            selectedPartner = Partners.First(p => p.Code == code);
            listViewMessages.ItemsSource = selectedPartner.Messages;
            selectedPartnerName.Text = selectedPartner.Name;
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
                string ip = dialog.getText();
                networkingController.connectNewClient(ip);
                Partners.Add(new ChatPartner(
                    "NO NAME",
                    ip
                ));
            }
        }

        public void OnIncomingMessage(ChatMessage message)
        {
            ChatPartner sendingPartner = Partners.First(p => p.Code == message.Ip);
            sendingPartner.AddMessage(new ChatMessage(
                message.Msg,
                true,
                message.Sender
            ));
        }

        private void OnDeleteChat(object sender = null, RoutedEventArgs e = null)
        {
            Partners.Remove(selectedPartner);
        }

        private void OnKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                OnSendMessage();
            }
        }
    }
}
