using PhoneKit.Framework.Core.MVVM;
using PocketBrain.App.Model;
using System;
using System.Collections.Generic;
using System.Windows.Input;

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

        /// <summary>
        /// The container of this item.
        /// </summary>
        private IList<NoteViewModel> _container;

        /// <summary>
        /// The delete command.
        /// </summary>
        private ICommand _deleteCommand;
 
        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty note.
        /// </summary>
        /// <param name="container">The container of the note.</param>
        public NoteViewModel(IList<NoteViewModel> container)
            : this(container, new Note())
        {
        }

        /// <summary>
        /// Creates a note.
        /// </summary>
        /// <param name="container">The container of the note.</param>
        /// <param name="note">The note.</param>
        public NoteViewModel(IList<NoteViewModel> container, Note note)
        {
            _container = container;
            _note = note;

            _deleteCommand = new DelegateCommand(() =>
                {
                    _container.Remove(this);
                });
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

        /// <summary>
        /// Gets the delete note command.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand;
            }
        }

        #endregion
    }
}
