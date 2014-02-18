using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using PocketBrain.App.ViewModel;
using System.Windows.Media.Imaging;
using PhoneKit.Framework.Core.Storage;

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

            DeleteNoteButton.Click += (s, e) =>
                {
                    NavigationService.GoBack();
                };

            DataContext = NoteListViewModel.Instance;
        }

        /// <summary>
        /// Is called when the page is navigated to.
        /// </summary>
        /// <param name="e">The event args</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UpdateAttachedImageSource();
        }

        /// <summary>
        /// Updates the attached image from the models image path.
        /// </summary>
        /// <remarks>
        /// Binding the image URI or path didn't work when the image is located in isolated storage,
        /// so we do it now this way manuelly.
        /// </remarks>
        private void UpdateAttachedImageSource()
        {
            var imagePath = NoteListViewModel.Instance.SelectedNote.AttachedImagePath;
            
            if (!string.IsNullOrEmpty(imagePath))
            {
                BitmapImage img = new BitmapImage();
                img.SetSource(StorageHelper.GetFileStream(imagePath));
                AttachementImage.Source = img;
            }
        }
    }
}