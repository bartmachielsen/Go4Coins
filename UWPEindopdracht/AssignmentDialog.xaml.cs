using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using UWPEindopdracht.DataConnections;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPEindopdracht
{
    public sealed partial class AssignmentDialog : ContentDialog
    {
        private Assignment _assignment;
        private string _imageURL;
        public bool Accepted = true;
        public AssignmentDialog(Assignment assignment, string imageURL, bool loaded = false)
        {
            this.InitializeComponent();
            _assignment = assignment;
            if (loaded)
            {
                SkipButton.Content = "Stop";
            }
            _imageURL = imageURL;
            LoadDetails();
        }

        private void LoadDetails()
        {
            AssignmentDetails.Text = _assignment.GetDescription();
            AssignmentName.Text = _assignment.Name;
            if(_imageURL != null)
                AssignmentImage.Source = new BitmapImage(new Uri(_imageURL));
            SkipButton.IsEnabled = _assignment.Skippable;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Accepted = true;
            Hide();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            Accepted = false;
            Hide();
        }
    }
}
