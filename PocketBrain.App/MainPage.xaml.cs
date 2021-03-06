using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using PhoneKit.Framework.Support;
using PocketBrain.App.ViewModel;
using System;
using System.Windows.Controls;
using System.Windows;
using PhoneKit.Framework.OS.ShakeGestures;
using System.Windows.Media.Imaging;
using PhoneKit.Framework.OS;
using PocketBrain.App.Misc;
using System.Windows.Media.Animation;

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

        /// <summary>
        /// The last shake event time.
        /// </summary>
        private DateTime _lastShakeEventTime = DateTime.MinValue;

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
            StartupActionManager.Instance.Register(15, ActionExecutionRule.Equals, () =>
                {
                    FeedbackManager.Instance.StartFirst();
                });
            StartupActionManager.Instance.Register(30, ActionExecutionRule.Equals, () =>
                {
                    FeedbackManager.Instance.StartSecond();
                });

            NewNoteButton.Click += (s, e) =>
                {
                    NewNote();
                };

            AboutButton.Click += (s, e) =>
                {
                    NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
                };

            SettingsButton.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
            };

            ArchiveButton.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/ArchivePage.xaml", UriKind.Relative));
            };

            ShakeGesturesHelper.Instance.ShakeGesture += (s, e) =>
                {
                    if (DateTime.Now - _lastShakeEventTime < TimeSpan.FromSeconds(1))
                        return;

                    Dispatcher.BeginInvoke(() =>
                        {
                            VibrationHelper.Vibrate(0.1f);

                            ToggleNoteTemplate();

                            _lastShakeEventTime = DateTime.Now;
                        });
                    
                };
            ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 7;
            ShakeGesturesHelper.Instance.WeakMagnitudeWithoutGravitationThreshold = 0.75;

            ExpansionButton.Click += (s, e) =>
                {
                    ToggleNoteTemplate();
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
                //Scroller.ScrollToVerticalOffset(0);

                ScrollToTopOnNextNavigationTo = false;
            }

            StartupActionManager.Instance.Fire(e);

            // activate shake listener
            if (Settings.ExpandListsMethod.Value == "0" || Settings.ExpandListsMethod.Value == "2")
                ShakeGesturesHelper.Instance.Active = true;

            UpdateExpansionButtonViewState();
            UpdateExpansionButtonVisibility();
            UpdateAddNoteButtonViewState();
        }

        /// <summary>
        /// Updates the add-note button view state.
        /// </summary>
        private void UpdateAddNoteButtonViewState()
        {
            if (Settings.ShowAddNoteButton.Value == "1")
            {
                AddNoteButtonContainer.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                AddNoteButtonContainer.Visibility = System.Windows.Visibility.Collapsed;
            }
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

            // deaktivate shake listener
            ShakeGesturesHelper.Instance.Active = false;
        }

        /// <summary>
        /// Event handler when a note is clicked/tapped.
        /// </summary>
        /// <param name="sender">The clicked note.</param>
        /// <param name="e">The event args.</param>
        private void NoteTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var noteId = ((Button)sender).Tag as string;

            OpenNote(noteId);
        }

        /// <summary>
        /// Navigates to the note page containing a new note.
        /// </summary>
        private void NewNote()
        {
            NavigationService.Navigate(new Uri("/NotePage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Opens the note with the given ID.
        /// </summary>
        /// <param name="noteId">The note ID.</param>
        private void OpenNote(string noteId)
        {
            if (!string.IsNullOrEmpty(noteId))
                NavigationService.Navigate(new Uri(string.Format("/NotePage.xaml?{0}={1}", AppConstants.PARAM_NOTE_ID, noteId), UriKind.Relative));
        }

        /// <summary>
        /// Toggles the note template.
        /// </summary>
        private void ToggleNoteTemplate()
        {
            NoteListViewModel.Instance.ToggleExpanderState();

            UpdateExpansionButtonViewState();
        }

        /// <summary>
        /// Updates the expansion button visibility.
        /// </summary>
        private void UpdateExpansionButtonVisibility()
        {
            NoteListViewModel.Instance.NotifyIsExtensionButtonVisible();
        }

        /// <summary>
        /// Updates the expansion button view state.
        /// </summary>
        private void UpdateExpansionButtonViewState()
        {
            if (NoteListViewModel.Instance.IsExtensionButtonVisible)
            {
                var themedImageSource = (ThemedImageSource)App.Current.Resources["ThemedImageSource"];

                Uri uri;
                if (NoteListViewModel.Instance.IsExpanded)
                {
                    uri = new Uri(themedImageSource.CollapsedImagePath, UriKind.Relative);
                }
                else
                {
                    uri = new Uri(themedImageSource.ExpandImagePath, UriKind.Relative);
                }
                ExpansionButtonImage.Source = new BitmapImage(uri);
            }

            // show the appropriate list depending on the toggle state.
            if (NoteListViewModel.Instance.IsExpanded)
            {
                NotesListMinimized.ItemsSource = null;
                NotesListMinimizedContainer.Visibility = System.Windows.Visibility.Collapsed;
                NotesListContainer.Visibility = System.Windows.Visibility.Visible;
                NotesList.ItemsSource = NoteListViewModel.Instance.Notes;
            }
            else
            {
                NotesList.ItemsSource = null;
                NotesListContainer.Visibility = System.Windows.Visibility.Collapsed;
                NotesListMinimizedContainer.Visibility = System.Windows.Visibility.Visible;
                NotesListMinimized.ItemsSource = NoteListViewModel.Instance.Notes;
            }
        }

        /// <summary>
        /// The swipe gesture listener event.
        /// </summary>
        /// <remarks>
        /// Workaround because toolkit:GestureListener caused errors in combination with ListPicker control of the settings page.
        /// </remarks>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event args.</param>
        private void SwipeManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            double flickX = e.FinalVelocities.LinearVelocity.X;
            double flickY = e.FinalVelocities.LinearVelocity.Y;

            if (flickX > AppConstants.SWIPE_VALUE_ADD_LIMIT && Math.Abs(flickY) < AppConstants.SWIPE_VALUE_LIMIT_Y)
            {
                NewNote();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwipeDeleteManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null)
                return;

            double flickX = e.FinalVelocities.LinearVelocity.X;
            double flickY = e.FinalVelocities.LinearVelocity.Y;

            if (flickX < -AppConstants.SWIPE_VALUE_DELETE_LIMIT && Math.Abs(flickY) < AppConstants.SWIPE_VALUE_LIMIT_Y)
            {
                var data = element.DataContext as NoteViewModel;
                if (data != null)
                {
                    Border b = element as Border;
                    Storyboard storyboard;
                    var id = data.Id;
                    storyboard = b.Resources["DeleteAnimation"] as Storyboard;
                    storyboard.Completed += (s, t) => {
                        if (data != null)
                        {
                            if (data.DeleteCommand.CanExecute(null))
                            {
                                data.DeleteCommand.Execute(null);
                            }
                        }
                    };
                    if (storyboard != null)
                    {
                        storyboard.Begin();
                    }
                }
            }
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