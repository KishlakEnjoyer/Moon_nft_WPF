using Moon_nft_api.DTOs;
using Moon_nft_application.Models;
using Moon_nft_application.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Moon_nft_application.Elements
{
    /// <summary>
    /// Логика взаимодействия для PresentModal.xaml
    /// </summary>
    public partial class PresentModal : Window
    {
        public presentDTO currPresent;
        public int? currLotId;
        public bool flagOnSale = false;
        public bool flagInProfile = false;
        public bool flagCart = false;

        public PresentModal(presentDTO pres, bool inProfile, bool cartFlag)
        {
            InitializeComponent();
            currPresent = pres;
            flagInProfile = inProfile;
            flagCart = cartFlag;
        }

        public PresentModal(presentDTO pres, bool inProfile, int? _lotId)
        {
            InitializeComponent();
            currPresent = pres;
            flagInProfile = inProfile;
            currLotId = _lotId;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (currPresent is not null)
            {
                DataContext = currPresent;
            }

            if (flagInProfile && !flagCart && currPresent?.UpgradePresent == 0)
            {
                inProfileGrid.Visibility = Visibility.Visible;
                upgradePresentButton.Visibility = Visibility.Visible;
                SellButton.Visibility = Visibility.Collapsed;
            }
            else if (flagInProfile && !flagCart)
            {
                inProfileGrid.Visibility = Visibility.Visible;
            }
            else if (flagInProfile && flagCart)
            {
                cartView.Visibility = Visibility.Visible;
            }
            else
            {
                inCatalogGrid.Visibility = Visibility.Visible;
            }

            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");

            try
            {
                var response = await client.GetAsync($"api/NFT/checkPresent?idPresent={currPresent.IdPresent}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<bool>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                if (!result)
                {
                    flagOnSale = true;
                    SellButton.Content = "Убрать с продажи";
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
            
        }

        private void closeBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private async void SellButton_Click(object sender, RoutedEventArgs e)
        {
            if (flagOnSale)
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:3000/");

                try
                {
                    var response = await client.PostAsync($"api/NFT/TurnOffLot?_presentId={currPresent.IdPresent}", null);
                    response.EnsureSuccessStatusCode();

                    MessageBox.Show("Подарок снят с продажи!");
                    SellButton.Content = "Выставить на продажу";

                    flagOnSale = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                }
            }
            else
            {
                PriceTextBox.Visibility = Visibility.Visible;
                CancelSellButton.Visibility = Visibility.Visible;

                if (double.TryParse(PriceTextBox.Text, out double price) && price > 0)
                {
                    using var client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:3000/");

                    try
                    {
                        var response = await client.PostAsync($"api/NFT/PublishLot?_presentId={currPresent.IdPresent}&_priceLot={price}", null);
                        response.EnsureSuccessStatusCode();

                        MessageBox.Show("Подарок выставлен на продажу!");

                        flagOnSale = true;
                        SellButton.Content = "Убрать с продажи";
                        PriceTextBox.Visibility = Visibility.Collapsed;
                        CancelSellButton.Visibility = Visibility.Collapsed;
                    }
                    catch (HttpRequestException ex)
                    {
                        MessageBox.Show($"Ошибка HTTP: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                    }
                }
                else
                {
                    
                }
            }
        }
        private void PriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.]+$");
        }

        private void CancelSellButton_Click(object sender, RoutedEventArgs e)
        {
            PriceTextBox.Visibility = Visibility.Collapsed;
            CancelSellButton.Visibility = Visibility.Collapsed;
            PriceTextBox.Text = "";
        }

        private async void buyModalbtn_Click(object sender, RoutedEventArgs e)
        {
            int buyerId = -1;
            if (Application.Current.MainWindow is MainWindow main)
            {
                buyerId = main.currentUserId;
                if (!main.isLogIn)
                {
                    MessageBox.Show("Сначала авторизируйтесь!", "Покупка не удалась!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    var authWindow = new AuthWindow();
                    authWindow.Owner = main;
                    Close();
                    authWindow.ShowDialog();
                    return;
                }


                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:3000/");
                HttpResponseMessage response = null;

                try
                {
                    if (currLotId is not null)
                    {
                        response = await client.PutAsync($"api/NFT/PurchasePresent?idLot={currLotId}&buyerId={buyerId}", null);
                    }
                    else
                    {
                        if (DataContext is presentDTO pres)
                        {
                            response = await client.PutAsync($"api/NFT/PurchasePresent?idLot={pres.currLot.IdLot}&buyerId={buyerId}", null);
                        }
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    await RefreshCartData(main.currentUserId);
                    Close();
                    MessageBox.Show(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                }
            }
        }

        private async void cartModalbtn_Click_1(object sender, RoutedEventArgs e)
        {
            int buyerId = -1;
            if (Application.Current.MainWindow is MainWindow main)
            {
                buyerId = main.currentUserId;
                if (!main.isLogIn)
                {
                    MessageBox.Show("Сначала авторизируйтесь!", "Покупка не удалась!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    var authWindow = new AuthWindow();
                    authWindow.Owner = main;
                    Close();
                    authWindow.ShowDialog();
                    return;
                }
            }
           

            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");
            HttpResponseMessage response = null;

            try
            {
                response = await client.PutAsync($"api/NFT/AddLotToCart?idUser={buyerId}&idLot={currLotId}", null);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                MessageBox.Show(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private async void upgradePresentButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:3000/");
                HttpResponseMessage response = null;

                try
                {
                    if (DataContext is presentDTO pres)
                    {
                        response = await client.PutAsync($"api/NFT/UpgradePresent?presentId={pres.IdPresent}", null);
                    }
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    Close();
                    await RefreshCartData(main.currentUserId);
                    MessageBox.Show(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                }
            }
        }

        private async void deleteFromCartBtn_Click(object sender, RoutedEventArgs e)
        {
            int buyerId = -1;
            if (Application.Current.MainWindow is MainWindow main)
            {
                buyerId = main.currentUserId;
                if (!main.isLogIn)
                {
                    MessageBox.Show("Сначала авторизируйтесь!", "Покупка не удалась!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    var authWindow = new AuthWindow();
                    authWindow.Owner = main;
                    Close();
                    authWindow.ShowDialog();
                    return;
                }


                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:3000/");
                HttpResponseMessage response = null;

                try
                {
                    if (currLotId is not null)
                    {
                        response = await client.DeleteAsync($"api/NFT/RemoveLotToCart?idUser={buyerId}&idLot={currLotId}");
                    }
                    else
                    {
                        if (DataContext is presentDTO pres)
                        {
                            response = await client.DeleteAsync($"api/NFT/RemoveLotToCart?idUser={buyerId}&idLot={pres.currLot.IdLot}");
                        }
                    }
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    Close();
                    await RefreshCartData(main.currentUserId);
                    MessageBox.Show(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                }
            }
        }

        private async Task RefreshCartData(int userId)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                if (main.main_frame.Content is ProfilePage prof)
                {
                    var updatedUserInfo = await prof.GetFullInfoOfUser(userId);
                    prof.userInfo = updatedUserInfo;

                    if (prof.flagCart)
                    {
                        prof.ItemsList.ItemsSource = updatedUserInfo.CartUser?.ToList();
                    }
                    else
                    {
                        prof.ItemsList.ItemsSource = updatedUserInfo.PresentsUser.OrderByDescending(p => p.DateUpgradePresent).ToList();
                    }

                    prof.DataContext = updatedUserInfo;
                }
            }
        }
    }
}
