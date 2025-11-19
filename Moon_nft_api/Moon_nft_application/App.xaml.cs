using System.Configuration;
using System.Data;
using System.Text;
using System.Windows;

namespace Moon_nft_application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }

}
