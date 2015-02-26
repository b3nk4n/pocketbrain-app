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
                case AppConstants.SIZE_S:
                    this.Title.FontSize = 28;
                    this.Text.FontSize = 18;
                    break;
                case AppConstants.SIZE_M:
                    this.Title.FontSize = 32;
                    this.Text.FontSize = 22;
                    break;
                case AppConstants.SIZE_L:
                    this.Title.FontSize = 36;
                    this.Text.FontSize = 26;
                    break;
                case AppConstants.SIZE_XL:
                    this.Title.FontSize = 42;
                    this.Text.FontSize = 30;
                    break;
                case AppConstants.SIZE_XXL:
                    this.Title.FontSize = 48;
                    this.Text.FontSize = 34;
                    break;
            }
        }
    }
}
