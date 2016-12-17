using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using UWPEindopdracht.DataConnections;
using UWPEindopdracht.Places;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPEindopdracht
{
    public sealed partial class PlaceDialog : ContentDialog
    {
        private Place place;
        public PlaceDialog(Place place)
        {
            this.place = place;
            ImageUrl();
            this.InitializeComponent();
            LoadDetails();
            
        }

        private async void ImageUrl()
        {
            string url = await ImageLoader.GetBestUrlFromPlace(place);
            if(url != null)
                PlaceImage.Source = new BitmapImage(new Uri(url));
        }

        private void LoadDetails()
        {
            Title = place.Name;
            if(place.Types != null)
                TypeBlock.Text = $"[{string.Join(", ", place.Types)}]";
            Distance.Text = "";

        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void NextImage(object sender, TappedRoutedEventArgs e)
        {
            // TODO GO TO NEXT PICTURE, LOAD MORE PICTURES THEN ONE
        }
    }
}
