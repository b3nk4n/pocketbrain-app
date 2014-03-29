using System.Windows.Controls;

namespace PocketBrain.App.Controls
{
    /// <summary>
    /// Note wide tile.
    /// </summary>
    public partial class NoteWideTile : UserControl
    {
        /// <summary>
        /// Creates a new wide tile instance.
        /// </summary>
        public NoteWideTile()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates a new wide tile instance.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="content">The note content.</param>
        public NoteWideTile(string title, string content)
            : this()
        {
            this.Title.Text = title;
            if (string.IsNullOrWhiteSpace(title))
                this.Title.Visibility = System.Windows.Visibility.Collapsed;

            this.Text.Text = content;
        }
    }
}
