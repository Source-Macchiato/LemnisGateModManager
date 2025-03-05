using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LemnisGateLauncher
{
    class ModsViewModel : INotifyPropertyChanged
    {
        public static ModsViewModel? Instance { get; private set; }

        public ObservableCollection<ModWrapper> Mods { get; set; } = new();

        private Dictionary<string, ModWrapper> modsDictionary = new();

        public ModsViewModel()
        {
            Instance = this;
            LoadMods();
        }

        public async void LoadMods()
        {
            var loadedMods = App.Instance?.LoadDownloadedMods();
            if (loadedMods != null)
            {
                Mods.Clear();
                modsDictionary.Clear();

                foreach (var mod in loadedMods)
                {
                    if (mod != null)
                    {
                        var wrappedMod = new ModWrapper(mod);
                        Mods.Add(wrappedMod);
                        modsDictionary[wrappedMod.Id] = wrappedMod;
                    }
                }

                await CompareWithRemoteMods();
            }
        }

        private async Task CompareWithRemoteMods()
        {
            string url = "https://raw.githubusercontent.com/Source-Macchiato/LemnisGateModManager/main/mods.json";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string json = await client.GetStringAsync(url);
                    var remoteMods = Newtonsoft.Json.JsonConvert.DeserializeObject<ModsFile>(json);

                    if (remoteMods?.Mods != null)
                    {
                        foreach (var wrappedMod in Mods)
                        {
                            var remoteMod = remoteMods.Mods.FirstOrDefault(m => m.Id == wrappedMod.Id);
                            if (remoteMod != null)
                            {
                                wrappedMod.RemoteVersion = remoteMod.Version;
                                wrappedMod.DownloadUrl = remoteMod.DownloadUrl;
                                wrappedMod.OnPropertyChanged(nameof(wrappedMod.DisplayVersion));
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Mod {wrappedMod.Name} (ID: {wrappedMod.Id}) doesn't exists in mods.json");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading mods: {ex.Message}");
                }
            }
        }

        public bool TryGetModById(string id, out ModWrapper? mod)
        {
            return modsDictionary.TryGetValue(id, out mod);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }


    public class ModWrapper : INotifyPropertyChanged
    {
        private string? _name;
        private string? _version;

        public string Id { get; }
        public string Name
        {
            get => _name ?? string.Empty;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Version
        {
            get => _version ?? string.Empty;
            set
            {
                if (_version != value)
                {
                    _version = value;
                    OnPropertyChanged(nameof(Version));
                }
            }
        }

        public string? RemoteVersion { get; set; }

        public string DisplayVersion =>
        !string.IsNullOrEmpty(RemoteVersion) && RemoteVersion != Version
            ? $"Version {Version} ({RemoteVersion} available)"
            : $"Version {Version}";

        public string? DownloadUrl { get; set; }
        public bool IsEnabled { get; set; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public ModWrapper(Mod mod)
        {
            Id = mod.Id ?? string.Empty;
            Name = mod.Name ?? string.Empty;
            Version = mod.Version ?? string.Empty;
            IsEnabled = true;

            UpdateCommand = new RelayCommand(Update);
            DeleteCommand = new RelayCommand(Delete);
        }

        private async void Update()
        {
            try
            {
                if (string.IsNullOrEmpty(DownloadUrl))
                {
                    System.Diagnostics.Debug.WriteLine("No download URL available for this mod.");
                    return;
                }

                string? saveFolder = App.Instance?.LoadSavedFolderPath();
                if (string.IsNullOrEmpty(saveFolder))
                {
                    System.Diagnostics.Debug.WriteLine("Save folder path is null or empty.");
                    return;
                }

                string savePath = Path.Combine(saveFolder, "LemnisGate", "Content", "Paks");
                string filePath = Path.Combine(savePath, $"{Id}_P.pak");

                // Delete previous file if exists
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error deleting old mod file: {ex.Message}");
                        return;
                    }
                }

                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(DownloadUrl))
                {
                    response.EnsureSuccessStatusCode();
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(filePath, fileBytes);
                }

                // Change local version after update
                if (RemoteVersion != null)
                {
                    Version = RemoteVersion;
                }

                // Save new version in config file
                App.Instance?.UpdateModVersionInConfig(Id, RemoteVersion);

                ModsViewModel.Instance?.LoadMods();

                System.Diagnostics.Debug.WriteLine($"Mod updated successfully: {filePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating mod: {ex.Message}");
            }
        }


        private void Delete()
        {
            App.Instance?.CheckConfigFile();

            var json = File.ReadAllText("appsettings.json");
            var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(json);

            if (jsonObj != null && jsonObj["Mods"] is Newtonsoft.Json.Linq.JArray modsArray)
            {
                var filteredMods = new Newtonsoft.Json.Linq.JArray(modsArray
                    .Where(m => m["id"]?.ToString() != Id));

                jsonObj["Mods"] = filteredMods;

                File.WriteAllText("appsettings.json", jsonObj.ToString(Newtonsoft.Json.Formatting.Indented));

                string? saveFolder = App.Instance?.LoadSavedFolderPath();
                if (!string.IsNullOrEmpty(saveFolder))
                {
                    string savePath = Path.Combine(saveFolder, "LemnisGate", "Content", "Paks");
                    string filePath = Path.Combine(savePath, $"{Id}_P.pak");

                    if (File.Exists(filePath))
                    {
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error while deleting mod: {ex.Message}");
                        }
                    }
                }
            }

            ModsViewModel.Instance?.LoadMods();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ModsFile
    {
        public List<Mod> Mods { get; set; }
    }
}
