using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;
using Moon_nft_api.DTOs;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace Moon_nft_application.Scripts
{
    public static class GenerateReceiptPdf
    {
        public static Task GeneratePDF(transactionDTO transaction)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Чек_транзакции_{transaction.IdTransaction}.pdf",
                Filter = "PDF файлы|*.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var document = new iTextSharp.text.Document(PageSize.A4);
                    var writer = PdfWriter.GetInstance(document, new FileStream(saveFileDialog.FileName, FileMode.Create));

                    document.Open();

                    // --- ШАПКА ЧЕКА ---
                    var logoPath = @"D:\Moon_nft_WPF\Moon_nft_api\Moon_nft_application\Resources\Images\Logo.png";

                    if (File.Exists(logoPath))
                    {
                        var logoImage = Image.GetInstance(logoPath);
                        logoImage.Alignment = Element.ALIGN_CENTER;
                        logoImage.ScaleToFit(150, 80);
                        document.Add(logoImage);
                    }

                    // --- Создаём шрифт с поддержкой кириллицы ---
                    var baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    var fontNormal = new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK);
                    var fontBold = new Font(baseFont, 12, Font.BOLD, BaseColor.BLACK);
                    var titleFont = new Font(baseFont, 20, Font.BOLD, BaseColor.BLACK);

                    document.Add(new iTextSharp.text.Paragraph(" "));
                    document.Add(new iTextSharp.text.Paragraph(" ", new Font(baseFont, 16, Font.BOLD, BaseColor.BLACK)) { Alignment = Element.ALIGN_CENTER });
                    document.Add(new iTextSharp.text.Paragraph("ЧЕК О ПОКУПКЕ", titleFont) { Alignment = Element.ALIGN_CENTER });
                    document.Add(new iTextSharp.text.Paragraph(" ", new Font(baseFont, 16, Font.BOLD, BaseColor.BLACK)) { Alignment = Element.ALIGN_CENTER });

                    // --- Разделитель ---
                    var separator = new iTextSharp.text.Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.GRAY, Element.ALIGN_CENTER, 1)));
                    document.Add(separator);

                    // --- Информация о транзакции ---
                    document.Add(new iTextSharp.text.Paragraph("Детали транзакции:", fontBold));
                    document.Add(new iTextSharp.text.Paragraph($"Номер транзакции: {transaction.IdTransaction}", fontNormal));
                    document.Add(new iTextSharp.text.Paragraph($"Дата покупки: {transaction.DateTransaction:yyyy-MM-dd}", fontNormal));
                    document.Add(new iTextSharp.text.Paragraph($"Сумма: {transaction.SumTransaction} TON", fontNormal));
                    document.Add(new iTextSharp.text.Paragraph($"Продавец: {transaction.NameSaler}", fontNormal));
                    document.Add(new iTextSharp.text.Paragraph($"Покупатель: {transaction.NameBuyer}", fontNormal));
                    document.Add(new iTextSharp.text.Paragraph(" "));

                    // --- Информация о подарке ---
                    document.Add(new iTextSharp.text.Paragraph("Информация о подарке:", fontBold));
                    document.Add(new iTextSharp.text.Paragraph($"Название: {transaction.CollectionPresent}", fontNormal));
                    document.Add(new iTextSharp.text.Paragraph($"Номер: {transaction.displayNum}", fontNormal));
                    document.Add(new iTextSharp.text.Paragraph(" "));

                    // --- Изображение подарка ---
                    if (transaction.ImagePresent != null && transaction.ImagePresent.Length > 0)
                    {
                        try
                        {
                            var giftImage = Image.GetInstance(transaction.ImagePresent);
                            giftImage.Alignment = Element.ALIGN_CENTER;
                            giftImage.ScaleToFit(250, 250);
                            document.Add(giftImage);
                        }
                        catch
                        {
                            document.Add(new iTextSharp.text.Paragraph("Изображение подарка недоступно.", fontNormal));
                        }
                    }

                    // --- Подпись ---
                    document.Add(new iTextSharp.text.Paragraph(" "));
                    document.Add(new iTextSharp.text.Paragraph("Спасибо за покупку!", new Font(baseFont, 14, Font.BOLD, BaseColor.BLUE)) { Alignment = Element.ALIGN_CENTER });

                    // --- Футер ---
                    document.Add(new iTextSharp.text.Paragraph(" "));
                    var footer = new iTextSharp.text.Paragraph("© Moon NFT — Ваша коллекция начинается здесь", new Font(baseFont, 9, Font.ITALIC, BaseColor.GRAY)) { Alignment = Element.ALIGN_CENTER };
                    document.Add(footer);

                    document.Close();

                    MessageBox.Show("Чек успешно сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании PDF: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return Task.CompletedTask;
        }
    }
}