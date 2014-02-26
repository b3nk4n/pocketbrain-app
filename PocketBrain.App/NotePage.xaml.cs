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
        /// The acive note ID.
        /// </summary>
        /// <remarks>Required to remember the active note when an attachement is going to be selected.</remarks>
        private string _aciveId;

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

            NoteViewModel currentNote;

            if (NavigationContext.QueryString.ContainsKey("id"))
            {
                currentNote = NoteListViewModel.Instance.GetNoteById(NavigationContext.QueryString["id"]);
            }
            else if (!string.IsNullOrEmpty(_aciveId))
            {
                currentNote = NoteListViewModel.Instance.GetNoteById(_aciveId);
            }
            else
            {
                currentNote = new NoteViewModel();
                NoteListViewModel.Instance.Notes.Insert(0, currentNote);
                _aciveId = currentNote.Id;
            }

            // set the current note as binding context
            DataContext = currentNote;

            UpdateAttachedImageSource(currentNote);
        }

        /// <summary>
        /// Saves the live tile, when the user leaves the notes page.
        /// </summary>
        /// <param name="e">The navigation event args.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // filter navigation to library/camera and delete button event
            if (e.NavigationMode == NavigationMode.New)
                return;

            // reset the active note
            _aciveId = null;

            NoteViewModel note = DataContext as NoteViewModel;

            if (note != null)
            {
                note.UpdateTile();
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
            if (note == null)
            {
                AttachementImage.Source = null;
                return;
            }

            var imagePath = note.AttachedImagePath;
            
            if (!string.IsNullOrEmpty(imagePath))
            {
                BitmapImage img = new BitmapImage();
                using (var imageStream = StorageHelper.GetFileStream(imagePath))
                {
                    img.SetSource(imageStream);
                    AttachementImage.Source = img;
                }
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