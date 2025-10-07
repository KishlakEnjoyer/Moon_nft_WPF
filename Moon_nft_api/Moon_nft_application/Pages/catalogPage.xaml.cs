using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Moon_nft_application.Scripts;
using Moon_nft_application.Models;
using System.Net.Http;
using System.Text.Json;

namespace Moon_nft_application.Pages
{
    /// <summary>
    /// Логика взаимодействия для catalogPage.xaml
    /// </summary>
    public partial class catalogPage : Page
    {
        private List<Model> _allModels;
        private List<Presentcollection> _allCollections;
        private List<Lot> _allLots;
        private List<Background> _allBackgrounds;
        private List<Symbol> _allSymbols;

        public catalogPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var realCollections = await LoadAllVid();

            var collectionsWithPlaceholder = new List<Presentcollection>
            {
                new Presentcollection { IdPresentCollections = 0, NamePresentCollection = "Все" }
            };
            collectionsWithPlaceholder.AddRange((IEnumerable<Presentcollection>)realCollections);

            filterVid.ItemsSource = collectionsWithPlaceholder;

            filterVid.SelectedIndex = 0;
        }

        private async Task<List<Presentcollection>> LoadAllVid()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5192/");

            try
            {
                var response = await client.GetAsync("api/NFT/GetAllPresentVid");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<Presentcollection>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return result ?? new List<Presentcollection>();
            }
            catch (Exception ex)
            {
                // Лучше логировать ошибку
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                return new List<Presentcollection>();
            }
        }

        private async Task<Presentcollection> LoadModelsForCollection(int currCollId)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5192/");

            try
            {
                var response = await client.GetAsync($"api/NFT/GetAllModelsForCollection?idCurrColl={currCollId}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Presentcollection>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return result;
            }
            catch (Exception ex)
            {
                // Лучше логировать ошибку
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                return new Presentcollection();
            }
        }

        private async void filterVid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (filterVid.SelectedItem is Presentcollection selectedPresentCollection && filterVid.SelectedIndex != 0)
            {
                Presentcollection currentPresentCollection = await LoadModelsForCollection(selectedPresentCollection.IdPresentCollections);
                var modelsWithPlaceholder = new List<Model>
                {
                    new Model { IdModel = 0, NameModel = "Все" }
                };
                modelsWithPlaceholder.AddRange(currentPresentCollection.IdModels);

                filterModel.ItemsSource = modelsWithPlaceholder;
                filterModel.SelectedIndex = 0;
            }
        }

        
    }
}
