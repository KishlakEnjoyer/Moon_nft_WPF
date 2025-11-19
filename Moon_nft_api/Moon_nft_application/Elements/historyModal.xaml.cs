using Moon_nft_api.DTOs;
using Moon_nft_application.Scripts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using System.Windows.Shapes;

namespace Moon_nft_application.Elements
{
    /// <summary>
    /// Логика взаимодействия для historyModal.xaml
    /// </summary>
    public partial class historyModal : Window
    {

        public historyModal()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<transactionDTO> transactions = await loadTransactions();
            List<transactionDTO> sales = await loadSales();

            transactionsListView.ItemsSource = transactions.OrderByDescending(t => t.DateTransaction);
            salesListView.ItemsSource = sales
                .OrderByDescending(t => t.DateTransaction)
                .Select(t => new transactionDTO
                {
                    IdTransaction = t.IdTransaction,
                    IdSaler = t.IdSaler,
                    NameSaler = t.NameSaler,
                    IdBuyer = t.IdBuyer,
                    NameBuyer = t.NameBuyer,
                    IdPresent = t.IdPresent,
                    ImagePresent = t.ImagePresent,
                    CollectionPresent = t.CollectionPresent,
                    displayNum = t.displayNum,
                    DateTransaction = t.DateTransaction,
                    SumTransaction = t.SumTransaction * 0.94f
                })
                .ToList();
            cbox.SelectedIndex = 0;
        }

        public async Task<List<transactionDTO>> loadTransactions()
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:3000/");

                try
                {
                    var response = await client.GetAsync($"api/User/GetAllTransactions?userId={main.currentUserId}");
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<transactionDTO>>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return result ?? new List<transactionDTO>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                    return new List<transactionDTO>();
                }
            }
            else
            {
                return new List<transactionDTO>();
            }
        }

        public async Task<List<transactionDTO>> loadSales()
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:3000/");

                try
                {
                    var response = await client.GetAsync($"api/User/GetAllSales?userId={main.currentUserId}");
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<transactionDTO>>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return result ?? new List<transactionDTO>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                    return new List<transactionDTO>();
                }
            }
            else
            {
                return new List<transactionDTO>();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbox.SelectedIndex)
            {
                case 0:
                    salesListView.Visibility = Visibility.Collapsed;
                    transactionsListView.Visibility = Visibility.Visible;
                    break;
                case 1:
                    transactionsListView.Visibility = Visibility.Collapsed;
                    salesListView.Visibility = Visibility.Visible;

                    break;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is transactionDTO transaction)
                {
                    try
                    {
                        await GenerateReceiptPdf.GeneratePDF(transaction);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при создании чека: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось получить данные транзакции.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
