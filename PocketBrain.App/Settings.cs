using PhoneKit.Framework.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketBrain.App
{
    /// <summary>
    /// The application settings.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Setting for list expandation.
        /// </summary>
        public static readonly StoredObject<string> ExpandListsMethod = new StoredObject<string>("explandLists", "2");

        /// <summary>
        /// Setting for whether the vibration is enabled.
        /// </summary>
        public static readonly StoredObject<string> ShowCreationDateOnList = new StoredObject<string>("showCreationDate", "1");

        /// <summary>
        /// Setting for maximum lock screen note items.
        /// </summary>
        public static readonly StoredObject<string> MaximumLockItems = new StoredObject<string>("maxLockItems", "4");

        /// <summary>
        /// Setting for the lockscreen background image path in isolated storage.
        /// </summary>
        /// <remarks>
        /// NULL means to load the DEFAULT image.
        /// </remarks>
        public static readonly StoredObject<string> LockScreenBackgroundImagePath = new StoredObject<string>("lockBackgroundImagePath", null);

        /// <summary>
        /// Setting for whether the note count is visible in the main tile.
        /// </summary>
        public static readonly StoredObject<string> ShowNoteCountOnLiveTile = new StoredObject<string>("showNoteCountOnTile", "1");

        /// <summary>
        /// Setting for whether the add note button is visible.
        /// </summary>
        public static readonly StoredObject<string> ShowAddNoteButton = new StoredObject<string>("showAddNoteButton", "1");
    }
}
