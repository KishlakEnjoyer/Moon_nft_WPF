using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Moon_nft_api.DTOs;

namespace Moon_nft_application.Elements
{
    public partial class EditProfileModal : Window
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly int _userId;

        public EditProfileModal(int userId)
        {
            InitializeComponent();
            _userId = userId;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var request = new UpdateProfileRequest
            {
                UserId = _userId,
                CurrentPassword = CurrentPasswordBox.Password,
                NewNickname = string.IsNullOrWhiteSpace(NewNicknameBox.Text) ? null : NewNicknameBox.Text,
                NewPassword = string.IsNullOrWhiteSpace(NewPasswordBox.Password) ? null : NewPasswordBox.Password,
                ConfirmNewPassword = string.IsNullOrWhiteSpace(ConfirmNewPasswordBox.Password) ? null : ConfirmNewPasswordBox.Password
            };

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                MessageBox.Show("Введите текущий пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                if (request.NewPassword != request.ConfirmNewPassword)
                {
                    MessageBox.Show("Новый пароль и подтверждение не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("http://localhost:3000/api/User/UpdateProfile", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Профиль успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка: {errorContent}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}