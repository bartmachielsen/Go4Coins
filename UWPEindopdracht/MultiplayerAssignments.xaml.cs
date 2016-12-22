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
        public ObservableCollection<MultiplayerAssignmentDetails> Assignments = new ObservableCollection<MultiplayerAssignmentDetails>();
        private MultiplayerData _data;
        private DispatcherTimer _timer;
        public MultiplayerAssignments(MultiplayerData data)
        {
            _data = data;
            
            this.InitializeComponent();

            UpdateAssignments(null,null);
            _timer = new DispatcherTimer() {Interval= TimeSpan.FromSeconds(3)};
            _timer.Tick += UpdateAssignments;
            _timer.Start();
        }

        private async void UpdateAssignments(object sender, object tick)
        {
            List<MultiplayerAssignmentDetails> assignmentDetailses = await _data.Db.GetMultiplayerAssignments();
            
            foreach (var assignment in assignmentDetailses)
            {
                var exists = false;
                foreach (var existing in Assignments)
                {
                    if (assignment.Id != existing.Id) continue;
                    exists = true;
                    existing.Merge(assignment);
                    break;
                }
                var users = _data.Users.FindAll((user => assignment.Participants.Contains(user.id)));
                if (!exists)
                {
                    Assignments.Add(assignment);
                }
            }
            var removal = new List<MultiplayerAssignmentDetails>();
            foreach (var assigment in Assignments)
            {
                var users = _data.Users.FindAll((user => assigment.Participants.Contains(user.id) || user.id == assigment.Administrator));
                if (assigment.Participants.Contains(_data.User.id))
                    users.Add(_data.User);
                
                if (!users.Exists((user => user.IsAlive())) || !assignmentDetailses.Exists((details => assigment.Id == details.Id)))
                {
                    removal.Add(assigment);
                }
            }
            foreach (var remover in removal)
            {
                Assignments.Remove(remover);
            }

        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void OnJoinClicked(object sender, RoutedEventArgs e)
        {
            var assignment = ((Button) sender).DataContext as MultiplayerAssignmentDetails;
            if (assignment != null)
            {  
                assignment.Joiners.Add(_data.User.id);
                assignment.OnPropertyChanged("Available");
                _timer.Stop();
                Hide();
            }
        }
    }
}
