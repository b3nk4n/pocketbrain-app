using Microsoft.Phone.Controls;
using PhoneKit.Framework.OS;
using PhoneKit.Framework.OS.ShakeGestures;
using PocketBrain.App.Misc;
using PocketBrain.App.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
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
            ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 7;
            ShakeGesturesHelper.Instance.WeakMagnitudeWithoutGravitationThreshold = 0.75;

            ExpansionButton.Click += (s, e) =>
                {
                    ToggleNoteTemplate();
                };

            UpdateExpansionButtonViewState();

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
            ArchiveListViewModel.Instance.ToggleExpanderState();

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

            var themedImageSource = (ThemedImageSource)App.Current.Resources["ThemedImageSource"];
            Uri uri;
            if (ArchiveListViewModel.Instance.IsExpanded)
            {
                uri = new Uri(themedImageSource.CollapsedImagePath, UriKind.Relative);
                NotesList.ItemTemplate = (DataTemplate)this.Resources["MaximizedNoteTemplate"];
            }
            else
            {
                uri = new Uri(themedImageSource.ExpandImagePath, UriKind.Relative);
                NotesList.ItemTemplate = (DataTemplate)this.Resources["MinimizedNoteTemplate"];
            }

            ExpansionButtonImage.Source = new BitmapImage(uri);
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
                    storyboard.Completed += (s, t) =>
                    {
                        if (data != null)
                        {
                            if (data.DeleteFromArchiveCommand.CanExecute(null))
                            {
                                data.DeleteFromArchiveCommand.Execute(null);
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
    }
}