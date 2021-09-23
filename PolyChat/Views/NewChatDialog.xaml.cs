using PolyChat.Models;
using PolyChat.Util;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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
            IsPrimaryButtonEnabled = false;
        }

        public string getValue()
        {
            return input.Text;
        }

        private void OnKeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (!IP.ValidateIP(IP.GetIPFromCode(input.Text)))
            {
                textSuccess.Visibility = Visibility.Collapsed;
                textError.Visibility = Visibility.Visible;
                IsPrimaryButtonEnabled = false;
            } else
            {
                textSuccess.Visibility = Visibility.Visible;
                textError.Visibility = Visibility.Collapsed;
                IsPrimaryButtonEnabled = true;
            }
        }
    }
}
