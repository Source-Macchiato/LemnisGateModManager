using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LemnisGateLauncher
{
    class ModsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ModWrapper> Mods { get; set; } = new();

        public ModsViewModel()
        {
            LoadMods();
        }

        public void LoadMods()
        {
            var loadedMods = App.Instance?.LoadDownloadedMods();
            if (loadedMods != null)
            {
                Mods.Clear();
                foreach (var mod in loadedMods)
                {
                    if (mod != null)
                    {
                        Mods.Add(new ModWrapper(mod));
                    }
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class ModWrapper : INotifyPropertyChanged
    {
        private string? _name;
        private string? _version;

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

        public ModWrapper(Mod mod)
        {
            Name = mod.Name ?? string.Empty;
            Version = "Version " + mod.Version ?? string.Empty;
            IsEnabled = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
