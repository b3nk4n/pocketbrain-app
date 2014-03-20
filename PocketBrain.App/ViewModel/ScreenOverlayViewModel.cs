using PhoneKit.Framework.Core.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketBrain.App.ViewModel
{
    /// <summary>
    /// The screen overlay view model for fixed screen elements.
    /// </summary>
    public class ScreenOverlayViewModel
    {
        /// <summary>
        /// The expand image for the light theme.
        /// </summary>
        private const string EXPAND_LIGHT = "Assets/AppBar/appbar.arrow.expand.png";

        /// <summary>
        /// The expand image for the dark theme.
        /// </summary>
        private const string EXPAND_DARK = "Assets/AppBar/appbar.arrow.expand.dark.png";

        /// <summary>
        /// The collapsed image for the light theme.
        /// </summary>
        private const string COLLAPSED_LIGHT = "Assets/AppBar/appbar.arrow.collapsed.png";

        /// <summary>
        /// The collapsed image for the dark theme.
        /// </summary>
        private const string COLLAPSED_DARK = "Assets/AppBar/appbar.arrow.collapsed.dark.png";

        /// <summary>
        /// Gets the expand button image path.
        /// </summary>
        public string ExpandImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return EXPAND_DARK;
                else
                    return EXPAND_LIGHT;
            }
        }

        /// <summary>
        /// Gets the collapsed button image path.
        /// </summary>
        public string CollapsedImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return COLLAPSED_DARK;
                else
                    return COLLAPSED_LIGHT;
            }
        }
    }
}
