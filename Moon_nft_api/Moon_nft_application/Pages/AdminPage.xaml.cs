using MaterialDesignColors;
using Moon_nft_api.DTOs;
using Moon_nft_application.Elements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Moon_nft_application.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        private readonly HttpClient _httpClient = new();

        public AdminPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            cbox.SelectedIndex = 1;
            collectionsListView.ItemsSource = await loadCollections();
            lotsListView.ItemsSource = await loadLots();
        }

        private async void addCollectionBtn_Click(object sender, RoutedEventArgs e)
        {
            AddCollectionWindow addWindow = new AddCollectionWindow();
            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
               collectionsListView.ItemsSource = await loadCollections();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbox.SelectedIndex)
            {
                case 0:
                    addCollectionBtn.Visibility = Visibility.Collapsed;
                    collectionsListView.Visibility = Visibility.Collapsed;

                    break;
                case 1:
                    addCollectionBtn.Visibility = Visibility.Visible;
                    collectionsListView.Visibility = Visibility.Visible;

                    break;
                case 2:
                    addCollectionBtn.Visibility = Visibility.Collapsed;
                    collectionsListView.Visibility = Visibility.Collapsed;
                    lotsListView.Visibility = Visibility.Visible;
                    break;
            }
        }

        public async Task<List<collectionDTO>> loadCollections()
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

        public async Task<List<transactionDTO>> loadTransactions()
        {
            return new List<transactionDTO>();
        }

        public async Task<List<LotDTO>> loadLots()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");

            try
            {
                var response = await client.GetAsync($"api/NFT/GetAllLotsAdmin");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<LotDTO>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                return new List<LotDTO>();
            }
        }

        
    }
}
