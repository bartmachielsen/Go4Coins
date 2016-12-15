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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using UWPEindopdracht.DataConnections;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPEindopdracht
{
    public sealed partial class AssignmentDialog : ContentDialog
    {
        private Assignment _assignment;
        public AssignmentDialog(Assignment assignment)
        {
            this.InitializeComponent();
            this._assignment = assignment;
            IsPrimaryButtonEnabled = false;
            IsSecondaryButtonEnabled = false;
            LoadImage();
            LoadDetails();
        }

        private void LoadDetails()
        {
            AssignDetails.Text = _assignment.Description;
            Title = _assignment.Target[0].Name;
        }

        private async void LoadImage()
        {
            if (_assignment.Target[0].ImageLocation != null)
            {
                string url = await new GooglePlacesConnector().GetImageURL(_assignment.Target[0], (int)((this.ActualWidth/4.0)*3.0));
                if (url != null) { 
                    AssignmentImage.Source = new BitmapImage(new Uri(url));
                    return;
                }
            }
            var urls = await new GoogleStreetviewConnector().GetURLToSavePicture(_assignment.Target[0]);
            if (urls.Count > 0)
                AssignmentImage.Source = new BitmapImage(new Uri(urls.ElementAt(0)));
            
            IsPrimaryButtonEnabled = true;
            IsSecondaryButtonEnabled = true;
        }
       
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
