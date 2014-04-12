using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using PhoneKit.Framework.Core.Storage;

namespace PocketBrain.App.Controls
{
    /// <summary>
    /// The note lock screen template.
    /// </summary>
    public partial class NoteLockScreen : UserControl
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
                    this.Title.FontSize = 46;
                    this.Text.FontSize = 32;
                    break;
                case "normal":
                    this.Title.FontSize = 52;
                    this.Text.FontSize = 36;
                    break;
                case "large":
                    this.Title.FontSize = 58;
                    this.Text.FontSize = 40;
                    break;
                case "extralarge":
                    this.Title.FontSize = 64;
                    this.Text.FontSize = 44;
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
