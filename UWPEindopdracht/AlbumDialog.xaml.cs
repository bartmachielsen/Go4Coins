using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
using UWPEindopdracht.Multiplayer;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPEindopdracht
{
    public sealed partial class AlbumDialog : ContentDialog
    {
        //private ObservableCollection<BitmapImage> _images = new ObservableCollection<BitmapImage>();
        private ObservableCollection<Reward> _won = new ObservableCollection<Reward>();
        
        private Thickness _size = new Thickness(2);
        private User _user;
        private List<Reward> _rewards;
        private ObservableCollection<Reward> _selectedList = new ObservableCollection<Reward>();
        private string _focus = "Marvel";
        private List<RewardValue> _values = new List<RewardValue>();
        private int _value = 1;
        private Reward _curReward;

        public AlbumDialog(User user, List<Reward> rewards)
        {
            foreach (RewardValue value in Enum.GetValues(typeof(RewardValue)))
                _values.Add(value);
            

            _rewards = rewards;
            _user = user;
            UpdateAllLists();

            this.InitializeComponent();
            RefreshChests();
        }

        private void UpdateAllLists()
        {
            _selectedList.Clear();

            foreach (var reward in _rewards)
            {
                if (reward.Categorie != _focus || !_values.Contains(reward.Value)) continue;
                reward.InInventory = _user.Rewards.FindAll((s => s == reward.Name)).Count;
                _selectedList.Add(reward);
            }

            // unlockedLabel.Text = ""+ _rewards.FindAll((reward => _user.Rewards.Contains(reward.Name))).Count;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void OpenNormalChest_Click(object sender, RoutedEventArgs e)
        {
            if (!_user.Chests.Contains(typeof(BasicChest).Name)) return;
            _user.Chests.Remove(typeof(BasicChest).Name);
            var rewardsWon = new BasicChest().GetRewards(_rewards);
            _won.Clear();
            foreach (var reward in rewardsWon)
            {
                _user.Rewards.Add(reward.Name);
                _won.Add(reward);
            }
            RefreshChests();
            ShowReward();
        }

        private void OpenRareChest_Click(object sender, RoutedEventArgs e)
        {
            if (!_user.Chests.Contains(typeof(AdvancedChest).Name)) return;
            _user.Chests.Remove(typeof(AdvancedChest).Name);
            var rewardsWon = new AdvancedChest().GetRewards(_rewards);
            _won.Clear();
            foreach (var reward in rewardsWon)
            {
                _user.Rewards.Add(reward.Name);
                _won.Add(reward);
            }
            
            
            RefreshChests();
            ShowReward();
        }

        private void ToggleButton(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(sender.GetType());
            Button button = (Button)sender;
            if (button.Name == "All")
            {
                var selecttext = "Selected";
                if(_values.Count != Enum.GetValues(typeof(RewardValue)).Length) { 
                    _values.Clear();
                    selecttext = "";
                }
                ToggleButton(Legendary, null);
                ToggleButton(Epic, null);
                ToggleButton(Rare, null);
                ToggleButton(Normal, null);
                UpdateAllLists();

                Image image = (Image) button.Content;
                image.Source = new BitmapImage(new Uri(this.BaseUri, $"/Assets/ShopAndAlbum/{button.Name}{selecttext}.png"));
                button.Padding = new Thickness(0, 0, 0, 0);
                button.Style = (Style)Application.Current.Resources["RarityButtonStyle"];
            }
            else
            {
                RewardValue val = (RewardValue) Enum.Parse(typeof(RewardValue), button.Name);
                if (_values.Contains(val))
                    _values.Remove(val);
                else
                {
                    _values.Add(val);
                }
                var select = "";
                if (_values.Contains(val))
                    select = "Selected";
                var image = (Image)button.Content;
                if (image != null)
                    image.Source = new BitmapImage(new Uri(this.BaseUri, $"/Assets/ShopAndAlbum/{button.Name}{@select}.png"));
                button.Padding = new Thickness(0, 0, 0, 0);
                button.Style = (Style) Application.Current.Resources["RarityButtonStyle"];
            }
            if(e != null)
                UpdateAllLists();
        }

        private void OpenLargeChest_Click(object sender, RoutedEventArgs e)
        {
            if (!_user.Chests.Contains(typeof(LargeChest).Name)) return;
            _user.Chests.Remove(typeof(LargeChest).Name);
            var rewardsWon = new LargeChest().GetRewards(_rewards);
            _won.Clear();
            foreach (var reward in rewardsWon)
            {
                _user.Rewards.Add(reward.Name);
                _won.Add(reward);
            }
            RefreshChests();
            ShowReward();
        }

        public void ShowReward()
        {
            RewardGrid.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Collapsed;
            HeaderText.Visibility = Visibility.Collapsed;
            ChestGrid.Visibility = Visibility.Collapsed;
            UpdateAllLists();
        }

        public void RefreshChests()
        {
            var chests = _user.getChests();
            NormalChestAmountText.Text = $"x{chests.FindAll(chest => chest is BasicChest).Count}";
            RareChestAmountText.Text = $"x{chests.FindAll(chest => chest is AdvancedChest).Count}";
            LargeChestAmountText.Text = $"x{chests.FindAll(chest => chest is LargeChest).Count}";
            AvailableChestsText.Text = $"x{chests.Count}";
        }

        private void MenuButton_Clicked(object sender, RoutedEventArgs e)
        {
            ChestRow.Height = ChestRow.Height == new GridLength(2, GridUnitType.Star) ? new GridLength(0) : new GridLength(2, GridUnitType.Star);
        }

        private void ShowInformation(object sender, TappedRoutedEventArgs e)
        {
            var selected = (Reward)((Image) sender).DataContext;
            _curReward = selected;
            NumericText.Text = "1";
            if (selected.InInventory < 1) NumericText.Text = "0";
            SelectedNameBox.Text = selected.NiceName;
            SelectedImageBox.Source = new BitmapImage(new Uri(this.BaseUri, "/" + selected.Image));
            SelectedImageBorder.BorderBrush = selected.RareColor;
            SelectedImageBox.DataContext = selected;
            SellButton.DataContext = selected;
            SellButton.Visibility = selected.InInventory > 0 ? Visibility.Visible : Visibility.Collapsed;
            // TODO ADD DATACONTEXT TO SELL BUTTON @NARD
            SelectedDescriptionBox.Text = selected.Description;
            InformationGrid.Visibility = Visibility.Visible;
            CollectionsGrid.Visibility = Visibility.Collapsed;
        }

        private void Sell_click(object sender, RoutedEventArgs e)
        {
            var removeList = new List<Reward>();
            var datacontext = (Reward)((Button)sender).DataContext;
            if (!_user.Rewards.Contains(datacontext.Name))
                return;
            removeList.Add(datacontext);
            if (removeList.Count == int.Parse(NumericText.Text))
            {
                foreach(var r in removeList)
                {
                    _user.Rewards.Remove(r.Name);
                    _user.Coins += r.CoinValue;
                    r.InInventory -= 1;
                }
            }
            InformationGrid.Visibility = Visibility.Collapsed;
            CollectionsGrid.Visibility = Visibility.Visible;
            UpdateAllLists();
        }

        private void Category1_Click(object sender, RoutedEventArgs e)
        {
            if (Category1.BorderThickness != _size)
            {
                ClearAll();
                Category1.BorderThickness = _size;
                _focus = "Marvel";
                UpdateAllLists();
            }
        }

        private void Category2_Click(object sender, RoutedEventArgs e)
        {
            if (Category2.BorderThickness != _size)
            {
                ClearAll();
                Category2.BorderThickness = _size;
                _focus = "Dc";
                UpdateAllLists();
            }
        }

        private void Category3_Click(object sender, RoutedEventArgs e)
        {
            if (Category3.BorderThickness != _size)
            {
                ClearAll();
                Category3.BorderThickness = _size;
                _focus = "Disney";
                UpdateAllLists();
            }
        }

        private void Category4_Click(object sender, RoutedEventArgs e)
        {
            if (Category4.BorderThickness != _size)
            {
                ClearAll();
                Category4.BorderThickness = _size;
                _focus = "WarnerBros";
                UpdateAllLists();
            }
        }

        private void Category5_Click(object sender, RoutedEventArgs e)
        {
            if (Category5.BorderThickness != _size)
            {
                ClearAll();
                Category5.BorderThickness = _size;
                _focus = "DreamWorks";
                UpdateAllLists();
            }
        }

        private void ClearAll()
        {
            var size = new Thickness(0);
            Category1.BorderThickness = size;
            Category2.BorderThickness = size;
            Category3.BorderThickness = size;
            Category4.BorderThickness = size;
            Category5.BorderThickness = size;
            
        }

        private void BackToAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            RewardGrid.Visibility = Visibility.Collapsed;
            HeaderText.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Visible;
            ChestGrid.Visibility = Visibility.Visible;
        }

        private void BackToAlbumButton2_Click(object sender, RoutedEventArgs e)
        {
            InformationGrid.Visibility = Visibility.Collapsed;
            CollectionsGrid.Visibility = Visibility.Visible;
        }

        public int NumValue
        {
            get { return _value; }
            set
            {
                _value = value;
                NumericText.Text = value.ToString();
            }
        }

        private void NumericText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NumericText == null)
            {
                return;
            }

            if (!int.TryParse(NumericText.Text, out _value))
            {
                NumericText.Text = _value.ToString();
            }

            if (int.Parse(NumericText.Text) > _curReward.InInventory ||
                int.Parse(NumericText.Text) < 0)
            {
                NumericText.Text = _value.ToString();
            }
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            if(NumValue < _curReward.InInventory)
                NumValue++;
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            if(NumValue > 0)
                NumValue--;
        }
    }
}
