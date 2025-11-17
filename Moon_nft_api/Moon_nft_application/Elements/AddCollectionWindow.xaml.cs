using Microsoft.Win32;
using Moon_nft_api.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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
    /// Логика взаимодействия для AddCollectionWindow.xaml
    /// </summary>
    public partial class AddCollectionWindow : Window
    {
        private byte[] selectedImageBytes;

        public AddCollectionWindow()
        {
            InitializeComponent();
        }

        private void BrowseImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string imagePath = openFileDialog.FileName;
                    selectedImageBytes = File.ReadAllBytes(imagePath);

                    BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
                    previewImage.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название коллекции", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedImageBytes == null)
            {
                MessageBox.Show("Пожалуйста, выберите изображение", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(limitTextBox.Text, out int limit) || limit <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректный лимит (положительное число)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(countTextBox.Text, out int count) || count < 0)
            {
                MessageBox.Show("Пожалуйста, введите корректное количество (неотрицательное число)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!float.TryParse(priceTextBox.Text, out float price) || price < 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную цену (неотрицательное число)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var model = new AddCollectionModel
                {
                    Name = nameTextBox.Text,
                    ImageBase64 = selectedImageBytes != null ? Convert.ToBase64String(selectedImageBytes) : "",
                    Limit = limit,
                    Count = count,
                    Price = price
                };

                using (HttpClient client = new HttpClient())
                {
                    string json = System.Text.Json.JsonSerializer.Serialize(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("http://localhost:3000/api/NFT/AddNewCollection", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Коллекция успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка: {error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
