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

namespace PocketBrain.App
{
    public partial class NotePage : PhoneApplicationPage
    {
        public NotePage()
        {
            InitializeComponent();

            DeleteNoteButton.Click += (s, e) =>
                {
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
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
                    var stream = pr.ChosenPhoto;
                };
                task.Show();
            };

            ImageFromCameraButton.Click += (s, e) =>
            {
                var task = new CameraCaptureTask();
                task.Completed += (se, pr) =>
                    {
                        if (pr.Error != null || pr.TaskResult != TaskResult.OK)
                            return;

                        var name = pr.OriginalFileName;
                        var stream = pr.ChosenPhoto;
                    };
                task.Show();
            };

            DataContext = NoteListViewModel.Instance;
        }

        private void task_Completed(object sender, PhotoResult e)
        {
            throw new NotImplementedException();
        }
    }
}