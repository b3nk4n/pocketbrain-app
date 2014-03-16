using Microsoft.Phone.Controls;
using PhoneKit.Framework.OS;
using PhoneKit.Framework.OS.ShakeGestures;
using PocketBrain.App.ViewModel;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace PocketBrain.App
{
    /// <summary>
    /// The archive page.
    /// </summary>
    public partial class ArchivePage : PhoneApplicationPage
    {
        /// <summary>
        /// The last shake event time.
        /// </summary>
        private DateTime _lastShakeEventTime = DateTime.MinValue;

        /// <summary>
        /// Creates a ArchivePage instance.
        /// </summary>
        public ArchivePage()
        {
            InitializeComponent();

            ShakeGesturesHelper.Instance.ShakeGesture += (s, e) =>
            {
                if (DateTime.Now - _lastShakeEventTime < TimeSpan.FromSeconds(1))
                    return;

                Dispatcher.BeginInvoke(() =>
                    {
                        VibrationHelper.Vibrate(0.1f);

                        // swap data template
                        var minimized = (DataTemplate)this.Resources["MinimizedNoteTemplate"];
                        var maximized = (DataTemplate)this.Resources["MaximizedNoteTemplate"];
                        NotesList.ItemTemplate = (NotesList.ItemTemplate == minimized) ? maximized : minimized;

                        _lastShakeEventTime = DateTime.Now;
                    });

            };
            ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 8;
            ShakeGesturesHelper.Instance.WeakMagnitudeWithoutGravitationThreshold = 0.75;

            ExpansionButton.Click += (s, e) =>
                {
                    ToggleNoteTemplate();
                };

            DataContext = ArchiveListViewModel.Instance;
        }

        /// <summary>
        /// When the page is navigated to.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // activate shake listener
            if (Settings.ExpandListsMethod.Value == "0" || Settings.ExpandListsMethod.Value == "2")
                ShakeGesturesHelper.Instance.Active = true;

            UpdateExpansionButtonViewState();
            UpdateExpansionButtonVisibility();
        }

        /// <summary>
        /// Is called when the page is navigated from.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // deaktivate shake listener
            ShakeGesturesHelper.Instance.Active = false;
        }

        /// <summary>
        /// Toggles the note template.
        /// </summary>
        private void ToggleNoteTemplate()
        {
            var minimized = (DataTemplate)this.Resources["MinimizedNoteTemplate"];
            var maximized = (DataTemplate)this.Resources["MaximizedNoteTemplate"];
            NotesList.ItemTemplate = (NotesList.ItemTemplate == minimized) ? maximized : minimized;

            UpdateExpansionButtonViewState();
        }

        /// <summary>
        /// Updates the expansion button visibility.
        /// </summary>
        private void UpdateExpansionButtonVisibility()
        {
            ArchiveListViewModel.Instance.NotifyIsExtensionButtonVisible();
        }

        /// <summary>
        /// Updates the expansion button view state.
        /// </summary>
        private void UpdateExpansionButtonViewState()
        {
            if (!ArchiveListViewModel.Instance.IsExtensionButtonVisible)
                return;

            var maximized = (DataTemplate)this.Resources["MaximizedNoteTemplate"];
            Uri uri;
            if (NotesList.ItemTemplate == maximized)
                uri = new Uri("/Assets/AppBar/appbar.arrow.collapsed.png", UriKind.Relative);
            else
                uri = new Uri("/Assets/AppBar/appbar.arrow.expand.png", UriKind.Relative);

            ExpansionButtonImage.Source = new BitmapImage(uri);
        }
    }
}