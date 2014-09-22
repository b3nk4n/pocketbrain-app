using PhoneKit.Framework.Core.Themeing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketBrain.App.Misc
{
    /// <summary>
    /// The themed image source.
    /// </summary>
    public class ThemedImageSource : ThemedImageSourceBase
    {
        public ThemedImageSource()
            : base("Assets/Images")
        {
        }

        /// <summary>
        /// Get the speak path.
        /// </summary>
        public string SpeakImagePath
        {
            get
            {
                return GetImagePath("microphone.png");
            }
        }

        /// <summary>
        /// Get the prepend path.
        /// </summary>
        public string SpeakPrependImagePath
        {
            get
            {
                return GetImagePath("prepend.png");
            }
        }

        /// <summary>
        /// Get the speak path.
        /// </summary>
        public string SpeakReplaceImagePath
        {
            get
            {
                return GetImagePath("replace.png");
            }
        }

        /// <summary>
        /// Get the speak path.
        /// </summary>
        public string SpeakAppendImagePath
        {
            get
            {
                return GetImagePath("append.png");
            }
        }

        /// <summary>
        /// Get the email path.
        /// </summary>
        public string EmailImagePath
        {
            get
            {
                return GetImagePath("email.png");
            }
        }

        /// <summary>
        /// Get the message path.
        /// </summary>
        public string MessageImagePath
        {
            get
            {
                return GetImagePath("message.png");
            }
        }

        /// <summary>
        /// Get the whatsapp path.
        /// </summary>
        public string WhatsappImagePath
        {
            get
            {
                return GetImagePath("whatsapp.png");
            }
        }

        /// <summary>
        /// Get the paperclip path.
        /// </summary>
        public string PaperclipImagePath
        {
            get
            {
                return GetImagePath("paperclip.png");
            }
        }

        /// <summary>
        /// Get the paperclip remove path.
        /// </summary>
        public string PaperclipRemoveImagePath
        {
            get
            {
                return GetImagePath("paperclip.remove.png");
            }
        }

        /// <summary>
        /// Get the share path.
        /// </summary>
        public string ShareImagePath
        {
            get
            {
                return GetImagePath("share.png");
            }
        }

        /// <summary>
        /// Get the share open path.
        /// </summary>
        public string ShareOpenImagePath
        {
            get
            {
                return GetImagePath("share.open.png");
            }
        }

        /// <summary>
        /// Get the pin path.
        /// </summary>
        public string PinImagePath
        {
            get
            {
                return GetImagePath("pin.png");
            }
        }

        /// <summary>
        /// Get the pin remove path.
        /// </summary>
        public string PinRemoveImagePath
        {
            get
            {
                return GetImagePath("pin.remove.png");
            }
        }

        /// <summary>
        /// Get the eye path.
        /// </summary>
        public string EyeImagePath
        {
            get
            {
                return GetImagePath("eye.png");
            }
        }

        /// <summary>
        /// Get the eye hide path.
        /// </summary>
        public string EyeHideImagePath
        {
            get
            {
                return GetImagePath("eye.hide.png");
            }
        }

        /// <summary>
        /// Get the delete path.
        /// </summary>
        public string DeleteImagePath
        {
            get
            {
                return GetImagePath("delete.png");
            }
        }

        /// <summary>
        /// Get the lock path.
        /// </summary>
        public string LockImagePath
        {
            get
            {
                return GetImagePath("lock.png");
            }
        }

        /// <summary>
        /// Get the add note path.
        /// </summary>
        public string AddNoteImagePath
        {
            get
            {
                return GetImagePath("add.note.png");
            }
        }

        /// <summary>
        /// Get the archive path.
        /// </summary>
        public string ArchiveImagePath
        {
            get
            {
                return GetImagePath("archive.png");
            }
        }

        /// <summary>
        /// Get the settings path.
        /// </summary>
        public string SettingsImagePath
        {
            get
            {
                return GetImagePath("settings.png");
            }
        }

        /// <summary>
        /// Get the info path.
        /// </summary>
        public string InfoImagePath
        {
            get
            {
                return GetImagePath("information.png");
            }
        }

        /// <summary>
        /// Get the archive clear path.
        /// </summary>
        public string ArchiveClearImagePath
        {
            get
            {
                return GetImagePath("archive.clear.png");
            }
        }

        /// <summary>
        /// Gets the left button image path.
        /// </summary>
        public string LeftButtonImagePath
        {
            get
            {
                return GetImagePath("back.cropped.png");
            }
        }

        /// <summary>
        /// Gets the right button image path.
        /// </summary>
        public string RightButtonImagePath
        {
            get
            {
                return GetImagePath("next.cropped.png");
            }
        }

        /// <summary>
        /// Gets the total right button image path.
        /// </summary>
        public string TotalRightButtonImagePath
        {
            get
            {
                return GetImagePath("next.total.cropped.png");
            }
        }

        /// <summary>
        /// Gets the expand button image path.
        /// </summary>
        public string ExpandImagePath
        {
            get
            {
                return GetImagePath("arrow.expand.png");
            }
        }

        /// <summary>
        /// Gets the collapsed button image path.
        /// </summary>
        public string CollapsedImagePath
        {
            get
            {
                return GetImagePath("arrow.collapsed.png");
            }
        }

        /// <summary>
        /// Gets the backup button image path.
        /// </summary>
        public string BackupImagePath
        {
            get
            {
                return GetImagePath("backup.png");
            }
        }

        /// <summary>
        /// Gets the store button image path.
        /// </summary>
        public string StoreImagePath
        {
            get
            {
                return GetImagePath("appbar.star.add.png");
            }
        }
    }
}
