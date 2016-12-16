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
    public sealed partial class UserDialog : ContentDialog
    {
        private User _user;
        public bool Dueled = false;
        public UserDialog(User user)
        {
            _user = user;
            InitializeComponent();
            LoadDetails();
        }

        private void LoadDetails()
        {
            NameBlock.Text = _user.Name;
            PointBlock.Text = _user.Coins + " Coins!";
            RewardsBlock.Text = _user.Rewards.Count + " Rewards earned!";
        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Dueled = false;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Dueled = true;
        }
    }
}
