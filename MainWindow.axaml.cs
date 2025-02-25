using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LemnisGateLauncher.Views;

namespace LemnisGateLauncher;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendClientAreaToDecorationsHint = true;

        NavigationView.SelectedItem = ModsItem;
        ContentFrame.Content = new ModsUserControl();
    }

    private void OnNavigationViewSelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected)
        {
            ContentFrame.Content = new SettingsUserControl();
        }
        else if (e.SelectedItem is NavigationViewItem selectedItem)
        {
            switch (selectedItem.Tag)
            {
                case "Mods":
                    ContentFrame.Content = new ModsUserControl();
                    break;
            }
        }
    }
}