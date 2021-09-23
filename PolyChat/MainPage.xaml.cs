using Newtonsoft.Json.Linq;
using PolyChat.Models;
using PolyChat.Util;
using PolyChat.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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
        private ChatPartner selectedPartner = null;
        private string username;
        private static ElementTheme Theme = ElementTheme.Light;
        
        public MainPage()
        {
            this.InitializeComponent();
            // init controller
            Controller = new Controller(this);
            // ui variables
            ipAddress.Text = IP.GetCodeFromIP(Controller.getIP());
            Partners = new ObservableCollection<ChatPartner>();
            // theming
            RequestedTheme = Theme;
            // updated placeholder
            updateNoChatsPlaceholder();
            updateNoUsernamePlaceholder();
            updateNoChatSelected();
            updateSendButtonEnabled();
        }

        public async void ShowConnectionError(string param, string heading, string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Dialog dialog = new Dialog(
                    Dialog.TYPE_ERROR,
                    heading,
                    message,
                    new DialogButton(
                        "Retry",
                        () =>
                        {
                            Controller.Connect(param);
                            Partners.Add(new ChatPartner(
                                "Connecting...",
                                param
                            ));
                            updateNoChatsPlaceholder();
                        }
                    ),
                    new DialogButton(
                        "Ignore",
                        () => { /* do nothing */ }
                    )
                );
            SafelyOpenDialog(dialog);
            });
        }

        // EVENTS

        public void OnSendMessage(object sender = null, RoutedEventArgs e = null)
        {
            selectedPartner.AddMessage(new ChatMessage(username, "message", inputSend.Text, DateTime.Now, false));
            Controller.SendMessage(selectedPartner.Code, "message", inputSend.Text);
            // clear input
            inputSend.Text = "";
        }

        public async void OnOpenNewChatDialog(object sender = null, RoutedEventArgs e = null)
        {
            // test
            /**/
            OnIncomingMessage(
                "localhost",
                new JObject(
                    new JProperty("type", "message"),
                    new JProperty("content", "Test")
                ).ToString(),
                DateTime.Now
             );
            /**/
            NewChatDialog dialog = new NewChatDialog();
            var result = await SafelyOpenDialog(dialog);
            if (result == ContentDialogResult.Primary)
            {
                string ip = IP.GetIPFromCode(dialog.getValue());
                Controller.Connect(ip);
                ChatPartner pa = Partners.FirstOrDefault(p => p.Code == ip);
                if (pa == null)
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
            var result = await SafelyOpenDialog(dialog);
            if (result == ContentDialogResult.Primary)
            {
                username = dialog.getValue();
                textUsername.Text = username;
                Controller.SendBroadcastMessage("username", username);
            }
            updateNoUsernamePlaceholder();
        }

        /// <summary>
        /// Adds a new ChatPartner to the UI with default Name.
        /// </summary>
        /// <param name="ip">IP Adress, gets shown as Util.IP > Code</param>
        public async void OnIncomingConnection(string ip)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Partners.Add(new ChatPartner(
                    "Connecting...",
                    ip
                ));
                updateNoChatsPlaceholder();
            });
        }
        /// <summary>
        /// Adds an message to the UI, based on .sender if known
        /// </summary>
        /// <param name="message">ChatMessage</param>
        public async void OnIncomingMessage(string origin, string json, DateTime timeStamp)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var doc = JsonDocument.Parse(json).RootElement;
                string type = doc.GetProperty("type").GetString();
                string content = doc.GetProperty("content").GetString();
                ChatPartner sendingPartner = Partners.FirstOrDefault(p => p.Code == origin);
                switch (type)
                {
                    case "username":
                        Debug.WriteLine($"! username change for {sendingPartner.Code} -> {content}");
                        sendingPartner.Name = content;
                        int index = Partners.IndexOf(sendingPartner);
                        Partners.Remove(sendingPartner);
                        Partners.Insert(index, sendingPartner);
                        break;
                    default:
                        sendingPartner.AddMessage(new ChatMessage(origin, type, content, timeStamp, true));
                        break;
                }
            });
        }
        public async void OnIncomingMessages(string origin, string json)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ChatPartner sendingPartner = Partners.FirstOrDefault(p => p.Code == origin);
                JArray arr = JArray.Parse(json);
                foreach (JObject item in arr)
                {
                    sendingPartner.AddMessage(
                        new ChatMessage(
                            origin,
                            item["type"].ToString(),
                            item["content"].ToString(),
                            DateTime.Parse(item["timestamp"].ToString()),
                            false // TODO: FIX !!!!
                        )
                    );
                }
            });
        }

        private void OnDeleteChat(object sender = null, RoutedEventArgs e = null)
        {
            Controller.CloseChat(selectedPartner.Code,delete: true);
            Partners.Remove(selectedPartner);
            updateNoChatsPlaceholder();
            updateNoChatSelected();
        }

        public async void OnChatPartnerDeleted(string code)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Partners.Remove(Partners.FirstOrDefault(p => p.Code.Equals(code)));
                selectedPartner = null;
                updateNoChatsPlaceholder();
                updateNoChatSelected();
            });
        }
        public void OnChatPartnerSelected(object sender, RoutedEventArgs e)
        {
            string code = ((RadioButton)sender).Tag.ToString();
            selectedPartner = Partners.FirstOrDefault(p => p.Code == code);
            listViewMessages.ItemsSource = selectedPartner.Messages;
            selectedPartnerName.Text = selectedPartner.Name;
            updateNoChatSelected();
            if (!Controller.IsConnected(code)) Controller.Connect(code);
        }

        public void OnToggleTheme(object sender, RoutedEventArgs e)
        {
            Theme = Theme == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;
            RequestedTheme = Theme;
        }

        private void OnKeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            updateSendButtonEnabled();
            if (buttonSend.IsEnabled && e.Key == Windows.System.VirtualKey.Enter)
            {
                OnSendMessage();
            }
        }

        public static IAsyncOperation<ContentDialogResult> SafelyOpenDialog(ContentDialog d)
        {
            if(VisualTreeHelper.GetOpenPopups(Window.Current).Count == 0)
                return d.ShowAsync();
            return null;
        }

        // GETTERS

        public static ElementTheme GetTheme() => Theme;

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
