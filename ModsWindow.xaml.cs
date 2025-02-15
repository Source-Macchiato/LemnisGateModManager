using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LemnisGateLauncher
{
    public sealed partial class ModsWindow : Window
    {
        private readonly Dictionary<string, Type> _pages = new Dictionary<string, Type>
        {
            { "LemnisGateLauncher.Views.ModsHomePage", typeof(Views.ModsHomePage) }
            // Ajoutez d'autres pages ici
        };

        public ModsWindow()
        {
            this.InitializeComponent();

            var app = (App)Application.Current;
            app.CustomizeTitleBar(this);

            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First();
            NavigateToPage("LemnisGateLauncher.Views.ModsHomePage");

            ExtendsContentIntoTitleBar = true;
        }

        private void NavigateToPage(string pageTag)
        {
            if (pageTag == null) throw new ArgumentNullException(nameof(pageTag));

            if (_pages.TryGetValue(pageTag, out Type? pageType))
            {
                ContentFrame.Navigate(pageType, null, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
            }
        }

        private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer?.Tag != null)
            {
                string pageTag = args.InvokedItemContainer.Tag.ToString() ?? string.Empty;
                NavigateToPage(pageTag);
            }
        }

        private void NavigationViewControl_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavigationViewControl.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType != null)
            {
                NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(n => n.Tag.Equals(ContentFrame.SourcePageType.FullName));
            }
        }

        private void OnControlsSearchBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string query = args.QueryText;
        }

        private void OnControlsSearchBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            string query = sender.Text;
        }
    }
}