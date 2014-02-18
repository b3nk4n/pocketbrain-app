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
        private NoteViewModel _selectedNote;

        /// <summary>
        /// Indicates whether the data has been loaded.
        /// </summary>
        private bool _isDataLoaded;

        /// <summary>
        /// Indicates whether the current note is unsaved.
        /// </summary>
        private bool _isCurrentNoteUnsaved = false;

        /// <summary>
        /// The delete command.
        /// </summary>
        private ICommand _deleteSelectedNoteCommand;

        /// <summary>
        /// The add note command.
        /// </summary>
        private ICommand _addNoteCommand;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a note list view model instance.
        /// </summary>
        public NoteListViewModel()
        {
            _deleteSelectedNoteCommand = new DelegateCommand(() =>
            {
                if (IsNoteSelected)
                {
                    _notes.Remove(_selectedNote);
                    SelectedNote = null;
                }
            },
            () =>
            {
                return IsNoteSelected;
            });

            _addNoteCommand = new DelegateCommand(() =>
            {
                var note = new NoteViewModel(_notes, new Note("Untitled", "..."));
                Notes.Insert(0, note);
                SelectedNote = note;
            });

            Load();
        }

        #endregion

        #region Methods

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
            return StorageHelper.SaveAsSerializedFile<IList<NoteViewModel>>("notes.data", _notes);
        }

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
        /// Gets the selected note.
        /// </summary>
        public NoteViewModel SelectedNote
        {
            set
            {
                _selectedNote = value;
            }
            get
            {
                return _selectedNote;
            }
        }

        /// <summary>
        /// Gets whether a note is selected.
        /// </summary>
        public bool IsNoteSelected
        {
            get
            {
                return _selectedNote != null;
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

        /// <summary>
        /// The delete selected note command.
        /// </summary>
        public ICommand DeleteSelectedNoteCommand
        {
            get
            {
                return _deleteSelectedNoteCommand;
            }
        }

        /// <summary>
        /// The save selected note command.
        /// </summary>
        public ICommand AddNoteCommand
        {
            get
            {
                return _addNoteCommand;
            }
        }

        #endregion
    }
}
