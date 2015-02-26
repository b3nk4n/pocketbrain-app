using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketBrain.App
{
    /// <summary>
    /// The global application constants.
    /// </summary>
    class AppConstants
    {
        /// <summary>
        /// The note id.
        /// </summary>
        public const string PARAM_NOTE_ID = "id";

        /// <summary>
        /// The media library index for attachements.
        /// </summary>
        public const string PARAM_TITLE = "title";

        /// <summary>
        /// The media library index for attachements.
        /// </summary>
        public const string PARAM_TEXT = "text";

        /// <summary>
        /// The media library index for attachements.
        /// </summary>
        public const string PARAM_MEDIA_LIB_INDEX = "mediaLibIndex";

        /// <summary>
        /// The PRO version key.
        /// </summary>
        public const string PRO_VERSION_KEY = "pocketbrain_pro";

        /// <summary>
        /// The swipe/flick value add limit for the gesture in X direction.
        /// </summary>
        public const int SWIPE_VALUE_ADD_LIMIT = 1500;

        /// <summary>
        /// The swipe/flick value delete limit for the gesture in X direction.
        /// </summary>
        public const int SWIPE_VALUE_DELETE_LIMIT = 1750;

        /// <summary>
        /// The swipe/flick value limit for the gesture in Y direction.
        /// </summary>
        public const int SWIPE_VALUE_LIMIT_Y = 100;

        /// <summary>
        /// The small tile or lock screen size.
        /// </summary>
        public const string SIZE_S = "small";

        /// <summary>
        /// The medium tile or lock screen size.
        /// </summary>
        public const string SIZE_M = "medium";

        /// <summary>
        /// The large tile or lock screen size.
        /// </summary>
        public const string SIZE_L = "large";

        /// <summary>
        /// The extra large tile or lock screen size.
        /// </summary>
        public const string SIZE_XL = "extralarge";

        /// <summary>
        /// The extra extra large (xxl) tile or lock screen size.
        /// </summary>
        public const string SIZE_XXL = "xxl";
    }
}
