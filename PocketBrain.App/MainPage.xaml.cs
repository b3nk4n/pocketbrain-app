using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PocketBrain.App.Resources;
using PhoneKit.Framework.Support;
using PocketBrain.App.ViewModel;
using System;
using System.Windows.Controls;
using PhoneKit.Framework.Voice;
using PhoneKit.Framework.Core.LockScreen;
using PhoneKit.Framework.Core.Graphics;
using System.Windows;
using System.Windows.Media;

namespace PocketBrain.App
{
    /// <summary>
    /// The main page.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// Indicates whether the scroller is navigated to the top on the next NavigatedTo event.
        /// </summary>
        /// <remarks>
        /// Used when a new item is created to ensure the new item is visible.
        /// </remarks>
        private static bool _scrollToTopOnNextNavigationTo = false;

        // Konstruktor
        public MainPage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
                {
                    WelcomeAnimation.Stop();
                    if (NoteListViewModel.Instance.Notes.Count == 0)
                        WelcomeAnimation.Begin();
                };

            // register startup actions
            StartupActionManager.Instance.Register(5, ActionExecutionRule.Equals, () =>
                {
                    FeedbackManager.Instance.StartFirst();
                });
            StartupActionManager.Instance.Register(10, ActionExecutionRule.Equals, () =>
                {
                    FeedbackManager.Instance.StartSecond();
                });

            NewNoteButton.Click += (s, e) =>
                {
                    NavigationService.Navigate(new Uri("/NotePage.xaml", UriKind.Relative));
                };

            AboutButton.Click += (s, e) =>
                {
                    NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
                };

            ArchiveButton.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/ArchivePage.xaml", UriKind.Relative));
            };

            DataContext = NoteListViewModel.Instance;
        }

        /// <summary>
        /// The selection changed event of the notes list.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void NotesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;

            if (listBox == null)
                return;

            string noteId = ((NoteViewModel)listBox.SelectedItem).Id;

            // unselect the item
            /*NotesList.SelectionChanged -= NotesList_SelectionChanged;
            NotesList.SelectedItem = null;
            NotesList.SelectionChanged += NotesList_SelectionChanged;*/

            NavigationService.Navigate(new Uri("/NotePage.xaml?id=" + noteId, UriKind.Relative));
        }

        /// <summary>
        /// When the page is navigated to.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString != null && 
                NavigationContext.QueryString.ContainsKey("clearbackstack"))
            {

                while (NavigationService.CanGoBack)
                    NavigationService.RemoveBackEntry();
            }

            // check for scroller reset.
            if (ScrollToTopOnNextNavigationTo)
            {
                Scroller.ScrollToVerticalOffset(0);

                ScrollToTopOnNextNavigationTo = false;
            }

            StartupActionManager.Instance.Fire();
        }

        /// <summary>
        /// Is called when the page is navigated from.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // verify it was a BACK button or a WINDOWS button
            if (e.NavigationMode == NavigationMode.Back ||
                e.Uri.OriginalString == "app://external/")
            {
                NoteListViewModel.Instance.UpdateLockScreen();
            }
        }

        /// <summary>
        /// Event handler when a note is clicked.
        /// </summary>
        /// <param name="sender">The clicked note.</param>
        /// <param name="e">The event args.</param>
        private void NoteClicked(object sender, RoutedEventArgs e)
        {
            var noteId = ((Button)sender).Tag as string;

            if (!string.IsNullOrEmpty(noteId))
                NavigationService.Navigate(new Uri("/NotePage.xaml?id=" + noteId, UriKind.Relative));
        }

        #region Properties

        /// <summary>
        /// Gets or the the scroll to top flag.
        /// </summary>
        public static bool ScrollToTopOnNextNavigationTo
        {
            get
            {
                return _scrollToTopOnNextNavigationTo;
            }
            set
            {
                _scrollToTopOnNextNavigationTo = value;
            }
        }

        #endregion
    }
}