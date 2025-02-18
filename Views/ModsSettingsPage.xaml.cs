using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LemnisGateLauncher.Views
{
    public sealed partial class ModsSettingsPage : Page, INotifyPropertyChanged
    {
        private string _folderPath = "Path where Lemnis Gate is located";

        public string FolderPath
        {
            get => _folderPath;
            set
            {
                if (_folderPath != value)
                {
                    _folderPath = value;
                    OnPropertyChanged(nameof(FolderPath));
                }
            }
        }

        public ModsSettingsPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
        {
            // Vérifiez que sender n'est pas null et qu'il peut être converti en Button
            if (sender is Button senderButton)
            {
                //disable the button to avoid double-clicking
                senderButton.IsEnabled = false;

                // Create a folder picker
                FolderPicker openPicker = new FolderPicker();

                // Retrieve the window handle (HWND) of the current WinUI 3 window.
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.ModsWindow);

                // Initialize the folder picker with the window handle (HWND).
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

                // Set options for your folder picker
                openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
                openPicker.FileTypeFilter.Add("*");

                // Open the picker for the user to pick a folder
                StorageFolder folder = await openPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                    FolderPath = folder.Path;
                }
                else
                {
                    FolderPath = "Operation cancelled.";
                }

                //re-enable the button
                senderButton.IsEnabled = true;
            }
        }
    }
}
