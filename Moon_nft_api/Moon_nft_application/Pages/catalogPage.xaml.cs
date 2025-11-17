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

        private List<modelDTO> _allModels = new();
        private List<collectionDTO> _allCollections = new();
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
        private List<bgDTO> _allBackgrounds = new();
        private List<symbolDTO> _allSymbols = new();
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
            var collectionsWithPlaceholder = new List<collectionDTO> { new collectionDTO { IdPresentCollections = 0, NamePresentCollection = "Все коллекции" } };
            collectionsWithPlaceholder.AddRange(_allCollections);

            var bgsWithPlaceholder = new List<bgDTO> { new bgDTO { IdBackground = 0, NameBackground = "Все фоны" } };
            bgsWithPlaceholder.AddRange(_allBackgrounds);

            var symbolsWithPlaceholder = new List<symbolDTO> { new symbolDTO { IdSymbol = 0, NameSymbol = "Все узоры" } };
            symbolsWithPlaceholder.AddRange(_allSymbols);

            var modelsWithPlaceholder = new List<modelDTO> { new modelDTO { IdModel = 0, NameModel = "Все модели" } };
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

        private async Task<List<collectionDTO>> LoadAllVid()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:3000/api/NFT/GetAllPresentVid");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<collectionDTO>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<collectionDTO>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки коллекций: {ex.Message}");
                return new List<collectionDTO>();
            }
        }

        private async Task<List<bgDTO>> LoadAllBg()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:3000/api/NFT/GetAllBG");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<bgDTO>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<bgDTO>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фонов: {ex.Message}");
                return new List<bgDTO>();
            }
        }

        private async Task<List<symbolDTO>> LoadAllSymbols()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:3000/api/NFT/GetAllSym");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<symbolDTO>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<symbolDTO>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки узоров: {ex.Message}");
                return new List<symbolDTO>();
            }
        }

        private async Task<collectionDTO> LoadModelsForCollection(int currCollId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:3000/api/NFT/GetAllModelsForCollection?idCurrColl={currCollId}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<collectionDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new collectionDTO();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки моделей коллекции: {ex.Message}");
                return new collectionDTO();
            }
        }

        private async Task<List<modelDTO>> LoadAllModels()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:3000/api/NFT/GetAllModels");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<modelDTO>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<modelDTO>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки всех моделей: {ex.Message}");
                return new List<modelDTO>();
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

            if (filterVid.SelectedItem is collectionDTO selected && selected.IdPresentCollections != 0)
            {
                var collection = await LoadModelsForCollection(selected.IdPresentCollections);
                var models = new List<modelDTO> { new modelDTO { IdModel = 0, NameModel = "Все модели" } };
                models.AddRange((IEnumerable<modelDTO>)collection);
                filterModel.ItemsSource = models;
                filterModel.SelectedIndex = 0;
            }
            else
            {
                var models = new List<modelDTO> { new modelDTO { IdModel = 0, NameModel = "Все модели" } };
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