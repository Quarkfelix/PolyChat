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
        private Controller Controller;
        private ObservableCollection<ChatPartner> Partners;
        private ChatPartner selectedPartner;
        private string username;
        public MainPage()
        {
            this.InitializeComponent();
            Controller = new Controller(this);
            Partners = new ObservableCollection<ChatPartner>();
            //ipAddress.Text = IP.GetCodeFromIP(Controller.GetIP());
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
                DateTime.Now.ToString(),
                false
            ));
            Controller.sendMessage(selectedPartner.Code, username, inputSend.Text);
            // clear input
            inputSend.Text = "";
        }

        public async void OnOpenNewChatDialog(object sender = null, RoutedEventArgs e = null)
        {
            NewChatDialog dialog = new NewChatDialog();
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                string ip = dialog.getValue();
                if (IP.ValidateIP(ip))
                {
                    Controller.Connect(ip);
                    Partners.Add(new ChatPartner(
                        "Connecting...",
                        ip
                    ));
                }
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
        }

        public void OnIncomingMessage(MSG message)
        {
            ChatPartner sendingPartner = Partners.First(p => p.Code == message.ip.ToString());
            sendingPartner.AddMessage(new ChatMessage(
                message.msg,
                message.timestamp.ToString(),
                true
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