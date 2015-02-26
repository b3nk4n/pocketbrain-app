using System.Windows.Controls;
using PhoneKit.Framework.Core.Storage;
using System.Windows.Media.Imaging;

namespace PocketBrain.App.Controls
{
    /// <summary>
    /// The note gual lock screen template.
    /// </summary>
    public partial class NoteLockScreenQuad : UserControl
    {
        /// <summary>
        /// Creates a NoteLockScreenQuad instance.
        /// </summary>
        /// <param name="title1">The title of note1.</param>
        /// <param name="text1">The content text of note1.</param>
        /// <param name="title2">The title of note2.</param>
        /// <param name="text2">The content text of note2.</param>
        /// <param name="title3">The title of note3.</param>
        /// <param name="text3">The content text of note3.</param>
        /// <param name="title4">The title of note4.</param>
        /// <param name="text4">The content text of note4.</param>
        /// <param name="backgroundPath">The background image path.</param>
        public NoteLockScreenQuad(string title1, string text1, string title2, string text2,
            string title3, string text3, string title4, string text4, string backgroundPath)
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

            this.Title3.Text = title3;
            if (string.IsNullOrWhiteSpace(title3))
                this.Title3.Visibility = System.Windows.Visibility.Collapsed;
            this.Text3.Text = text3;

            this.Title4.Text = title4;
            this.Text4.Text = text4;
            if (string.IsNullOrWhiteSpace(title4))
                this.Title4.Visibility = System.Windows.Visibility.Collapsed;

            SetupFontSizeFromSettings();
            SetBackgroundImage(backgroundPath);
        }

        /// <summary>
        /// Sets up the font sized which are logically defined in the settings page.
        /// </summary>
        private void SetupFontSizeFromSettings()
        {
            switch (Settings.LockScreenFontSize.Value)
            {
                case AppConstants.SIZE_S:
                    this.Title1.FontSize = this.Title2.FontSize = this.Title3.FontSize = this.Title4.FontSize = 30;
                    this.Text1.FontSize = this.Text2.FontSize = this.Text3.FontSize = this.Text4.FontSize = 24;
                    break;
                case AppConstants.SIZE_M:
                    this.Title1.FontSize = this.Title2.FontSize = this.Title3.FontSize = this.Title4.FontSize = 36;
                    this.Text1.FontSize = this.Text2.FontSize = this.Text3.FontSize = this.Text4.FontSize = 28;
                    break;
                case AppConstants.SIZE_L:
                    this.Title1.FontSize = this.Title2.FontSize = this.Title3.FontSize = this.Title4.FontSize = 44;
                    this.Text1.FontSize = this.Text2.FontSize = this.Text3.FontSize = this.Text4.FontSize = 32;
                    break;
                case AppConstants.SIZE_XL:
                    this.Title1.FontSize = this.Title2.FontSize = this.Title3.FontSize = this.Title4.FontSize = 52;
                    this.Text1.FontSize = this.Text2.FontSize = this.Text3.FontSize = this.Text4.FontSize = 36;
                    break;
                case AppConstants.SIZE_XXL:
                    this.Title1.FontSize = this.Title2.FontSize = this.Title3.FontSize = this.Title4.FontSize = 58;
                    this.Text1.FontSize = this.Text2.FontSize = this.Text3.FontSize = this.Text4.FontSize = 40;
                    break;
            }
        }

        /// <summary>
        /// Updates the attached image from the models image path.
        /// </summary>
        /// <remarks>
        /// Binding the image URI or path didn't work when the image is located in isolated storage,
        /// so we do it now this way manuelly.
        /// </remarks>
        /// <param name="note">The current note view model.</param>
        private void SetBackgroundImage(string imagePath)
        {
            // check if the default image should be used.
            if (imagePath == null)
                return;

            BitmapImage image = new BitmapImage();
            using (var imageStream = StorageHelper.GetFileStream(imagePath))
            {
                // in case of a not successfully saved image
                if (imageStream == null)
                {
                    BackgroundImage.Source = null;
                    return;
                }

                image.SetSource(imageStream);
                BackgroundImage.Source = image;
            }
        }
    }
}
