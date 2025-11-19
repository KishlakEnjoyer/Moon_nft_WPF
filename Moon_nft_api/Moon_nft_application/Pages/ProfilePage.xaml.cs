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
using Moon_nft_application.Scripts;
using Moon_nft_api.Services;
using Moon_nft_application.Elements;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moon_nft_api.DTOs;

namespace Moon_nft_application.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        private int currUserId = -1;

        public bool flagCart = false;

        public UserDTO userInfo = null;
        public ProfilePage(int currentUserId)
        {
            InitializeComponent();
            currUserId = currentUserId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            userInfo = await GetFullInfoOfUser(currUserId);
            userInfo.PresentsUser = userInfo.PresentsUser.OrderByDescending(p => p.DateUpgradePresent).ToList();
            if (userInfo != null) 
            {   
                DataContext = userInfo;
            }
            if (userInfo.RoleUser == "Admin")
            {
                adminBtn.Visibility = Visibility.Visible;
            }
        }

        public async Task<UserDTO> GetFullInfoOfUser(int currentUserId)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/");

            try
            {
                var response = await client.GetAsync($"api/User/GetFullProfileInfo?userId={currentUserId}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UserDTO>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return result ?? new UserDTO() { NicknameUser = "Ошибка при загрузке пользователя" };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                return new UserDTO() { NicknameUser = "Ошибка при загрузке пользователя" };
            }
        }

        private async void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                var modal = new EditProfileModal(main.currentUserId); 
                modal.Owner = main;
                modal.ShowDialog();
                userInfo = await GetFullInfoOfUser(currUserId);
                userInfo.PresentsUser = userInfo.PresentsUser.OrderByDescending(p => p.DateUpgradePresent).ToList();
                if (userInfo != null)
                {
                    DataContext = userInfo;
                }
                if (userInfo.RoleUser == "Admin")
                {
                    adminBtn.Visibility = Visibility.Visible;
                }
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TelegramService.OpenTelegramBot();
        }

        private void historyBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                var modalHistory = new historyModal();
                modalHistory.Owner = main;
                modalHistory.ShowDialog();
            }
        }

        private void cartBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!flagCart)
            {
                PresentTitles.Text = "Корзина";
                ItemsList.ItemsSource = userInfo.CartUser.ToList();
                flagCart = true;
            }
            else
            {
                PresentTitles.Text = "Ваши подарки";
                ItemsList.ItemsSource = userInfo.PresentsUser;
                flagCart = false;
            }

        }

        private void adminBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Go.to(new AdminPage());
        }

        private void logoutBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                main.currentUserId = -1;
                main.profileTB.Text = "Войти";
                main.isLogIn = false;
                Go.to(main._catalogPage);
            }
        }

        private void mypresent_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder)
            {
                var presentData = clickedBorder.DataContext as presentDTO;
                if (Application.Current.MainWindow is MainWindow main)
                {
                    var modalPresent = new PresentModal(presentData, true, flagCart);
                    modalPresent.Owner = main;
                    modalPresent.ShowDialog();
                }
                
            }
        }

        
    }
}
