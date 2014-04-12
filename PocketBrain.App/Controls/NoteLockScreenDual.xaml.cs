using System.Windows.Controls;
using PhoneKit.Framework.Core.Storage;
using System.Windows.Media.Imaging;

namespace PocketBrain.App.Controls
{
    /// <summary>
    /// The note gual lock screen template.
    /// </summary>
    public partial class NoteLockScreenDual : UserControl
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
            SetBackgroundImage(backgroundPath);
        }

        /// <summary>
        /// Sets up the font sized which are logically defined in the settings page.
        /// </summary>
        private void SetupFontSizeFromSettings()
        {
            switch (Settings.LockScreenFontSize.Value)
            {
                case "small":
                    this.Title1.FontSize = this.Title2.FontSize = 40;
                    this.Text1.FontSize = this.Text2.FontSize = 28;
                    break;
                case "normal":
                    this.Title1.FontSize = this.Title2.FontSize = 44;
                    this.Text1.FontSize = this.Text2.FontSize = 32;
                    break;
                case "large":
                    this.Title1.FontSize = this.Title2.FontSize = 50;
                    this.Text1.FontSize = this.Text2.FontSize = 36;
                    break;
                case "extralarge":
                    this.Title1.FontSize = this.Title2.FontSize = 56;
                    this.Text1.FontSize = this.Text2.FontSize = 40;
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
