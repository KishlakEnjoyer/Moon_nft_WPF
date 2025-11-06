using Moon_nft_application.Models;
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
        public Present currPresent;
        public bool flagOnSale = false;
        public bool flagInProfile = false;
        public PresentModal(Present pres, bool inProfile)
        {
            InitializeComponent();
            currPresent = pres;
            flagInProfile = inProfile;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (currPresent is not null)
            {
                DataContext = currPresent;
            }

            if (flagInProfile)
            {

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
                    var response = await client.GetAsync($"api/NFT/TurnOffLot?_presentId={currPresent.IdPresent}");
                    response.EnsureSuccessStatusCode();

                    MessageBox.Show("Подарок снят с продажи!");
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
        private void PriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.]+$");
        }

        private void CancelSellButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cartModalbtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
