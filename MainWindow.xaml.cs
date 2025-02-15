using Microsoft.UI.Xaml;

namespace LemnisGateLauncher
{
    public sealed partial class MainWindow : Window
    {
        private ModsWindow? modsWindow;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Closed += MainWindow_Closed;

            var app = (App)Application.Current;
            app.CustomizeTitleBar(this);
        }

        private void ModsButton_Click(object sender, RoutedEventArgs e)
        {
            if (modsWindow == null)
            {
                modsWindow = new ModsWindow();
                modsWindow.Closed += ModsWindow_Closed;
            }

            modsWindow.Activate();
        }

        private void MainWindow_Closed(object sender, WindowEventArgs e)
        {
            if (modsWindow != null)
            {
                modsWindow.Close();
                modsWindow = null;
            }
        }

        private void ModsWindow_Closed(object sender, WindowEventArgs e)
        {
            modsWindow = null;
        }
    }
}