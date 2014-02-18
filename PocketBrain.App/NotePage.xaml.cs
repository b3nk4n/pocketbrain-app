using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PocketBrain.App.ViewModel;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using PhoneKit.Framework.Core.Storage;
using System.IO;

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

            ImageFromAblumButton.Click += (s, e) =>
            {
                var task = new PhotoChooserTask();
                task.ShowCamera = true;
                task.Completed += (se, pr) =>
                {
                    if (pr.Error != null || pr.TaskResult != TaskResult.OK)
                        return;

                    var name = pr.OriginalFileName;
                    FileInfo fileInfo = new FileInfo(pr.OriginalFileName);
                    string filePath = "/attachements/" + fileInfo.Name;
                    if (StorageHelper.SaveFileFromStream(filePath, pr.ChosenPhoto))
                    {
                        NoteListViewModel.Instance.SelectedNote.AttachedImagePath = filePath;
                        UpdateAttachedImageSource();
                    }
                };
                task.Show();
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