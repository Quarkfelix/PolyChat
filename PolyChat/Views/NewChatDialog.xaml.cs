using PolyChat.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PolyChat.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewChatDialog : ContentDialog
    {
        public NewChatDialog()
        {
            this.InitializeComponent();
        }

        public string getText()
        {
            return inputIP.Text;
        }

        public void OnConnect(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        public void OnClose(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
