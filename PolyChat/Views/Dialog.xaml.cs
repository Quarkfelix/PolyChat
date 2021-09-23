using PolyChat.Models;
using PolyChat.Util;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PolyChat.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Dialog : ContentDialog
    {
        public const string TYPE_ERROR = "error";
        public const string TYPE_SUCCESS = "success";
        private Action Primary;
        private Action Secondary;
        public Dialog(string type, string header, string message, DialogButton primary, DialogButton secondary)
        {
            this.InitializeComponent();
            Title = header;
            setType(type, message);
            PrimaryButtonText = primary.Text;
            SecondaryButtonText = secondary.Text;
            // TODO: use event handlers and asign actions here
            Primary = primary.Action;
            Secondary = secondary.Action;
        }

        private void setType(string type, string message)
        {
            switch (type)
            {
                case TYPE_ERROR:
                    textError.Text = message;
                    textError.Visibility = Visibility.Visible;
                    break;
                case TYPE_SUCCESS:
                    textSuccess.Text = message;
                    textSuccess.Visibility = Visibility.Visible;
                    break;
            }
        }

        private async void ShowDialogAsync()
        {
            await ShowAsync();
        }

        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Primary();
        }

        private void OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Secondary();
        }
    }
}
