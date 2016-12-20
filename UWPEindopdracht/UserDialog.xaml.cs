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
        public bool Dueled;
        public UserDialog(User user, bool newUser = true)
        {
            _user = user;
            InitializeComponent();
            LoadDetails(newUser);
        }

        private void LoadDetails(bool newuser)
        {
            NameBlock.Text = _user.Name;
            PointBlock.Text = _user.Coins + " Coins!";
            
            if(_user.Rewards != null)
                RewardsBlock.Text = _user.Rewards.Count + " Rewards earned!";
            else
                RewardsBlock.Text = "No rewards earned yet!";
        }

        private void IgnoreButton_Click(object sender, RoutedEventArgs e)
        {
            Dueled = false;
            Hide();
        }

        private void ChallengeButton_Click(object sender, RoutedEventArgs e)
        {
            Dueled = true;
            Hide();
        }
    }
}
