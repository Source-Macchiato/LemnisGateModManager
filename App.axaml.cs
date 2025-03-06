using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;

namespace LemnisGateLauncher;

public partial class App : Application
{
    public static App? Instance { get; private set; }
    private IConfiguration? _configuration;

    public App()
    {
        Instance = this;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Save
    public void SaveSelectedFolderPath(string path)
    {
        CheckConfigFile();

        var configFilePath = Path.Combine(GetAppDirectoryPath(), "appsettings.json");
        var json = File.ReadAllText(configFilePath);
        dynamic? jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

        if (jsonObj != null)
        {
            jsonObj["GamePath"] = path;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(configFilePath, output);
        }
    }

    public void SaveDownloadedMod(Mod mod)
    {
        CheckConfigFile();

        var configFilePath = Path.Combine(GetAppDirectoryPath(), "appsettings.json");
        var json = File.ReadAllText(configFilePath);
        dynamic? jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

        // If Mods array doesn't exists add it
        if (jsonObj != null)
        {
            if (jsonObj["Mods"] == null)
            {
                jsonObj["Mods"] = new Newtonsoft.Json.Linq.JArray();
            }

            // Set values
            var newMod = new Newtonsoft.Json.Linq.JObject
            {
                ["name"] = mod.Name,
                ["id"] = mod.Id,
                ["description"] = mod.Description,
                ["version"] = mod.Version
            };

            jsonObj["Mods"].Add(newMod);

            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(configFilePath, output);
        }
    }

    // Load
    public string? LoadSavedFolderPath()
    {
        CheckConfigFile();

        var path = _configuration?["GamePath"];

        return string.IsNullOrWhiteSpace(path) ? null : path;
    }

    public Mod?[] LoadDownloadedMods()
    {
        CheckConfigFile();

        var modsSection = _configuration?.GetSection("Mods").Get<Mod[]>();
        return modsSection ?? [];
    }

    public bool DoesModExists(Mod mod)
    {
        CheckConfigFile();

        var mods = LoadDownloadedMods();
        return mods.Any(m => m != null && m.Id == mod.Id);
    }

    public void UpdateModVersionInConfig(string modId, string? newVersion)
    {
        try
        {
            if (newVersion != null)
            {
                string jsonPath = Path.Combine(GetAppDirectoryPath(), "appsettings.json");
                if (!File.Exists(jsonPath))
                {
                    System.Diagnostics.Debug.WriteLine("appsettings.json not found.");
                    return;
                }

                string json = File.ReadAllText(jsonPath);
                var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(json);

                if (jsonObj != null && jsonObj["Mods"] is Newtonsoft.Json.Linq.JArray modsArray)
                {
                    foreach (var mod in modsArray)
                    {
                        if (mod["id"]?.ToString() == modId)
                        {
                            mod["version"] = newVersion;
                            break;
                        }
                    }

                    File.WriteAllText(jsonPath, jsonObj.ToString(Newtonsoft.Json.Formatting.Indented));
                    System.Diagnostics.Debug.WriteLine($"Updated version of {modId} in appsettings.json.");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating mod version in config: {ex.Message}");
        }
    }

    public string GetAppDirectoryPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LemnisGateModManager");
    }


    public void CheckConfigFile()
    {
        string appDirectoryPath = GetAppDirectoryPath();
        string configFilePath = Path.Combine(appDirectoryPath, "appsettings.json");

        if (!Directory.Exists(appDirectoryPath))
        {
            Directory.CreateDirectory(appDirectoryPath);
        }

        if (!File.Exists(configFilePath))
        {
            var defaultConfig = new
            {
                GamePath = "",
                Mods = new string[] { }
            };
            string defaultConfigJson = Newtonsoft.Json.JsonConvert.SerializeObject(defaultConfig, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(configFilePath, defaultConfigJson);
        }

        _configuration = new ConfigurationBuilder()
            .SetBasePath(appDirectoryPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
}