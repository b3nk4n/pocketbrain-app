using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PocketBrain.App.Resources;
using PhoneKit.Framework.Support;
using PocketBrain.App.ViewModel;
using System;

namespace PocketBrain.App
{
    /// <summary>
    /// The main page.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        // Konstruktor
        public MainPage()
        {
            InitializeComponent();

            // Beispielcode zur Lokalisierung der ApplicationBar
            BuildLocalizedApplicationBar();

            // register startup actions
            StartupActionManager.Instance.Register(2, ActionExecutionRule.LessOrEquals, () =>
            {
                //MessageBox.Show("Less or Equals 2 startups of the app.");
                FeedbackManager.Instance.StartFirst();
            });
            StartupActionManager.Instance.Register(7, ActionExecutionRule.Equals, () =>
            {
                //MessageBox.Show("Equals 7 startups of the app.");
                FeedbackManager.Instance.StartSecond();
            });

            DataContext = NoteListViewModel.Instance;
        }

        /// <summary>
        /// When the page is navigated to.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            StartupActionManager.Instance.Fire();
        }

        /// <summary>
        /// Builds the localized app bar.
        /// </summary>
        private void BuildLocalizedApplicationBar()
        {
            // ApplicationBar der Seite einer neuen Instanz von ApplicationBar zuweisen
            ApplicationBar = new ApplicationBar();

            // Eine neue Schaltfläche erstellen und als Text die lokalisierte Zeichenfolge aus AppResources zuweisen.
            //ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
            //appBarButton.Text = AppResources.AppBarButtonText;
            //ApplicationBar.Buttons.Add(appBarButton);

            // Ein neues Menüelement mit der lokalisierten Zeichenfolge aus AppResources erstellen
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarAbout);
            appBarMenuItem.Click += (s, e) =>
                {
                    NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
                };
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }
    }
}