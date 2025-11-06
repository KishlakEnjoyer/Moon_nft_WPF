using Moon_nft_application.Models;
using Moon_nft_application.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Moon_nft_application.Elements
{
    /// <summary>
    /// Логика взаимодействия для PresentCard.xaml
    /// </summary>
    public partial class PresentCard : UserControl
    {
        public PresentCard()
        {
            InitializeComponent();
            
        }

        private async void CartButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int buyerId = -1;
            int lotId = -1;
            if (Application.Current.MainWindow is MainWindow main)
            {
                buyerId = main.currentUserId;
                if (!main.isLogIn)
                {
                    MessageBox.Show("Сначала авторизируйтесь!", "Покупка не удалась!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    var authWindow = new AuthWindow();
                    authWindow.Owner = main;
                    authWindow.ShowDialog();
                    return;
                }
            }
            if (DataContext is Lot lot)
            {
                lotId = lot.IdLot;
            }

            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");
            HttpResponseMessage response = null;

            try
            {
                response = await client.PutAsync($"api/NFT/AddLotToCart?idUser={buyerId}&idLot={lotId}", null);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                MessageBox.Show(json);
                CartButton.IsEnabled = false;
                CartButton.Background = new BrushConverter().ConvertFromString("#FF1689FE") as Brush;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private async void BuyButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int buyerId = -1;
            int lotId = -1;
            if (Application.Current.MainWindow is MainWindow main)
            {
                buyerId = main.currentUserId;
                if (!main.isLogIn)
                {
                    MessageBox.Show("Сначала авторизируйтесь!", "Покупка не удалась!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    var authWindow = new AuthWindow();
                    authWindow.Owner = main;
                    authWindow.ShowDialog();
                    return;
                }
            }
            if (DataContext is Lot lot)
            {
                lotId = lot.IdLot;
            }
            MessageBoxResult result = MessageBox.Show(
                "Вы уверены, что хотите совершить покупку?",
                "Подтверждение покупки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:3000/");
                HttpResponseMessage response = null;

                try
                {
                    response = await client.PutAsync($"api/NFT/PurchasePresent?idLot={lotId}&buyerId={buyerId}", null);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show(errorContent, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return; 
                    }

                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();

                    MessageBox.Show(json);
                    if(Application.Current.MainWindow is MainWindow main2)
                    {
                        if (main2.main_frame.Content is catalogPage cp)
                        {
                            await cp.UpdateLotList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                }
            }
        }
    }
}
