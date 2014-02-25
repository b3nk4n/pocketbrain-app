using PhoneKit.Framework.Core.MVVM;
using PocketBrain.App.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PhoneKit.Framework.Core.Storage;

namespace PocketBrain.App.ViewModel
{
    /// <summary>
    /// Represents note list view model.
    /// </summary>
    public class NoteListViewModel : ViewModelBase
    {
        #region Members

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static NoteListViewModel _instance;

        /// <summary>
        /// The note collection.
        /// </summary>
        private ObservableCollection<NoteViewModel> _notes = new ObservableCollection<NoteViewModel>();

        /// <summary>
        /// The selected note.
        /// </summary>
        //private NoteViewModel _selectedNote;

        /// <summary>
        /// Indicates whether the data has been loaded.
        /// </summary>
        private bool _isDataLoaded;

        /// <summary>
        /// Indicates whether the current note is unsaved.
        /// </summary>
        private bool _isCurrentNoteUnsaved = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a note list view model instance.
        /// </summary>
        public NoteListViewModel()
        {
            Load();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the note with the given ID.
        /// </summary>
        /// <param name="id">The note ID.</param>
        /// <returns>Returns the note or NULL if the note does not exist.</returns>
        public NoteViewModel GetNoteById(string id)
        {
            foreach (var note in _notes)
            {
                if (note.Id == id)
                    return note;
            }

            return null;
        }

        /// <summary>
        /// Loads the notes data.
        /// </summary>
        private void Load()
        {
            if (_isDataLoaded)
                return;

            var loadedNotes = StorageHelper.LoadSerializedFile<ObservableCollection<NoteViewModel>>("notes.data");

            if (loadedNotes != null)
                Notes = loadedNotes;

            _isDataLoaded = true;
        }

        /// <summary>
        /// Saves the notes data.
        /// </summary>
        public bool Save()
        {
            var res = StorageHelper.SaveAsSerializedFile<ObservableCollection<NoteViewModel>>("notes.data", _notes);
            return res;
        }

        //public void SetSelectedTile

        #endregion

        #region Properties

        /// <summary>
        /// Gets the note view list view model instance.
        /// </summary>
        public static NoteListViewModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NoteListViewModel();
                return _instance;
            }
        }

        /// <summary>
        /// Gets the notes list.
        /// </summary>
        public ObservableCollection<NoteViewModel> Notes
        {
            private set
            {
                if (_notes != value)
                {
                    _notes = value;
                    NotifyPropertyChanged("Notes");
                }
            }
            get
            {
                return _notes;
            }
        }

        /// <summary>
        /// Gets whether the current note is unsaved. This flag is used for newly created notes.
        /// </summary>
        public bool IsCurrentNoteUnsaved
        {
            get
            {
                return _isCurrentNoteUnsaved;
            }
        }

        #endregion
    }
}
