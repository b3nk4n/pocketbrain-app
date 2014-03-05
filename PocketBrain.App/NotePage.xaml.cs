using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using PocketBrain.App.ViewModel;
using System.Windows.Media.Imaging;
using PhoneKit.Framework.Core.Storage;
using System;
using System.Windows.Input;
using System.Windows.Media;
using PhoneKit.Framework.Storage;

namespace PocketBrain.App
{
    /// <summary>
    /// The page of a single note.
    /// </summary>
    public partial class NotePage : PhoneApplicationPage
    {
        /// <summary>
        /// Creates the note page instance.
        /// </summary>
        public NotePage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
                {
                    if (Text.Text.Length == 0)
                    {
                        // select the text end of the notes content text field
                        Text.Focus();
                        Text.Select(Text.Text.Length, 0);
                    }
                };

            DeleteNoteButton.Click += (s, e) =>
                {
                    ClearAttachedImageSource();

                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack(); 
                    else
                        NavigationService.Navigate(new Uri("/MainPage.xaml?clearbackstack=true", UriKind.Relative));
                };

            RemoveAttachementButton.Click += (s, e) =>
                {
                    ClearAttachedImageSource();
                };

            ShareButton.Click += (s, e) =>
                {
                    ShareButton.Visibility = System.Windows.Visibility.Collapsed;
                    SharingContainer.Visibility = System.Windows.Visibility.Visible;
                };
        }

        /// <summary>
        /// Is called when the page is navigated to.
        /// </summary>
        /// <param name="e">The event args</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // reset sharing buttons
            ResetSharingButtonsVisibility();

            // load state
            if (PhoneStateHelper.ValueExists("currentNote"))
            {
                NoteListViewModel.Instance.CurrentNote = PhoneStateHelper.LoadValue<NoteViewModel>("currentNote");
                PhoneStateHelper.DeleteValue("currentNote");
            }

            if (NavigationContext.QueryString != null && 
                NavigationContext.QueryString.ContainsKey("id"))
            {
                NoteListViewModel.Instance.CurrentNote = NoteListViewModel.Instance.GetNoteById(NavigationContext.QueryString["id"]);
            }
            else if (NoteListViewModel.Instance.CurrentNote != null && !string.IsNullOrEmpty(NoteListViewModel.Instance.CurrentNote.Id))
            {
                NoteListViewModel.Instance.CurrentNote = NoteListViewModel.Instance.GetNoteById(NoteListViewModel.Instance.CurrentNote.Id);
            }
            else
            {
                NoteListViewModel.Instance.CurrentNote = new NoteViewModel();
            }

            // set the current note as binding context
            DataContext = NoteListViewModel.Instance.CurrentNote;

            UpdateAttachedImageSource(NoteListViewModel.Instance.CurrentNote);
        }

        /// <summary>
        /// Saves the live tile, when the user leaves the notes page.
        /// </summary>
        /// <param name="e">The navigation event args.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // verify it was a BACK button or a WINDOWS button
            if (e.Uri.OriginalString == "app://external/")
            {
                NoteListViewModel.Instance.UpdateLockScreen();
            }

            // ensure there is an active note
            if (NoteListViewModel.Instance.CurrentNote == null)
                return;

            // save state
            if (e.NavigationMode != NavigationMode.Back || e.Uri.OriginalString == "app://external/")
            {
                PhoneStateHelper.SaveValue("currentNote", NoteListViewModel.Instance.CurrentNote);
            }

            // filter navigation to library/camera and delete button event
            if (e.NavigationMode == NavigationMode.New)
            {
                // add the note to the list, if it wasn't already stored before
                if (!NoteListViewModel.Instance.Notes.Contains(NoteListViewModel.Instance.CurrentNote))
                    NoteListViewModel.Instance.Notes.Insert(0, NoteListViewModel.Instance.CurrentNote);
                return;
            }

            // save the current note
            if (NoteListViewModel.Instance.CurrentNote.IsValid)
            {
                // add the note to the list, if it wasn't already stored before
                if (!NoteListViewModel.Instance.Notes.Contains(NoteListViewModel.Instance.CurrentNote))
                    NoteListViewModel.Instance.Notes.Insert(0, NoteListViewModel.Instance.CurrentNote);
            }

            NoteListViewModel.Instance.CurrentNote.UpdateTile();
            NoteListViewModel.Instance.CurrentNote = null;
        }

        /// <summary>
        /// Updates the attached image from the models image path.
        /// </summary>
        /// <remarks>
        /// Binding the image URI or path didn't work when the image is located in isolated storage,
        /// so we do it now this way manuelly.
        /// </remarks>
        /// <param name="note">The current note view model.</param>
        private void UpdateAttachedImageSource(NoteViewModel note)
        {
            if (note != null && note.HasAttachement)
            {
                var imagePath = note.AttachedImagePath;

                BitmapImage img = new BitmapImage();
                using (var imageStream = StorageHelper.GetFileStream(imagePath))
                {
                    // in case of a not successfully saved image
                    if (imageStream == null)
                    {
                        AttachementImage.Source = null;
                        AttachementImageContainer.Visibility = System.Windows.Visibility.Collapsed;
                        return;
                    }

                    img.SetSource(imageStream);
                    AttachementImage.Source = img;
                    AttachementImageContainer.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                AttachementImage.Source = null;
                AttachementImageContainer.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Clears the attached image source.
        /// </summary>
        /// <remarks>
        /// This is required because the image can not be deleted when any control has still
        /// any reference to this file.
        /// </remarks>
        private void ClearAttachedImageSource()
        {
            AttachementImage.Source = null;
        }

        /// <summary>
        /// Resets the settings button visibility.
        /// </summary>
        private void ResetSharingButtonsVisibility()
        {
            ShareButton.Visibility = System.Windows.Visibility.Visible;
            SharingContainer.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// The manipulation delta event for pinch zoom of the attachement image.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void AttachementImage_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            if (e.PinchManipulation != null)
            {
                var transform = (CompositeTransform)AttachementImage.RenderTransform;

                // Scale Manipulation
                transform.ScaleX = e.PinchManipulation.CumulativeScale;
                transform.ScaleY = e.PinchManipulation.CumulativeScale;

                // Translate manipulation
                var originalCenter = e.PinchManipulation.Original.Center;
                var newCenter = e.PinchManipulation.Current.Center;
                transform.TranslateX = newCenter.X - originalCenter.X;
                transform.TranslateY = newCenter.Y - originalCenter.Y;

                // end 
                e.Handled = true;
            }
        }
    }
}