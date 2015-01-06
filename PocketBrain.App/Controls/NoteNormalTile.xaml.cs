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
            if (string.IsNullOrWhiteSpace(title))
                this.Title.Visibility = System.Windows.Visibility.Collapsed;

            this.Text.Text = content;

            SetupFontSizeFromSettings();
        }

        /// <summary>
        /// Sets up the font sized which are logically defined in the settings page.
        /// </summary>
        private void SetupFontSizeFromSettings()
        {
            switch(Settings.LiveTileFontSize.Value)
            {
                case "small":
                    this.Title.FontSize = 28;
                    this.Text.FontSize = 18;
                    break;
                case "normal":
                    this.Title.FontSize = 32;
                    this.Text.FontSize = 22;
                    break;
                case "large":
                    this.Title.FontSize = 36;
                    this.Text.FontSize = 26;
                    break;
                case "extralarge":
                    this.Title.FontSize = 42;
                    this.Text.FontSize = 30;
                    break;
            }
        }
    }
}
