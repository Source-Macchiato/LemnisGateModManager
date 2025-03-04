using System;
using System.IO;
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

        var json = File.ReadAllText("appsettings.json");
        dynamic? jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

        if (jsonObj != null)
        {
            jsonObj["GamePath"] = path;

            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("appsettings.json", output);
        }
    }

    public void SaveDownloadedMod(Mod mod)
    {
        CheckConfigFile();

        var json = File.ReadAllText("appsettings.json");
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
            File.WriteAllText("appsettings.json", output);
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

        var json = File.ReadAllText("appsettings.json");
        dynamic? jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

        if (jsonObj != null && jsonObj["Mods"] != null)
        {
            return jsonObj["Mods"].ToObject<Mod[]>();
        }
        else
        {
            return [];
        }
    }

    public bool DoesModExists(Mod mod)
    {
        CheckConfigFile();

        var json = File.ReadAllText("appsettings.json");
        dynamic? jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

        if (jsonObj != null && jsonObj["Mods"] != null)
        {
            foreach (var jsonMod in jsonObj["Mods"])
            {
                if (jsonMod["id"] != null && jsonMod["id"].ToString() == mod.Id)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void UpdateModVersionInConfig(string modId, string? newVersion)
    {
        try
        {
            if (newVersion != null)
            {
                string jsonPath = "appsettings.json";
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


    public void CheckConfigFile()
    {
        string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

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

        if (_configuration == null)
        {
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        }
    }
}