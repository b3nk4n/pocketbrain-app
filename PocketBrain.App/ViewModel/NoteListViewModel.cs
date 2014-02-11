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

namespace PocketBrain.App.ViewModel
{
    /// <summary>
    /// Represents note list view model.
    /// </summary>
    public class NoteListViewModel
    {
        #region Members

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static NoteListViewModel _instance;

        /// <summary>
        /// The note collection.
        /// </summary>
        private IList<NoteViewModel> _notes = new ObservableCollection<NoteViewModel>();

        /// <summary>
        /// The selected note.
        /// </summary>
        private NoteViewModel _selectedNote;

        /// <summary>
        /// Indicates whether the current note is unsaved.
        /// </summary>
        private bool _isCurrentNoteUnsaved = false;

        /// <summary>
        /// The delete command.
        /// </summary>
        private ICommand _deleteSelectedNoteCommand;

        /// <summary>
        /// The save command.
        /// </summary>
        private ICommand _saveSelectedNoteCommand;

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
                    _selectedNote = null;
                }
            },
            () =>
            {
                return IsNoteSelected;
            });

            _saveSelectedNoteCommand = new DelegateCommand(() =>
            {
                if (IsCurrentNoteUnsaved)
                {
                    var note = SelectedNote;
                    if (!string.IsNullOrEmpty(note.Title))
                        Notes.Add(note);
                    else
                        MessageBox.Show("Title vergessen?"); //TODO change text here
                }
            },
            () =>
            {
                return IsNoteSelected;
            });

            // generate test data
            if (Debugger.IsAttached)
            {
                Notes.Add(new NoteViewModel(new Note("Title1", "Content text 1.")));
                Notes.Add(new NoteViewModel(new Note("Title2", "Content text 2. Content text 2.")));
                Notes.Add(new NoteViewModel(new Note("Title3", "Content text 3. Content text 3. Content text 3.")));
                Notes.Add(new NoteViewModel(new Note("Title4", "Content text 4.")));
                Notes.Add(new NoteViewModel(new Note("Title5", "Content text 5.")));
                Notes.Add(new NoteViewModel(new Note("Title6", "Content text 6.")));
                Notes.Add(new NoteViewModel(new Note("Title7", "Content text 7.")));
                Notes.Add(new NoteViewModel(new Note("Title8", "Content text 8.")));
                Notes.Add(new NoteViewModel(new Note("Title9", "Content text 9.")));
            }
        }

        #endregion

        #region Methods

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
        public IList<NoteViewModel> Notes
        {
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
        public ICommand SaveSelectedNoteCommand
        {
            get
            {
                return _saveSelectedNoteCommand;
            }
        }

        #endregion
    }
}
