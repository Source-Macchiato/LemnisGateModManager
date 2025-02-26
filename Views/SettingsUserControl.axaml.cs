using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace LemnisGateLauncher.Views;

public partial class SettingsUserControl : UserControl
{
    public SettingsUserControl()
    {
        InitializeComponent();        
    }

    private async void OnSelectFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var window = this.VisualRoot as Window;
        
        if (window != null)
        {
            var folder = await SelectFolderAsync(window);

            if (!string.IsNullOrEmpty(folder))
            {
                App.Instance?.SaveSelectedFolderPath(folder);
            }
        }
    }

    private async Task<string?> SelectFolderAsync(Window window)
    {
        var options = new FolderPickerOpenOptions
        {
            Title = "Select Folder"
        };

        var result = await window.StorageProvider.OpenFolderPickerAsync(options);

        if (result.Count > 0)
        {
            return result[0].Path.LocalPath;
        }

        return null;
    }
}