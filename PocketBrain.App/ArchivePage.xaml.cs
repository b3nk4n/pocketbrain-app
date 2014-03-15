using Microsoft.Phone.Controls;
using PocketBrain.App.ViewModel;

namespace PocketBrain.App
{
    /// <summary>
    /// The archive page.
    /// </summary>
    public partial class ArchivePage : PhoneApplicationPage
    {
        /// <summary>
        /// Creates a ArchivePage instance.
        /// </summary>
        public ArchivePage()
        {
            InitializeComponent();

            DataContext = ArchiveListViewModel.Instance;
        }
    }
}