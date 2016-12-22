using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UWPEindopdracht.Multiplayer;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPEindopdracht
{
    public sealed partial class MultiplayerSettings : ContentDialog
    {
        private User _user; 
          
        public MultiplayerSettings(User user)
        {
            this._user = user;
            this.InitializeComponent();
            UsernameText.Text = user.Name;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _user.Name = UsernameText.Text;
            Hide();
        }
    }
}
