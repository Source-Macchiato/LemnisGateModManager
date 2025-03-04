using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        public void LoadMods()
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

        public bool IsEnabled { get; set; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public ModWrapper(Mod mod)
        {
            Id = mod.Id ?? string.Empty;
            Name = mod.Name ?? string.Empty;
            Version = "Version " + mod.Version ?? string.Empty;
            IsEnabled = true;

            UpdateCommand = new RelayCommand(Update);
            DeleteCommand = new RelayCommand(Delete);
        }

        private void Update()
        {
            // Implémentation de la mise à jour
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
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
