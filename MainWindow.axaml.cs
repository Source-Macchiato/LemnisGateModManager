using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LemnisGateLauncher;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendClientAreaToDecorationsHint = true;
    }
}