using PhoneKit.Framework.Core.MVVM;
using PocketBrain.App.Model;
using System;

namespace PocketBrain.App.ViewModel
{
    /// <summary>
    /// Represents the note view model.
    /// </summary>
    public class NoteViewModel : ViewModelBase
    {
        #region Members

        /// <summary>
        /// The note model.
        /// </summary>
        private Note _note;
 
        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty note.
        /// </summary>
        public NoteViewModel()
            : this(new Note())
        {
        }

        /// <summary>
        /// Creates a note.
        /// </summary>
        /// <param name="note">The note.</param>
        public NoteViewModel(Note note)
        {
            _note = note;
        }


        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get
            {
                return _note.Title;
            }
            set
            {
                if (_note.Title != value)
                {
                    _note.Title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public string Content
        {
            get
            {
                return _note.Content;
            }
            set
            {
                if (_note.Content != value)
                {
                    _note.Content = value;
                    NotifyPropertyChanged("Content");
                }
            }
        }

        /// <summary>
        /// Gets or sets the attached image.
        /// </summary>
        public string AttachedImagePath
        {
            get 
            {
                return _note.AttachedImagePath;
            }
            set
            {
                if (_note.AttachedImagePath != value)
                {
                    _note.AttachedImagePath = value;
                    NotifyPropertyChanged("AttachedImagePath");
                }
            }
        }

        /// <summary>
        /// Gets the creation date.
        /// </summary>
        public DateTime DateCreated
        {
            get
            {
                return _note.DateCreated;
            }
        }

        #endregion
    }
}
