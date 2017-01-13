using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        ObservableCollection<BitmapImage> _images = new ObservableCollection<BitmapImage>();
        ObservableCollection<Reward> won = new ObservableCollection<Reward>();
        
            Thickness _size = new Thickness(2);
        private User _user;
        private List<Reward> _rewards;
        ObservableCollection<Reward> selectedList = new ObservableCollection<Reward>();
        private string focus = "Marvel";
        private List<RewardValue> values = new List<RewardValue>();
        


        public AlbumDialog(User user, List<Reward> rewards)
        {
            foreach (RewardValue value in Enum.GetValues(typeof(RewardValue)))
                values.Add(value);
            

            _rewards = rewards;
            _user = user;
            updateAllLists();

            this.InitializeComponent();
            RefreshChests();
        }

        private void updateAllLists()
        {
            selectedList.Clear();

            foreach (var reward in _rewards)
            {
                if (reward.Categorie != focus || !values.Contains(reward.Value)) continue;
                reward.inInventory = _user.Rewards.FindAll((s => s == reward.Name)).Count;
                selectedList.Add(reward);
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
            won.Clear();
            foreach (var reward in rewardsWon)
            {
                _user.Rewards.Add(reward.Name);
                won.Add(reward);
            }
            RefreshChests();
            ShowReward();
        }

        private void OpenRareChest_Click(object sender, RoutedEventArgs e)
        {
            if (!_user.Chests.Contains(typeof(AdvancedChest).Name)) return;
            _user.Chests.Remove(typeof(AdvancedChest).Name);
            var rewardsWon = new AdvancedChest().GetRewards(_rewards);
            won.Clear();
            foreach (var reward in rewardsWon)
            {
                _user.Rewards.Add(reward.Name);
                won.Add(reward);
            }
            
            
            RefreshChests();
            ShowReward();
        }

        private void OpenLargeChest_Click(object sender, RoutedEventArgs e)
        {
            if (!_user.Chests.Contains(typeof(LargeChest).Name)) return;
            _user.Chests.Remove(typeof(LargeChest).Name);
            var rewardsWon = new LargeChest().GetRewards(_rewards);
            won.Clear();
            foreach (var reward in rewardsWon)
            {
                _user.Rewards.Add(reward.Name);
                won.Add(reward);
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
            updateAllLists();
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
            SelectedNameBox.Text = selected.niceName;
            SelectedImageBox.Source = new BitmapImage(new Uri(this.BaseUri, "/" + selected.Image));
            SelectedImageBorder.BorderBrush = selected.rareColor;
            SelectedImageBox.DataContext = selected;
            SellButton.DataContext = selected;
            SellButton.Visibility = selected.inInventory > 0 ? Visibility.Visible : Visibility.Collapsed;
            // TODO ADD DATACONTEXT TO SELL BUTTON @NARD
            SelectedDescriptionBox.Text = selected.Description;
            InformationGrid.Visibility = Visibility.Visible;
            CollectionsGrid.Visibility = Visibility.Collapsed;

        }

        private void Category1_Click(object sender, RoutedEventArgs e)
        {
            if (Category1.BorderThickness != _size)
            {
                Category1.BorderThickness = _size;
                focus = "Marvel";
                updateAllLists();
            }
        }

        private void sell_click(object sender, RoutedEventArgs e)
        {
            var datacontext = (Reward)((Button)sender).DataContext;
            if (!_user.Rewards.Contains(datacontext.Name))
                return;
            _user.Rewards.Remove(datacontext.Name);
            _user.Coins += datacontext.coinValue;
            datacontext.inInventory -= 1;

            InformationGrid.Visibility = Visibility.Collapsed;
            CollectionsGrid.Visibility = Visibility.Visible;
            updateAllLists();

        }
        
        private void Category2_Click(object sender, RoutedEventArgs e)
        {
            if (Category2.BorderThickness != _size)
            {
                ClearAll();
                Category2.BorderThickness = _size;
                focus = "Dc";
                updateAllLists();
            }
        }

        private void Category3_Click(object sender, RoutedEventArgs e)
        {
            if (Category3.BorderThickness != _size)
            {
                ClearAll();
                Category3.BorderThickness = _size;
                focus = "Disney";
                updateAllLists();
            }
        }

        private void Category4_Click(object sender, RoutedEventArgs e)
        {
            if (Category4.BorderThickness != _size)
            {
                ClearAll();
                Category4.BorderThickness = _size;
                focus = "WarnerBros";
                updateAllLists();
            }
        }

        private void Category5_Click(object sender, RoutedEventArgs e)
        {
            if (Category5.BorderThickness != _size)
            {
                ClearAll();
                Category5.BorderThickness = _size;
                focus = "DreamWorks";
                updateAllLists();
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

        private void List1_Click(object sender, RoutedEventArgs e)
        {
            InformationGrid.Visibility = Visibility.Visible;
            CollectionsGrid.Visibility = Visibility.Collapsed;
        }

        private void List2_Click(object sender, RoutedEventArgs e)
        {
            InformationGrid.Visibility = Visibility.Visible;
            CollectionsGrid.Visibility = Visibility.Collapsed;
        }

        private void List3_Click(object sender, RoutedEventArgs e)
        {
            InformationGrid.Visibility = Visibility.Visible;
            CollectionsGrid.Visibility = Visibility.Collapsed;
        }

        private void List4_Click(object sender, RoutedEventArgs e)
        {
            InformationGrid.Visibility = Visibility.Visible;
            CollectionsGrid.Visibility = Visibility.Collapsed;
        }

        private void List5_Click(object sender, RoutedEventArgs e)
        {
            InformationGrid.Visibility = Visibility.Visible;
            CollectionsGrid.Visibility = Visibility.Collapsed;
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
    }
}
