using Avalonia.Controls;

namespace LemnisGateLauncher.Views;

public partial class ModsUserControl : UserControl
{
    public ModsUserControl()
    {
        InitializeComponent();

        DataContext = new ModsViewModel();
    }
}