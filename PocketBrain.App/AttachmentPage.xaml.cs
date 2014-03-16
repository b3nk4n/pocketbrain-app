using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PhoneKit.Framework.Core.Storage;
using Microsoft.Xna.Framework;

namespace PocketBrain.App
{
    /// <summary>
    /// The attachment page to see the whole scaleable image.
    /// </summary>
    public partial class AttachmentPage : PhoneApplicationPage
    {
        /// <summary>
        /// The displayed image.
        /// </summary>
        private BitmapImage _image;

        /// <summary>
        /// Creates a AttachmentPage instance.
        /// </summary>
        public AttachmentPage()
        {
            InitializeComponent();

            OrientationChanged += (s, e) =>
                {
                    ResetTransform();
                };

            AttachementImage.DoubleTap += (s, e) =>
                {
                    ResetTransform();
                };
        }

        /// <summary>
        /// Resets the transformation.
        /// </summary>
        private void ResetTransform()
        {
            var transform = (CompositeTransform)AttachementImage.RenderTransform;
            transform.ScaleX = 1;
            transform.ScaleY = 1;
            transform.TranslateX = 1;
            transform.TranslateY = 1;

            UpdateImageScaleCenter();
        }

        /// <summary>
        /// The manipulation delta event for pinch zoom of the attachement image.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void AttachementImage_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            var transform = (CompositeTransform)AttachementImage.RenderTransform;

            if (e.PinchManipulation != null)
            {
                // Scale Manipulation
                transform.ScaleX = MathHelper.Clamp((float)e.PinchManipulation.CumulativeScale, 1.0f, 2.25f);
                transform.ScaleY = MathHelper.Clamp((float)e.PinchManipulation.CumulativeScale, 1.0f, 2.25f);
            }
            else
            {
                transform.TranslateX += e.DeltaManipulation.Translation.X;
                transform.TranslateY += e.DeltaManipulation.Translation.Y;
            }

            // end 
            e.Handled = true;
        }

        /// <summary>
        /// When the page is navigated to.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString != null &&
                NavigationContext.QueryString.ContainsKey("imagePath"))
            {
                string imagePath = NavigationContext.QueryString["imagePath"];
                UpdateAttachedImageSource(imagePath);
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
        private void UpdateAttachedImageSource(string imagePath)
        {
            _image = new BitmapImage();
            using (var imageStream = StorageHelper.GetFileStream(imagePath))
            {
                // in case of a not successfully saved image
                if (imageStream == null)
                {
                    AttachementImage.Source = null;
                    return;
                }

                _image.SetSource(imageStream);
                AttachementImage.Source = _image;

                UpdateImageScaleCenter();
            }
        }

        /// <summary>
        /// Updates the image scale center.
        /// </summary>
        private void UpdateImageScaleCenter()
        {
            // ensure there is an image
            if (_image == null || _image.PixelWidth == 0)
                return;

            var transform = (CompositeTransform)AttachementImage.RenderTransform;
            double imageRatio = (double)_image.PixelHeight / _image.PixelWidth;

            // portrait mode
            if (Orientation == PageOrientation.Portrait ||
                Orientation == PageOrientation.PortraitUp ||
                Orientation == PageOrientation.PortraitDown)
            {
                double w;
                double h;
                if (imageRatio >= 1.66)
                {
                    h = 800;
                    w = h / imageRatio;
                }
                else
                {
                    w = 480;
                    h = w * imageRatio;
                }

                transform.CenterX = w / 2;
                transform.CenterY = h / 2;
            }
            else // landscape mode
            {
                double w;
                double h;
                if (imageRatio <= 0.6)
                {
                    w = 800;
                    h = w * imageRatio;  
                }
                else
                {
                    h = 480;
                    w = h / imageRatio;
                }

                transform.CenterX = w / 2;
                transform.CenterY = h / 2;
            }
        }
    }
}