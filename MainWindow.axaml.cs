using System;
using System.IO;
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
            string? searchText = SearchMods.Text;

            // Check if a the exists based on name
            var matchingMod = Mods.FirstOrDefault(mod => mod.Name.Equals(searchText, StringComparison.OrdinalIgnoreCase));

            if (matchingMod != null)
            {
                var dialog = new ContentDialog
                {
                    Title = matchingMod.Name,
                    Content = matchingMod.Description,
                    PrimaryButtonText = "Download",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await DownloadMod(matchingMod);
                }
            }
        }
    }

    private async Task DownloadMod(Mod mod)
    {
        try
        {
            if (string.IsNullOrEmpty(mod.DownloadUrl))
            {
                System.Diagnostics.Debug.WriteLine("Download URL is empty.");
                return;
            }

            // Déterminer le chemin de destination
            string? saveFolder = App.Instance?.LoadSavedFolderPath();
            if (string.IsNullOrEmpty(saveFolder))
            {
                System.Diagnostics.Debug.WriteLine("Save folder path is null or empty.");
                return;
            }

            string savePath = Path.Combine(saveFolder, "LemnisGate", "Content", "Paks");
            string filePath = Path.Combine(savePath, $"{mod.Id}_P.pak");

            // Créer le dossier s'il n'existe pas
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            using (HttpResponseMessage response = await _httpClient.GetAsync(mod.DownloadUrl))
            {
                response.EnsureSuccessStatusCode();
                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(filePath, fileBytes);
            }

            System.Diagnostics.Debug.WriteLine($"Mod downloaded successfully: {filePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error downloading mod: {ex.Message}");
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