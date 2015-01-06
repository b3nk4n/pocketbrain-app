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

            SetupFontSizeFromSettings();
        }

        /// <summary>
        /// Sets up the font sized which are logically defined in the settings page.
        /// </summary>
        private void SetupFontSizeFromSettings()
        {
            switch (Settings.LiveTileFontSize.Value)
            {
                case "small":
                    this.Title.FontSize = 36;
                    this.Text.FontSize = 20;
                    break;
                case "normal":
                    this.Title.FontSize = 40;
                    this.Text.FontSize = 24;
                    break;
                case "large":
                    this.Title.FontSize = 44;
                    this.Text.FontSize = 28;
                    break;
                case "extralarge":
                    this.Title.FontSize = 48;
                    this.Text.FontSize = 32;
                    break;
            }
        }
    }
}
