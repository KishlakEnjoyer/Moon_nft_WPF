using System;
using Moon_nft_application;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Moon_nft_application.Pages;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace Moon_nft_application.Scripts
{
    public static class Go
    {
        public static void to(Page targetPage)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                main.main_frame.NavigationService.Navigate(targetPage);
            }
        }
    }
}
