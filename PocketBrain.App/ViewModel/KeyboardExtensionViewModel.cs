using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PocketBrain.App.ViewModel
{
    /// <summary>
    /// The keyboard extension view model.
    /// </summary>
    public class KeyboardExtensionViewModel : ViewModelBase
    {
        #region Members

        /// <summary>
        /// The left image for the light theme.
        /// </summary>
        private const string LEFT_IMAGE_LIGHT = "/Assets/AppBar/appbar.back.cropped.dark.png";

        /// <summary>
        /// The left image for the dark theme.
        /// </summary>
        private const string LEFT_IMAGE_DARK = "/Assets/AppBar/appbar.back.cropped.png";

        /// <summary>
        /// The right image for the light theme.
        /// </summary>
        private const string RIGHT_IMAGE_LIGHT = "/Assets/AppBar/appbar.next.cropped.dark.png";

        /// <summary>
        /// The right image for the dark theme.
        /// </summary>
        private const string RIGHT_IMAGE_DARK = "/Assets/AppBar/appbar.next.cropped.png";

        /// <summary>
        /// The total right image for the light theme.
        /// </summary>
        private const string TOTAL_RIGHT_IMAGE_LIGHT = "/Assets/AppBar/appbar.next.total.cropped.dark.png";

        /// <summary>
        /// The total right image for the dark theme.
        /// </summary>
        private const string TOTAL_RIGHT_IMAGE_DARK = "/Assets/AppBar/appbar.next.total.cropped.png";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the left button image path.
        /// </summary>
        public string LeftButtonImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return LEFT_IMAGE_LIGHT;
                else
                    return LEFT_IMAGE_DARK;
            }
        }

        /// <summary>
        /// Gets the right button image path.
        /// </summary>
        public string RightButtonImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return RIGHT_IMAGE_LIGHT;
                else
                    return RIGHT_IMAGE_DARK;
            }
        }

        /// <summary>
        /// Gets the total right button image path.
        /// </summary>
        public string TotalRightButtonImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return TOTAL_RIGHT_IMAGE_LIGHT;
                else
                    return TOTAL_RIGHT_IMAGE_DARK;
            }
        }

        #endregion
    }
}
