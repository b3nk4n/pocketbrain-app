using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.Storage;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PocketBrain.App.ViewModel
{
    /// <summary>
    /// The archived notes view model.
    /// </summary>
    class ArchiveListViewModel : ViewModelBase
    {
        #region Members

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static ArchiveListViewModel _instance;

        /// <summary>
        /// The note collection.
        /// </summary>
        private ObservableCollection<NoteViewModel> _notes = new ObservableCollection<NoteViewModel>();

        /// <summary>
        /// Indicates whether the data has been loaded.
        /// </summary>
        private static bool _isDataLoaded;

        /// <summary>
        /// Indicates whether the archive data has changed.
        /// </summary>
        /// <remarks>
        /// As an performance optimization to not save the archive data when nothing has changed.
        /// </remarks>
        private bool _hasDataChanged = false;

        /// <summary>
        /// The clear all notes from archive command.
        /// </summary>
        private DelegateCommand _clearCommand;

        #endregion

        /// <summary>
        /// Creates a ArchiveListViewModel instance.
        /// </summary>
        #region Constructors

        public ArchiveListViewModel()
        {
            Load();

            _clearCommand = new DelegateCommand(() =>
                {
                    if (MessageBox.Show("Do you really want to clear the archive?", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        Clear();
                        UpdateCanExecuteChanged();
                    }
                }, () =>
                {
                    return _notes.Count > 0;
                });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a note to the archive.
        /// </summary>
        /// <param name="note">The note to archive.</param>
        public void AddNote(NoteViewModel note)
        {
            _notes.Add(note);
            _hasDataChanged = true;
        }

        /// <summary>
        /// Clears all archived notes.
        /// </summary>
        public void Clear()
        {
            _notes.Clear();
            _hasDataChanged = true;
        }

        /// <summary>
        /// Updates the command manager to test the canExecute predicates.
        /// </summary>
        private void UpdateCanExecuteChanged()
        {
            _clearCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Loads the achived notes data.
        /// </summary>
        private void Load()
        {
            if (_isDataLoaded)
                return;

            var loadedNotes = StorageHelper.LoadSerializedFile<ObservableCollection<NoteViewModel>>("archive.data");

            if (loadedNotes != null)
                Notes = loadedNotes;

            _isDataLoaded = true;
        }

        /// <summary>
        /// Saves the notes archive data.
        /// </summary>
        public bool Save()
        {
            if (!_hasDataChanged)
                return true;

            var res = StorageHelper.SaveAsSerializedFile<ObservableCollection<NoteViewModel>>("archive.data", _notes);
            return res;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the note view list view model instance.
        /// </summary>
        public static ArchiveListViewModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ArchiveListViewModel();
                return _instance;
            }
        }

        /// <summary>
        /// Gets wheter the data has been loaded.
        /// </summary>
        /// <remarks>
        /// Used to not unnecessarily load the data when nothing has changed one exit.
        /// </remarks>
        public static bool IsLoaded
        {
            get
            {
                return _isDataLoaded;
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
        /// Gets the clear archive command.
        /// </summary>
        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand;
            }
        }

        #endregion
    }
}
