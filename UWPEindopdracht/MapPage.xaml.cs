﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using UWPEindopdracht.Assignments;
using UWPEindopdracht.DataConnections;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.JSON;
using UWPEindopdracht.Multiplayer;
using UWPEindopdracht.Places;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPEindopdracht
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        private bool _dialogClaimed = false;
        private Assignment _assignment;
        private readonly MultiplayerData _multiplayerData = new MultiplayerData();
        private readonly Random _random = new Random();
        private bool updateAssignments = false;
        private bool assignmentsLoaded = false;

        public MapPage()
        {
            GPSHelper.ClearGeofences();
            LoadMultiplayerDetails();
            
            InitializeComponent();
            
            MapControl.MapElementClick += MapControl_MapElementClick;

            GPSHelper.NotifyOnLocationUpdate(LocationChanged);
            
            SetLocation();
            MapControl.ZoomInteractionMode = MapInteractionMode.GestureAndControl;
            MapControl.ZoomLevel = 16;
        }
        
        private async void LoadMultiplayerDetails()
        {
            try
            {
                await _multiplayerData.RegisterMultiplayerUser();
            }
            catch (NoInternetException)
            {
                InternetException();
            }
            try
            {
                await _multiplayerData.LoadRewards();
            }
            catch (NoInternetException)
            {
                InternetException();
            }
            LiveUpdateUser();
        }

        private async void LiveUpdateUser()
        {
            while (true)
            {
                await Task.Delay(MultiplayerData.ServerTimeOut*1000);
                await UpdateUserDetails();
                await CheckIfLocationUpdate(null);
                if (updateAssignments)
                {
                    await _multiplayerData.UpdateAssignments();
                    assignmentsLoaded = true;
                }
                var details = _assignment as MultiplayerAssignmentDetails;
                if (details != null)
                {
                    if (details.syncNeeded)
                        await _multiplayerData.Db.UpdateMultiplayerAssignmentDetail(details);
                    var existed = details.Targets == null;
                    try
                    {
                        await _multiplayerData.Db.GetMultiplayerAssignment(details);
                    }
                    catch (NoInternetException)
                    {
                        InternetException();
                    }
                    if (existed && details.Targets != null)
                        SetAssignment(await GPSHelper.getLocation(), details);
                    
                }
            }
        }

        public static async void InternetException()
        {
            if (!MultiplayerData.NoInternetConfirmed)
            {
                await new MessageDialog("No internet connection found", "Internet connection error").ShowAsync();
                MultiplayerData.NoInternetConfirmed = true;
            }
        }
        private async Task UpdateUserDetails()
        {
            try
            {
                _multiplayerData.Users = await _multiplayerData.Db.GetUsers(_multiplayerData.Users);
                MultiplayerData.NoInternetConfirmed = false;
            }
            catch (NoInternetException)
            {
                InternetException();
                return;
            }
            catch (NoResponseException)
            {
                Debug.WriteLine("Got no response from database, but we'll continue because shit happens");
            }

            if (_multiplayerData.Users == null) return;
            foreach (var user in _multiplayerData.Users)
                if (user.id != _multiplayerData.User.id)
                {
                    var geopoint = GPSHelper.getPointOutLocation(user.Location);
                    if (!user.IsAlive())
                    {
                        if (user.Icon == null) continue;
                        MapControl.MapElements.Remove(user.Icon);
                        user.Icon = null;
                    }
                    else
                    {
                        if (user.Duel != null && !_multiplayerData.User.AcceptedDuels.Contains(user.Duel.MultiplayerID))
                        {
                            _multiplayerData.User.AcceptedDuels.Add(user.Duel.MultiplayerID);
                            ShowDual(user.Duel, user);
                        }

                        System.Diagnostics.Debug.WriteLine($"{user.id} is loaded! {DateTime.Now-user.LastSynced}");
                        if (user.Icon == null)
                        {
                            user.Icon = new MapIcon
                            {
                                Location = geopoint,
                                Title = user.Name,
                                Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/MapPage/stickman.png"))

                        };
                            MapControl.MapElements.Add(user.Icon);
                        }
                        else
                        {
                            user.Icon.Title = user.Name;
                            user.Icon.Location = geopoint;
                        }
                    }
                }
        }

        private async Task<bool> ShowUserDetails(User user, bool newUser = true, bool self = false)
        {
            ContentDialog dialog = new UserDialog(user,newUser);

            if(self)
                dialog = new MultiplayerSettings(user);
            await ShowDialog(dialog);
            var userDialog = dialog as UserDialog;
            return userDialog != null && userDialog.Dueled;
        }

        private async void ShowDual(DuelRequest request, User requestee)
        {
            MultiplayerAssignmentDetails assignment = new MultiplayerAssignmentDetails(-1, request.MultiplayerID, requestee.id);
            assignment.CurrentUser = _multiplayerData.User.id;
            await new RestDBConnector().GetMultiplayerAssignment(assignment);

            assignment.dual = true;

            if(!assignment.Closed)
                await SetAssignment(await GPSHelper.getLocation(), assignment, true);
        }

        private async Task ShowDialog(ContentDialog dialog)
        {
            while (_dialogClaimed) { }
            if (!_dialogClaimed)
            {
                _dialogClaimed = true;
                await dialog.ShowAsync();
                _dialogClaimed = false;
            }
        }
       
        private async void MapControl_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            foreach (var user in _multiplayerData.Users)
            {
                if (user.Icon == null) continue;
                if (args.MapElements.All(element => element != user.Icon)) continue;
                DuelMultiplayerUser(ShowUserDetails(user, false), user);
                return;
            }
            if(_assignment != null)
                foreach (var target in _assignment.Targets)
                    foreach (var element in args.MapElements)
                        if (target.Icon != null && target.Icon == element)
                            ShowDialog(new PlaceDialog(target));
                    
                
            if(args.MapElements.All(element => element == _multiplayerData.User.Icon))
                await ShowUserDetails(_multiplayerData.User, false, true);
        }

        private async void DuelMultiplayerUser(Task<bool> dual, User user)
        {
            bool dualed = await dual;
            System.Diagnostics.Debug.WriteLine(dualed + " DUALED");
            if (dualed)
            {
                MultiplayerAssignmentDetails multiplayerAssignment = new MultiplayerAssignmentDetails(2, null,
                    _multiplayerData.User.id);
                multiplayerAssignment.dual = true;
                multiplayerAssignment.Participants.Add(user.id);
                multiplayerAssignment.CurrentUser = _multiplayerData.User.id;
                await new RestDBConnector().UploadMultiplayerAssignmentDetail(multiplayerAssignment);
                await SetAssignment((await GPSHelper.getLocation()), multiplayerAssignment, true);


                _multiplayerData.User.Duel = new DuelRequest()
                {
                    MultiplayerID = multiplayerAssignment.Id, UserID = user.id
                };

                
            }
        }
        

        private async void RemovePinPoints(Assignment oldAssignment)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var place in oldAssignment.Targets)
                {
                    MapControl.MapElements.Remove(place.Icon);
                    place.Icon = null;
                }
            });
            
        }

        private void PlacePinPoints(Geopoint location)
        {
            if ((_assignment?.Targets != null) && _assignment.ShowPinPoint)
            {
                foreach (var target in _assignment.Targets)
                {
                    if (target.Icon != null) continue;
                    target.Icon = new MapIcon()
                    {
                        Title = target.Name,
                        Location = GPSHelper.getPointOutLocation(target.Location)
                    };
                        
                    if (!string.IsNullOrEmpty(target.IconLink))
                        target.Icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri(target.IconLink));
                    MapControl.MapElements.Add(target.Icon);
                }
            }
            if (location == null) return;
            if (_multiplayerData.User == null) return;
            if (_multiplayerData.User.Icon == null)
            {
                _multiplayerData.User.Icon = new MapIcon { Title = "Your Location", 
                    Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/MapPage/stickman.png"))
                    
                };
                
                MapControl.MapElements.Add(_multiplayerData.User.Icon);
            }
            _multiplayerData.User.Icon.Location = location;
        }

        private void ShowLoading()
        {
            LoadingAnimation.Visibility = Visibility.Visible;
            LoadingText.Visibility = Visibility.Visible;
            NewAssignmentButton.IsEnabled = false;
            GoToAlbumButton.IsEnabled = false;
            GoToShopButton.IsEnabled = false;
            OnTargetButton.IsEnabled = false;
            MapControl.PanInteractionMode = MapPanInteractionMode.Disabled;
            MapControl.ZoomInteractionMode = MapInteractionMode.Disabled;
            MapControl.RotateInteractionMode = MapInteractionMode.Disabled;
            MapControl.TiltInteractionMode = MapInteractionMode.Disabled;
        }

        private void HideLoading()
        {
            LoadingAnimation.Visibility = Visibility.Collapsed;
            LoadingText.Visibility = Visibility.Collapsed;
            NewAssignmentButton.IsEnabled = true;
            GoToAlbumButton.IsEnabled = true;
            GoToShopButton.IsEnabled = true;
            OnTargetButton.IsEnabled = true;
            MapControl.PanInteractionMode = MapPanInteractionMode.Auto;
            MapControl.ZoomInteractionMode = MapInteractionMode.Auto;
            MapControl.RotateInteractionMode = MapInteractionMode.Auto;
            MapControl.TiltInteractionMode = MapInteractionMode.Auto;
        }
        private async Task SetAssignment(GCoordinate loc, Assignment newAssignment, bool selectNew = false)
        {
            if (_assignment != null)
            {
                RemovePinPoints(_assignment);
                _assignment = null;
                DistanceTextBlock.Text = "0 km";
                TimeTextBlock.Text = "00:00";
            }
            var tempSave = _assignment;
            _assignment = null;
            if (loc == null || newAssignment == null)
                return;
            ShowLoading();
            if (newAssignment.LoadPlaces())
                await _multiplayerData.placeLoader.LoadPlaces(loc);

            try
            {
                await newAssignment.FillTarget(_multiplayerData.placeLoader.Places, loc);
            }
            catch (NoTargetAvailable)
            {
                await new MessageDialog("Move to another area! (move +5KM)", "Not enough targets!").ShowAsync();
                _assignment = null;
                HideLoading();
                return;
            }
            catch (CantCalculateRouteException)
            {
                await new MessageDialog("Cant find route to target(s)", "please move to walking area!").ShowAsync();
                _assignment = null;
                HideLoading();
                return;
            }
            bool cont = false;
            if (newAssignment.Targets != null)
            {
                var dialog = new AssignmentDialog(newAssignment, await ImageLoader.GetBestUrlFromPlace(newAssignment));
                await ShowDialog(dialog);
                cont = dialog.Accepted;
            }
            if (newAssignment is MultiplayerAssignmentDetails)
            {
                cont = true;
            }
            if (!cont)
            {
                if (selectNew)
                {
                    newAssignment = GetRandomAssignment();
                }
                if (_multiplayerData.User.Coins > 500)
                    _multiplayerData.User.Coins -= 500;
                else
                {
                    _multiplayerData.User.Coins = 0;
                }
                await SetAssignment(loc, newAssignment, selectNew);
            }
            else
            {
                newAssignment.StartAssignment();
                _assignment = newAssignment;
                PlacePinPoints(GPSHelper.getPointOutLocation(loc));
                HideLoading();
                ChangeDistance(loc);
                ChangeTime();
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                timer.Tick += delegate
                {
                    if (_assignment != null)
                        ChangeTime();
                    else
                        timer.Stop();
                };
                timer.Start();
            }
        }

        private async void SetLocation()
        {
            var loc = await GPSHelper.getLocationOriginal();
            if (loc != null)
            {
                MapControl.Center = loc.Coordinate.Point;
                HideLoading();
            }
            else
            {
                await new MessageDialog("No GPS connection!", "Can't get your location").ShowAsync();
            }
        }

        private async void ChangeDistance(GCoordinate current)
        {
            if (_assignment != null)
                DistanceTextBlock.Text = await _assignment.GetDistanceText(current);
        }

        private async void ChangeTime()
        {
            if (_assignment != null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (_assignment == null) return;
                    var time = _assignment.GetTimeText();
                    TimeTextBlock.Text = time ?? "00:00";
                });
            }
        }

        private async Task CheckIfLocationUpdate(Geopoint point)
        {
            if (DateTime.Now - _multiplayerData.LastLocationSync <= TimeSpan.FromSeconds(MultiplayerData.ServerTimeOut)) return;
            if (point == null)
                point = (await GPSHelper.getLocationOriginal()).Coordinate.Point;
            await _multiplayerData.UpdateMultiplayerServer(GPSHelper.GetGcoordinate(point));
        }

        private async Task LocationChanged(GCoordinate coordinate)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var location = GPSHelper.getPointOutLocation(coordinate);
                if (location == null) return;
                PlacePinPoints(location);
                ChangeDistance(GPSHelper.GetGcoordinate(location));
                try
                {
                    _assignment?.LocationChanged(coordinate);
                }
                catch (SpeedException e)
                {
                    switch (e.Warning)
                    {
                        case SpeedException.WarningLevel.Warning:
                            await new MessageDialog("TO FAST!", "NOT SO FAST").ShowAsync();
                            break;
                        case SpeedException.WarningLevel.Critical:
                            // TODO CHECK WHAT KIND OF ASSIGNMENT
                            await SetAssignment(coordinate, new MapAssignment());
                            break;
                    }
                }
            });
        }
        
        private async void OnTargetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_assignment == null)
            {
                OnTargetText.Text = "No assignment picked!";
                OnTargetText.Foreground = new SolidColorBrush(Colors.Red);
                OnTargetText.Opacity = 1.0;
                OnTargetErrorAnimation.Begin();
                return;
            }
            if (_assignment.CurrentLocation == null)
            {
                OnTargetText.Text = "Not close enough!";
                OnTargetText.Foreground = new SolidColorBrush(Colors.Red);
                OnTargetText.Opacity = 1.0;
                OnTargetErrorAnimation.Begin();
            }
            else if (!_assignment.RegisterTarget(_assignment.CurrentLocation))
            {
                OnTargetText.Text = "Already reached!";
                OnTargetText.Foreground = new SolidColorBrush(Colors.Red);
                OnTargetText.Opacity = 1.0;
                OnTargetErrorAnimation.Begin();
            }
            else
            {
                OnTargetText.Text = "Reached!";
                OnTargetText.Foreground = new SolidColorBrush(Colors.Green);
                OnTargetText.Opacity = 1.0;
                OnTargetErrorAnimation.Begin();

                if (_assignment.AssignmentFinished())
                {
                    //TODO ADD SCORE TO USER!
                    _multiplayerData.User.Coins += _assignment.TotalScore(_assignment.GetSpentTime());
                    await SetAssignment(null, null);
                }
            }
        }

        private void NewAssignmentButton_Click(object sender, RoutedEventArgs e)
        {
            var stack = new StackPanel();
            var style = new Style(typeof(FlyoutPresenter));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Colors.Transparent)));
            style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));
            NewAssignmentButton.Flyout = new Flyout() { Content = stack };
            NewAssignmentButton.Flyout.SetValue(Flyout.FlyoutPresenterStyleProperty, style);
            if (_assignment != null)
            {
                var current = new Button() { Content = "Currently playing", Style = (Style)Application.Current.Resources["FooterButtonStyle"], HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 0, 10) };
                stack.Children.Add(current);
                current.Click += async (o, args) =>
                {
                    NewAssignmentButton.Flyout.Hide();
                    var dialog = new AssignmentDialog(_assignment, await ImageLoader.GetBestUrlFromPlace(_assignment), true);
                    await ShowDialog(dialog);
                    if (dialog.Accepted) return;
                    if (_assignment is MultiplayerAssignmentDetails)
                    {
                        var multi = _assignment as MultiplayerAssignmentDetails;
                        multi.Participants.RemoveAll(s => s == _multiplayerData.User.id);
                        if (multi.Participants.Count == 0 || multi.Administrator == _multiplayerData.User.id)
                        {
                            multi.Closed = true;
                        }
                        new RestDBConnector().UpdateMultiplayerAssignmentDetail(multi);
                        SetAssignment(null, null);
                    }
                    if(_multiplayerData.User.Coins > 500)
                        _multiplayerData.User.Coins -= 500;
                    else
                    {
                        _multiplayerData.User.Coins = 0;
                    }
                    await SetAssignment(null, null);
                };

            }
            var multiplayer = new Button() { Content = "Multiplayer", Style = (Style)Application.Current.Resources["FooterButtonStyle"], HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 0, 10) };
            var singleplayer = new Button() { Content = "Singleplayer", Style = (Style)Application.Current.Resources["FooterButtonStyle"], HorizontalAlignment = HorizontalAlignment.Stretch };
            stack.Children.Add(multiplayer);
            stack.Children.Add(singleplayer);
            singleplayer.Click += async (o, args) =>
            {
                if (_assignment != null)
                {
                    TakeAssignmentErrorAnimation.Begin();
                    return;
                }
                NewAssignmentButton.Flyout.Hide();
                var loc = await GPSHelper.getLocation();
                if (loc != null)
                {
                    await SetAssignment(loc, GetRandomAssignment(), true);
                }
            };

            multiplayer.Click += async (o, args) =>
            {
                if (_assignment != null)
                {
                    TakeAssignmentErrorAnimation.Begin();
                    return;
                }
                NewAssignmentButton.Flyout.Hide();
                var loc = await GPSHelper.getLocation();
                if (loc != null)
                {
                    
                    var dialog = new MultiplayerAssignments(_multiplayerData);
                    await _multiplayerData.UpdateAssignments();
                    updateAssignments = true;
                    await ShowDialog(dialog);
                    updateAssignments = false;
                    if (dialog.selected != null)
                    {
                        dialog.selected.CurrentUser = _multiplayerData.User.id;
                        await SetAssignment(loc, dialog.selected);
                    }
                }
            };
        }

        private Assignment GetRandomAssignment()
        {
            List<Assignment> assignments = new List<Assignment>() {new MapAssignment(), new SearchAssignment(), new AssistedAssignment()};
            var index = _random.Next(assignments.Count);
            System.Diagnostics.Debug.WriteLine(index);
            return assignments.ElementAt(index);
        }
        private async void GoToAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            var a = new AlbumDialog(_multiplayerData.User, _multiplayerData.Rewards);
            await a.ShowAsync();
        }

        private async void GoToShopButton_Click(object sender, RoutedEventArgs e)
        {
            var s = new ShopDialog(_multiplayerData.User);
            await s.ShowAsync();
        }
    }
}