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
using System.Runtime.CompilerServices;
using Moon_nft_application.Elements;
using Moon_nft_api.DTOs;

namespace Moon_nft_application.Pages
{
    public partial class catalogPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<Model> _allModels = new();
        private List<Presentcollection> _allCollections = new();
        private ObservableCollection<LotDTO> _allLots = new();
        public ObservableCollection<LotDTO> AllLots
        {
            get => _allLots;
            set
            {
                _allLots = value;
                OnPropertyChanged();
            }
        }
        private List<Background> _allBackgrounds = new();
        private List<Symbol> _allSymbols = new();
        private bool _isInitialized = false;
        private readonly HttpClient _httpClient = new();

        public catalogPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _allCollections = await LoadAllVid();
                _allBackgrounds = await LoadAllBg();
                _allSymbols = await LoadAllSymbols();
                _allModels = await LoadAllModels();

                SetupFilters();
                _isInitialized = true;
                await UpdateLotList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void SetupFilters()
        {
            var collectionsWithPlaceholder = new List<Presentcollection> { new Presentcollection { IdPresentCollections = 0, NamePresentCollection = "Все коллекции" } };
            collectionsWithPlaceholder.AddRange(_allCollections);

            var bgsWithPlaceholder = new List<Background> { new Background { IdBackground = 0, NameBackground = "Все фоны" } };
            bgsWithPlaceholder.AddRange(_allBackgrounds);

            var symbolsWithPlaceholder = new List<Symbol> { new Symbol { IdSymbol = 0, NameSymbol = "Все узоры" } };
            symbolsWithPlaceholder.AddRange(_allSymbols);

            var modelsWithPlaceholder = new List<Model> { new Model { IdModel = 0, NameModel = "Все модели" } };
            modelsWithPlaceholder.AddRange(_allModels);

            filterVid.ItemsSource = collectionsWithPlaceholder;
            filterBG.ItemsSource = bgsWithPlaceholder;
            filterSymbol.ItemsSource = symbolsWithPlaceholder;
            filterModel.ItemsSource = modelsWithPlaceholder;

            filterVid.SelectedIndex = 0;
            filterBG.SelectedIndex = 0;
            filterSymbol.SelectedIndex = 0;
            filterModel.SelectedIndex = 0;
            filterSort.SelectedIndex = 0;
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
            if (lots.Count > 0)
            {
                AllLots.AddRange(lots);
            }
        }

        private async Task<List<Presentcollection>> LoadAllVid()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:3000/api/NFT/GetAllPresentVid");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Presentcollection>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Presentcollection>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки коллекций: {ex.Message}");
                return new List<Presentcollection>();
            }
        }

        private async Task<List<Background>> LoadAllBg()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:3000/api/NFT/GetAllBG");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Background>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Background>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фонов: {ex.Message}");
                return new List<Background>();
            }
        }

        private async Task<List<Symbol>> LoadAllSymbols()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:3000/api/NFT/GetAllSym");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Symbol>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Symbol>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки узоров: {ex.Message}");
                return new List<Symbol>();
            }
        }

        private async Task<Presentcollection> LoadModelsForCollection(int currCollId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:3000/api/NFT/GetAllModelsForCollection?idCurrColl={currCollId}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Presentcollection>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Presentcollection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки моделей коллекции: {ex.Message}");
                return new Presentcollection();
            }
        }

        private async Task<List<Model>> LoadAllModels()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:3000/api/NFT/GetAllModels");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Model>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Model>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки всех моделей: {ex.Message}");
                return new List<Model>();
            }
        }

        private async Task<List<LotDTO>> LoadAllLots(string search, string _collection, string _model, string _background, string _symbol, string _sort)
        {
            try
            {
                var url = $"http://localhost:3000/api/NFT/GetAllActiveLots?" +
                          $"search={HttpUtility.UrlEncode(search)}" +
                          $"&_collection={HttpUtility.UrlEncode(_collection)}" +
                          $"&_model={HttpUtility.UrlEncode(_model)}" +
                          $"&_background={HttpUtility.UrlEncode(_background)}" +
                          $"&_symbol={HttpUtility.UrlEncode(_symbol)}" +
                          $"&_sort={HttpUtility.UrlEncode(_sort)}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<LotDTO>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<LotDTO>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки лотов: {ex.Message}");
                return new List<LotDTO>();
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

        private void nonUpPresentsShow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                var modalPresent = new buyNonUpPresentModal(main.currentUserId);
                modalPresent.Owner = main;
                modalPresent.ShowDialog();
            }
        }
    }
}