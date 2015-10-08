
namespace PocketBrain.App.Controls
{
    /// <summary>
    /// The note gual lock screen template.
    /// </summary>
    public partial class NoteLockScreenDual : LockScreenUserControl
    {
        /// <summary>
        /// Creates a NoteLockScreenDual instance.
        /// </summary>
        /// <param name="title1">The title of note1.</param>
        /// <param name="text1">The content text of note1.</param>
        /// <param name="title2">The title of note2.</param>
        /// <param name="text2">The content text of note2.</param>
        /// /// <param name="backgroundPath">The background image path.</param>
        public NoteLockScreenDual(string title1, string text1, string title2, string text2, string backgroundPath)
        {
            InitializeComponent();

            this.Title1.Text = title1;
            if (string.IsNullOrWhiteSpace(title1))
                this.Title1.Visibility = System.Windows.Visibility.Collapsed;
            this.Text1.Text = text1;

            this.Title2.Text = title2;
            if (string.IsNullOrWhiteSpace(title2))
                this.Title2.Visibility = System.Windows.Visibility.Collapsed;
            this.Text2.Text = text2;

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
                    this.Title1.FontSize = this.Title2.FontSize = 38;
                    this.Text1.FontSize = this.Text2.FontSize = 26;
                    break;
                case AppConstants.SIZE_M:
                    this.Title1.FontSize = this.Title2.FontSize = 44;
                    this.Text1.FontSize = this.Text2.FontSize = 32;
                    break;
                case AppConstants.SIZE_L:
                    this.Title1.FontSize = this.Title2.FontSize = 52;
                    this.Text1.FontSize = this.Text2.FontSize = 38;
                    break;
                case AppConstants.SIZE_XL:
                    this.Title1.FontSize = this.Title2.FontSize = 62;
                    this.Text1.FontSize = this.Text2.FontSize = 44;
                    break;
                case AppConstants.SIZE_XXL:
                    this.Title1.FontSize = this.Title2.FontSize = 70;
                    this.Text1.FontSize = this.Text2.FontSize = 48;
                    break;
            }
        }
    }
}
