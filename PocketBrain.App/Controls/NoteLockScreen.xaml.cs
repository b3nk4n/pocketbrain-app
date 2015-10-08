
namespace PocketBrain.App.Controls
{
    /// <summary>
    /// The note lock screen template.
    /// </summary>
    public partial class NoteLockScreen : LockScreenUserControl
    {
        /// <summary>
        /// Creates a NoteLockScreen instance.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="text">The content text.</param>
        /// <param name="backgroundPath">The background image path.</param>
        public NoteLockScreen(string title, string text, string backgroundPath)
        {
            InitializeComponent();

            this.Title.Text = title;
            if (string.IsNullOrWhiteSpace(title))
                this.Title.Visibility = System.Windows.Visibility.Collapsed;
            this.Text.Text = text;

            SetupFontSizeFromSettings();
            SetBackgroundImage(BackgroundImage, backgroundPath);
        }

        /// <summary>
        /// Sets up the font sized which are logically defined in the settings page.
        /// </summary>
        private void SetupFontSizeFromSettings()
        {
            switch (Settings.LockScreenFontSize.Value)
            {
                case AppConstants.SIZE_S:
                    this.Title.FontSize = 44;
                    this.Text.FontSize = 30;
                    break;
                case AppConstants.SIZE_M:
                    this.Title.FontSize = 52;
                    this.Text.FontSize = 36;
                    break;
                case AppConstants.SIZE_L:
                    this.Title.FontSize = 60;
                    this.Text.FontSize = 42;
                    break;
                case AppConstants.SIZE_XL:
                    this.Title.FontSize = 70;
                    this.Text.FontSize = 48;
                    break;
                case AppConstants.SIZE_XXL:
                    this.Title.FontSize = 78;
                    this.Text.FontSize = 52;
                    break;
                case AppConstants.SIZE_XXXL:
                    this.Title.FontSize = 86;
                    this.Text.FontSize = 56;
                    break;
            }
        }
    }
}
