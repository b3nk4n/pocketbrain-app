using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using PocketBrain.App.ViewModel;
using System.Windows.Media.Imaging;
using PhoneKit.Framework.Core.Storage;
using System;
using System.Windows.Input;
using System.Windows.Media;

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
        }

        /// <summary>
        /// Is called when the page is navigated to.
        /// </summary>
        /// <param name="e">The event args</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString != null && 
                NavigationContext.QueryString.ContainsKey("id"))
            {
                _currentNote = NoteListViewModel.Instance.GetNoteById(NavigationContext.QueryString["id"]);
            }
            else if (_currentNote != null && !string.IsNullOrEmpty(_currentNote.Id))
            {
                _currentNote = NoteListViewModel.Instance.GetNoteById(_currentNote.Id);
            }
            else
            {
                _currentNote = new NoteViewModel();
            }

            // set the current note as binding context
            DataContext = _currentNote;

            UpdateAttachedImageSource(_currentNote);
        }

        private NoteViewModel _currentNote;

        /// <summary>
        /// Saves the live tile, when the user leaves the notes page.
        /// </summary>
        /// <param name="e">The navigation event args.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // filter navigation to library/camera and delete button event
            if (e.NavigationMode == NavigationMode.New)
            {
                // add the note to the list, if it wasn't already stored before
                if (!NoteListViewModel.Instance.Notes.Contains(_currentNote))
                    NoteListViewModel.Instance.Notes.Insert(0, _currentNote);
                return;
            }

            // save the current note
            if (_currentNote != null && _currentNote.IsValid)
            {
                // add the note to the list, if it wasn't already stored before
                if (!NoteListViewModel.Instance.Notes.Contains(_currentNote))
                    NoteListViewModel.Instance.Notes.Insert(0, _currentNote);

                _currentNote.UpdateTile();
                _currentNote = null;
            }
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