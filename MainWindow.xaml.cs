using Microsoft.UI.Xaml;

namespace LemnisGateLauncher
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Closed += MainWindow_Closed;

            var app = (App)Application.Current;
            app.CustomizeTitleBar(this);
        }

        private void ModsButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.ModsWindow == null)
            {
                App.ModsWindow = new ModsWindow();
                App.ModsWindow.Closed += ModsWindow_Closed;
            }

            App.ModsWindow.Activate();
        }

        private void MainWindow_Closed(object sender, WindowEventArgs e)
        {
            if (App.ModsWindow != null)
            {
                App.ModsWindow.Close();
                App.ModsWindow = null;
            }
        }

        private void ModsWindow_Closed(object sender, WindowEventArgs e)
        {
            App.ModsWindow = null;
        }
    }
}