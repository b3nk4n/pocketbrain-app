﻿using System.Windows.Controls;
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
        /// Creates a NoteLockScreenDual instance.
        /// </summary>
        /// <param name="title1">The title of note1.</param>
        /// <param name="text1">The content text of note1.</param>
        /// <param name="title2">The title of note2.</param>
        /// <param name="text2">The content text of note2.</param>
        /// /// <param name="title3">The title of note3.</param>
        /// <param name="text3">The content text of note3.</param>
        /// <param name="title4">The title of note4.</param>
        /// <param name="text4">The content text of note4.</param>
        /// /// <param name="backgroundPath">The background image path.</param>
        public NoteLockScreenQuad(string title1, string text1, string title2, string text2,
            string title3, string text3, string title4, string text4, string backgroundPath)
        {
            InitializeComponent();

            this.Title1.Text = title1;
            this.Text1.Text = text1;
            this.Title2.Text = title2;
            this.Text2.Text = text2;
            this.Title3.Text = title3;
            this.Text3.Text = text3;
            this.Title4.Text = title4;
            this.Text4.Text = text4;

            SetBackgroundImage(backgroundPath);
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