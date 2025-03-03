using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using LemnisGateLauncher.Views;
using Newtonsoft.Json;

namespace LemnisGateLauncher;

public partial class MainWindow : Window
{
    private static readonly string ModsUrl = "https://raw.githubusercontent.com/Source-Macchiato/LemnisGateModManager/master/mods.json";
    private readonly HttpClient _httpClient = new HttpClient();
    private ObservableCollection<Mod> Mods = new ObservableCollection<Mod>();

    public MainWindow()
    {
        InitializeComponent();
        LoadMods();

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

    private async void LoadMods()
    {
        try
        {
            string json = await _httpClient.GetStringAsync(ModsUrl);
            var modList = JsonConvert.DeserializeObject<ModList>(json);

            Mods.Clear();
            foreach (var mod in modList.Mods)
            {
                Mods.Add(mod);
            }

            SearchMods.ItemsSource = Mods.Select(mod => mod.Name).ToList()
                .OrderBy(x => x);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error loading mods: " + ex.Message);
        }
    }

    private async void OnSearch(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var dialog = new ContentDialog
            {
                Title = "Name",
                Content = "Description",
                PrimaryButtonText = "Download",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            await dialog.ShowAsync();
        }
    }
}

public class ModList
{
    public Mod[]? Mods { get; set; }
}

public class Mod
{
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? Description { get; set; }
    public string? LatestVersion { get; set; }
    public string? DownloadUrl { get; set; }
}