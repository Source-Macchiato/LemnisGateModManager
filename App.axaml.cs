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

    public string? LoadSavedFolderPath()
    {
        CheckConfigFile();

        var path = _configuration?["GamePath"];

        return string.IsNullOrWhiteSpace(path) ? null : path;
    }

    public void CheckConfigFile()
    {
        string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        if (!File.Exists(configFilePath))
        {
            var defaultConfig = new
            {
                GamePath = ""
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