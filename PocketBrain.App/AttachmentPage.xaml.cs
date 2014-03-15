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

namespace PocketBrain.App
{
    /// <summary>
    /// The attachment page to see the whole scaleable image.
    /// </summary>
    public partial class AttachmentPage : PhoneApplicationPage
    {
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
                transform.ScaleX += e.PinchManipulation.CumulativeScale - transform.ScaleX;
                transform.ScaleX = Math.Max(1, transform.ScaleX);
                System.Diagnostics.Debug.WriteLine(e.PinchManipulation.CumulativeScale);
                transform.ScaleY += e.PinchManipulation.CumulativeScale - transform.ScaleY;
                transform.ScaleY = Math.Max(1, transform.ScaleY);

                // end 
                e.Handled = true;
            }
            else
            {
                transform.TranslateX += e.DeltaManipulation.Translation.X;
                transform.TranslateY += e.DeltaManipulation.Translation.Y;
            }
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
            BitmapImage img = new BitmapImage();
            using (var imageStream = StorageHelper.GetFileStream(imagePath))
            {
                // in case of a not successfully saved image
                if (imageStream == null)
                {
                    AttachementImage.Source = null;
                    return;
                }

                img.SetSource(imageStream);
                AttachementImage.Source = img;
            }
        }
    }
}