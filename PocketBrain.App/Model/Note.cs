using System;
using System.Collections.Generic;
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
        /// The note title.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// The note content text.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The optional attached image.
        /// </summary>
        public Uri AttachedImageUri { get; set; }

        /// <summary>
        /// The creation date.
        /// </summary>
        public DateTime DateCreated { get; private set; }
 
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
        /// <param name="attachedImageUri">The attached image URI.</param>
        public Note(string title, string content, Uri attachedImageUri)
        {
            Title = title;
            Content = content;
            AttachedImageUri = attachedImageUri;

            DateCreated = DateTime.Now;
        }

        #endregion
    }
}
