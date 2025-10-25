using Moon_nft_application.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Moon_nft_application.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        private int currUserId = -1;
        public ProfilePage(int currentUserId)
        {
            InitializeComponent();
            currUserId = currentUserId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            User userInfo = await GetFullInfoOfUser(currUserId);
            if (userInfo != null) 
            {   
                DataContext = userInfo;
            }
        }

        public async Task<User> GetFullInfoOfUser(int currentUserId)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5192/");

            try
            {
                var response = await client.GetAsync($"api/User/GetFullProfileInfo?userId={currentUserId}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<User>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return result ?? new User() { NicknameUser = "Ошибка при загрузке пользователя" };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                return new User() { NicknameUser = "Ошибка при загрузке пользователя" };
            }
        }

        
    }
}
