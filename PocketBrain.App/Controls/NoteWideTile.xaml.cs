using System.Windows.Controls;
using System.Windows.Media;

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

            // background transparency
            if (Settings.UseTransparentTile.Value == "0")
            {
                BackgroundBorder.Background = (Brush)Resources["PhoneAccentBrush"];
            }
        }

        /// <summary>
        /// Sets up the font sized which are logically defined in the settings page.
        /// </summary>
        private void SetupFontSizeFromSettings()
        {
            switch (Settings.LiveTileFontSize.Value)
            {
                case AppConstants.SIZE_S:
                    this.Title.FontSize = 36;
                    this.Text.FontSize = 20;
                    break;
                case AppConstants.SIZE_M:
                    this.Title.FontSize = 40;
                    this.Text.FontSize = 24;
                    break;
                case AppConstants.SIZE_L:
                    this.Title.FontSize = 44;
                    this.Text.FontSize = 28;
                    break;
                case AppConstants.SIZE_XL:
                    this.Title.FontSize = 48;
                    this.Text.FontSize = 32;
                    break;
                case AppConstants.SIZE_XXL:
                    this.Title.FontSize = 52;
                    this.Text.FontSize = 36;
                    break;
                case AppConstants.SIZE_XXXL:
                    this.Title.FontSize = 56;
                    this.Text.FontSize = 40;
                    break;
            }
        }
    }
}
