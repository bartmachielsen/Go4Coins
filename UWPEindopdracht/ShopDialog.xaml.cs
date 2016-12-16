﻿using System;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPEindopdracht
{
    public sealed partial class ShopDialog : ContentDialog
    {
        private int _normalIndex = 1;
        private int _rareIndex = 1;
        private int _largeIndex = 1;
        private const int _normalPrice = 2000;
        private const int _rarePrice = 3000;
        private const int _largePrice = 4000;

        public ShopDialog()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            _normalIndex = 1;
            _rareIndex = 1;
            _largeIndex = 1;
        }

        private void BuyNormalButton_Click(object sender, RoutedEventArgs e)
        {
            Notificate(NormalChestBuyNotification, FadeAnimationNormal, _normalIndex, _normalPrice);
            if (int.Parse(PointsText.Text) >= _normalPrice)
            {
                _normalIndex++;
            }
        }

        private void BuyRareButton_Click(object sender, RoutedEventArgs e)
        {
            Notificate(RareChestBuyNotification, FadeAnimationRare, _rareIndex, _rarePrice);
            if (int.Parse(PointsText.Text) >= _rarePrice)
            {
                _rareIndex++;
            }
        }

        private void BuyLargeButton_Click(object sender, RoutedEventArgs e)
        {
            Notificate(LargeChestBuyNotification, FadeAnimationLarge, _largeIndex, _largePrice);
            if (int.Parse(PointsText.Text) >= _largePrice)
            {
                _largeIndex++;
            }
        }

        private void Notificate(TextBlock notificationText, Storyboard fadeAnimation, int index, int price)
        {
            if (int.Parse(PointsText.Text) < price)
            {
                notificationText.Text = "Can't buy";
                fadeAnimation.Begin();
            }
            else
            {
                PointsText.Text = (int.Parse(PointsText.Text) - price).ToString();
                notificationText.Text = $"Bought x{index}";
                fadeAnimation.Begin();
            }
        }
    }
}
