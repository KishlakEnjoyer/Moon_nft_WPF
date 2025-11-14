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
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Moon_nft_application.Models;
using Moon_nft_api.Services;
using Moon_nft_api.DTOs;

namespace Moon_nft_application.Elements
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        const string ApiBaseUrl = "http://localhost:3000/api";

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void SwitchModeButton_Click(object sender, RoutedEventArgs e)
        {
            TelegramService.OpenTelegramBot();
        }

        

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string login = lbox.Text;
            string password = pbox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Authorization(login, password);            
        }

        public async void Authorization(string login, string password)
        {
            using var httpClient = new HttpClient();
            var loginRequest = new { Email = login, Password = password };
            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{ApiBaseUrl}/user/login", content);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true 
                };

                var result = JsonSerializer.Deserialize<AuthResponse>(
                    await response.Content.ReadAsStringAsync(),
                    options
                );

                if (App.Current.MainWindow is MainWindow main)
                {
                    main.profileTB.Text = result.Nickname;
                    main.isLogIn = true;
                    main.currentUserId = result.UserId;
                }
                Close();

            }
            else
            {
                MessageBox.Show("Неверная почта или пароль!");
            }
        }
    }
}
