using Moon_nft_api.DTOs;
using Moon_nft_application.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Moon_nft_application.Elements
{
    /// <summary>
    /// Логика взаимодействия для buyNonUpPresentModal.xaml
    /// </summary>
    public partial class buyNonUpPresentModal : Window
    {
        public int _currUserId = -1;
        public List<collectionDTO> NonUpgradedPresents;
        public buyNonUpPresentModal(int _id)
        {
            InitializeComponent();
            _currUserId = _id;
        }

        public buyNonUpPresentModal()
        {
            InitializeComponent();
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NonUpgradedPresents = await LoadAllNonUpPresents();

            PresentsListView.ItemsSource = NonUpgradedPresents;

            
        }

        public async Task<List<collectionDTO>> LoadAllNonUpPresents()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");

            try
            {
                var response = await client.GetAsync($"api/NFT/GetAllPresentVid");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<collectionDTO>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                return new List<collectionDTO>();
            }
        }

        private async void buyNonUpPresentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currUserId == -1)
            {
                MessageBox.Show("Авторизируйтесь!");
                Close();
                if (Application.Current.MainWindow is MainWindow main)
                {
                    var authModal = new AuthWindow();
                    authModal.Owner = main;
                    authModal.ShowDialog();
                }
                return;
            }
            var button = sender as Button;
            if (button?.Tag is int collectionId)
            {
                await PurchasePresent(collectionId);
            }
        }

        private async Task PurchasePresent(int collectionId)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");

            var requestData = new Moon_nft_api.DTOs.PurchaseRequest
            {
                CollectionId = collectionId,
                UserId = _currUserId
            };

            try
            {
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/NFT/PurchaseNonUpPresent", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(responseContent, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем данные в списке
                    NonUpgradedPresents = await LoadAllNonUpPresents();
                    PresentsListView.ItemsSource = NonUpgradedPresents;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(errorContent, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при покупке: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        
    }
}
