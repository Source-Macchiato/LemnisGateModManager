using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using LemnisGateLauncher.Views;

namespace LemnisGateLauncher;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendClientAreaToDecorationsHint = true;

        // Select default page
        NavigationView.SelectedItem = ModsItem;
        ContentFrame.Content = new ModsUserControl();
    }

    private void OnNavigationViewSelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsUserControl), null, new EntranceNavigationTransitionInfo());
        }
        else if (e.SelectedItem is NavigationViewItem selectedItem)
        {
            switch (selectedItem.Tag)
            {
                case "Mods":
                    ContentFrame.Navigate(typeof(ModsUserControl), null, new EntranceNavigationTransitionInfo());
                    break;
            }
        }
    }
}