using System.Windows.Controls;

namespace PocketBrain.App.Controls
{
    /// <summary>
    /// The normal note tile.
    /// </summary>
    public partial class NoteNormalTile : UserControl
    {
        /// <summary>
        /// Creates a new normal tile instance.
        /// </summary>
        public NoteNormalTile()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates a new normal tile instance.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="content">The note content.</param>
        public NoteNormalTile(string title, string content)
            : this()
        {
            this.Title.Text = title;
            this.Text.Text = content;
        }
    }
}
