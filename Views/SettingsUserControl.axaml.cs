using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace LemnisGateLauncher.Views;

public partial class SettingsUserControl : UserControl, INotifyPropertyChanged
{
    private string? _selectedFolderPath;
    public string SelectedFolderPath
    {
        get => _selectedFolderPath ?? App.Instance?.LoadSavedFolderPath() ?? "Folder where Lemnis Gate is located";
        set
        {
            if (_selectedFolderPath != value)
            {
                _selectedFolderPath = value;
                OnPropertyChanged(nameof(SelectedFolderPath));
            }
        }
    }

    public SettingsUserControl()
    {
        InitializeComponent();

        DataContext = this;
    }

    private async void OnSelectFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var window = VisualRoot as Window;
        
        if (window != null)
        {
            var folder = await SelectFolderAsync(window);

            if (!string.IsNullOrEmpty(folder))
            {
                App.Instance?.SaveSelectedFolderPath(folder);
                SelectedFolderPath = folder;
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

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}