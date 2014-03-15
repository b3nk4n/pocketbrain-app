using Microsoft.Phone.Shell;
using PhoneKit.Framework.Core.Storage;
using PhoneKit.Framework.Core.Tile;
using PhoneKit.Framework.Tile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketBrain.App.Model
{
    /// <summary>
    /// Represents a note.
    /// </summary>
    public class Note
    {
        #region Members


        /// <summary>
        /// The note ID.
        /// </summary>
        /// <remarks>
        /// Public set required for JSON serialization.
        /// </remarks>
        public string Id { get; set; }

        /// <summary>
        /// The note title.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// The note content text.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The optional attached image stored in isolated storage.
        /// </summary>
        public string AttachedImagePath { get; set; }

        /// <summary>
        /// The creation date.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// The deletion date.
        /// </summary>
        public DateTime DateDeleted { get; set; }
 
        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty note.
        /// </summary>
        public Note()
            : this(string.Empty, string.Empty, null)
        {
        }

        /// <summary>
        /// Creates a note.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="content">The content text.</param>
        public Note(string title, string content)
            : this(title, content, null)
        {
        }

        /// <summary>
        /// Creates a note with attachement
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="content">The content text.</param>
        /// <param name="attachedImageUri">The attached image path.</param>
        public Note(string title, string content, string attachedImagePath)
        {
            Id = Guid.NewGuid().ToString();
            Title = title;
            Content = content;
            AttachedImagePath = attachedImagePath;

            DateCreated = DateTime.Now;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Deletes the attached file and also its local copy in isolated storage.
        /// </summary>
        public void RemoveAttachement()
        {
            if (HasAttachement)
            {
                // copy the attachement file path to get no race condition
                var attachementPathCopy = AttachedImagePath;
                AttachedImagePath = null;

                // delete file in background task
                Task.Run(() =>
                {
                    StorageHelper.DeleteFile(attachementPathCopy);
                });
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the note has an attachement.
        /// </summary>
        public bool HasAttachement
        {
            get
            {
                return !string.IsNullOrEmpty(AttachedImagePath);
            }
        }

        #endregion
    }
}
