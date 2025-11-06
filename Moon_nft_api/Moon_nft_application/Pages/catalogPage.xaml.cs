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
using System.Collections.ObjectModel;
using System.Web;
using System.ComponentModel;
using System.Runtime.CompilerServices; // Для HttpUtility.UrlEncode

namespace Moon_nft_application.Pages
{
    /// <summary>
    /// Логика взаимодействия для catalogPage.xaml
    /// </summary>
    public partial class catalogPage : Page, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private List<Model> _allModels;
        private List<Presentcollection> _allCollections;

        private ObservableCollection<Lot> _allLots = new();
        public ObservableCollection<Lot> AllLots
        {
            get => _allLots;
            set
            {
                _allLots = value;
                OnPropertyChanged();
            }
        }

        private List<Background> _allBackgrounds;
        private List<Symbol> _allSymbols;

        private bool _isInitialized = false; // Флаг для предотвращения срабатывания фильтров при инициализации

        public catalogPage()
        {
            InitializeComponent();
            DataContext = this;

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _allCollections = await LoadAllVid();
            _allBackgrounds = await LoadAllBg();
            _allSymbols = await LoadAllSymbols();

            var collectionsWithPlaceholder = new List<Presentcollection>
            {
                new Presentcollection { IdPresentCollections = 0, NamePresentCollection = "Все коллекции" }
            };
            var bgsWithPlaceholder = new List<Background>
            {
                new Background { IdBackground = 0, NameBackground = "Все фоны" }
            };
            var symbolsWithPlaceholder = new List<Symbol>
            {
                new Symbol { IdSymbol = 0, NameSymbol = "Все узоры" }
            };

            bgsWithPlaceholder.AddRange(_allBackgrounds);
            symbolsWithPlaceholder.AddRange(_allSymbols);
            collectionsWithPlaceholder.AddRange(_allCollections);

            filterVid.ItemsSource = collectionsWithPlaceholder;
            filterBG.ItemsSource = bgsWithPlaceholder;
            filterSymbol.ItemsSource = symbolsWithPlaceholder;

            filterVid.SelectedIndex = 0;
            filterBG.SelectedIndex = 0;
            filterSymbol.SelectedIndex = 0;
            filterSort.SelectedIndex = 0;

            var modelsWithPlaceholder = new List<Model>
            {
                new Model { IdModel = 0, NameModel = "Все модели" }
            };
            _allModels = await LoadAllModels();
            modelsWithPlaceholder.AddRange(_allModels);
            filterModel.ItemsSource = modelsWithPlaceholder;
            filterModel.SelectedIndex = 0;

            _isInitialized = true; 

            await UpdateLotList();
        }

        public async Task UpdateLotList()
        {
            if (!_isInitialized) return;

            string collectionName = (filterVid.SelectedItem as Presentcollection)?.NamePresentCollection ?? "Все коллекции";
            string modelName = (filterModel.SelectedItem as Model)?.NameModel ?? "Все модели";
            string backgroundName = (filterBG.SelectedItem as Background)?.NameBackground ?? "Все фоны";
            string symbolName = (filterSymbol.SelectedItem as Symbol)?.NameSymbol ?? "Все узоры";
            string sortName = (filterSort.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Нет (Сортировка)";
            string searchQuery = searchBar?.Text ?? "";

            var lots = await LoadAllLots(searchQuery, collectionName, modelName, backgroundName, symbolName, sortName);

            AllLots.Clear();
            foreach (var lot in lots)
            {
                AllLots.Add(lot);
            }
        }

        private async Task<List<Presentcollection>> LoadAllVid()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");
            try
            {
                var response = await client.GetAsync("api/NFT/GetAllPresentVid");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Presentcollection>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Presentcollection>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки коллекций: {ex.Message}");
                return new List<Presentcollection>();
            }
        }

        private async Task<List<Background>> LoadAllBg()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");
            try
            {
                var response = await client.GetAsync("api/NFT/GetAllBG");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Background>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Background>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фонов: {ex.Message}");
                return new List<Background>();
            }
        }

        private async Task<List<Symbol>> LoadAllSymbols()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");
            try
            {
                var response = await client.GetAsync("api/NFT/GetAllSym");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Symbol>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Symbol>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки узоров: {ex.Message}");
                return new List<Symbol>();
            }
        }

        private async Task<Presentcollection> LoadModelsForCollection(int currCollId)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");
            try
            {
                var response = await client.GetAsync($"api/NFT/GetAllModelsForCollection?idCurrColl={currCollId}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Presentcollection>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Presentcollection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки моделей коллекции: {ex.Message}");
                return new Presentcollection();
            }
        }

        private async Task<List<Model>> LoadAllModels()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");
            try
            {
                var response = await client.GetAsync("api/NFT/GetAllModels");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Model>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Model>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки всех моделей: {ex.Message}");
                return new List<Model>();
            }
        }

        private async Task<List<Lot>> LoadAllLots(string search, string _collection, string _model, string _background, string _symbol, string _sort)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");
            try
            {
                var url = $"api/NFT/GetAllActiveLots?" +
                          $"search={HttpUtility.UrlEncode(search)}" +
                          $"&_collection={HttpUtility.UrlEncode(_collection)}" +
                          $"&_model={HttpUtility.UrlEncode(_model)}" +
                          $"&_background={HttpUtility.UrlEncode(_background)}" +
                          $"&_symbol={HttpUtility.UrlEncode(_symbol)}" +
                          $"&_sort={HttpUtility.UrlEncode(_sort)}";

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Lot>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Lot>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки лотов: {ex.Message}");
                return new List<Lot>();
            }
        }

        private async void filterVid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || filterVid.SelectedItem == null) return;

            if (filterVid.SelectedItem is Presentcollection selected && selected.IdPresentCollections != 0)
            {
                var collection = await LoadModelsForCollection(selected.IdPresentCollections);
                var models = new List<Model> { new Model { IdModel = 0, NameModel = "Все модели" } };
                models.AddRange(collection?.IdModels ?? new List<Model>());
                filterModel.ItemsSource = models;
                filterModel.SelectedIndex = 0;
            }
            else
            {
                var models = new List<Model> { new Model { IdModel = 0, NameModel = "Все модели" } };
                models.AddRange(_allModels);
                filterModel.ItemsSource = models;
                filterModel.SelectedIndex = 0;
            }

            await UpdateLotList();
        }

        private async void filterModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitialized) await UpdateLotList();
        }

        private async void filterBG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitialized) await UpdateLotList();
        }

        private async void filterSymbol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitialized) await UpdateLotList();
        }

        private async void filterSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitialized) await UpdateLotList();
        }

        private async void searchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitialized) await UpdateLotList();
        }
    }
}