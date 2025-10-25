using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Moon_nft_application.Elements;
using Moon_nft_application.Pages;
using Moon_nft_application.Scripts;

namespace Moon_nft_application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool isLogIn = false;
        public int currentUserId = -1;
        public MainWindow()
        {
            InitializeComponent();
            Go.to(new catalogPage());
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLogIn)
            {
                main_frame.NavigationService.Navigate(new ProfilePage(currentUserId));
            }
            else
            {
                var authWindow = new AuthWindow();
                authWindow.Owner = this;
                authWindow.ShowDialog(); 
            }
            
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (main_frame.Content is not catalogPage)
            {
                main_frame.NavigationService.Navigate(new catalogPage());
            }
        }
    }
}