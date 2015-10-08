using PhoneKit.Framework.Core.Storage;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PocketBrain.App.Controls
{
    public class LockScreenUserControl : UserControl
    {
        /// <summary>
        /// Updates the attached image from the models image path.
        /// </summary>
        /// <remarks>
        /// Binding the image URI or path didn't work when the image is located in isolated storage,
        /// so we do it now this way manuelly.
        /// </remarks>
        /// <param name="backgroundImageControl">The background image control.</param>
        /// <param name="imagePath">The image path.</param>
        public void SetBackgroundImage(Image backgroundImageControl, string imagePath)
        {
            // set opacity, even for the default image
            backgroundImageControl.Opacity = Settings.LockscreenImageOpacity.Value;

            // check if the default image should be used.
            if (imagePath == null)
                return;

            BitmapImage image = new BitmapImage();
            using (var imageStream = StorageHelper.GetFileStream(imagePath))
            {
                // in case of a not successfully saved image
                if (imageStream == null)
                {
                    backgroundImageControl.Source = null;
                    return;
                }

                image.SetSource(imageStream);
                backgroundImageControl.Source = image;
            }
        }
    }
}
