using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class MultiplayerAssignments : ContentDialog
    {
        private MultiplayerData _data;
        public MultiplayerAssignmentDetails selected;
        public MultiplayerAssignments(MultiplayerData data)
        {
            _data = data;
            this.InitializeComponent();
        }

       
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private async void OnJoinClicked(object sender, RoutedEventArgs e)
        {
            var assignment = ((Button) sender).DataContext as MultiplayerAssignmentDetails;
            if (assignment != null)
            {  
                // todo check if not already joined!
                
                assignment.Participants.Add(_data.User.id);
                assignment.OnPropertyChanged("Available");
                selected = assignment;
                await _data.Db.UpdateMultiplayerAssignmentDetail(assignment);
                Hide();
            }
        }

        private void MultiplayerAssignments_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // TODO CHECK IF NOT ALREADY JOINED!
            var assignment = new MultiplayerAssignmentDetails(4, null, _data.User.id);
            _data.Db.UploadMultiplayerAssignmentDetail(assignment);
            _data.MultiplayerAssignmentDetailses.Add(assignment);
            selected = assignment;
        }
    }
}
