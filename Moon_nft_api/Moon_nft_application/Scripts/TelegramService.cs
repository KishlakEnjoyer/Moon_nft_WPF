using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;



namespace Moon_nft_api.Services
{
    public static class TelegramService
    {
        public static void OpenTelegramBot()
        {
            try
            {
                // Открывает в Telegram-клиенте, если он установлен
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "tg://resolve?domain=moon_exchange_bot&start",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // Если не удалось — открываем в браузере
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://t.me/moon_exchange_bot?start",
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show("Не удалось открыть телеграм!");
                }
            }
        }
    }
}
