using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.Storage;
using PocketBrain.App.Resources;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PocketBrain.App.ViewModel
{
    /// <summary>
    /// The archived notes view model.
    /// </summary>
    class ArchiveListViewModel : ExpandableListViewModelBase
    {
        #region Members

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static ArchiveListViewModel _instance;

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
            : base("archive")
        {
            Load();

            _clearCommand = new DelegateCommand(() =>
                {
                    if (MessageBox.Show(AppResources.MessageBoxClearCheck, AppResources.MessageBoxWarningTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        Clear();
                        UpdateCanExecuteChanged();
                        NotifyIsExtensionButtonVisible();
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
        public void AddNoteToArchive(NoteViewModel note)
        {
            note.DateDeleted = DateTime.Now;
            InsertNote(note);
            _hasDataChanged = true;
        }

        /// <summary>
        /// Clears all archived notes.
        /// </summary>
        public void Clear()
        {
            for (int i = _notes.Count - 1; i >= 0; --i)
            {
                _notes[i].RemoveAttachement();
                RemoveNote(_notes[i]);
            }
            _hasDataChanged = true;
        }

        /// <summary>
        /// Clears all archived notes.
        /// </summary>
        public void ClearNote(NoteViewModel note)
        {
            note.RemoveAttachement();
            RemoveNote(note);
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
        protected override void Load()
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
        public override bool Save()
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
        /// Gets the clear archive command.
        /// </summary>
        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand;
            }
        }

        /// <summary>
        /// Notifies the UI to update the view state.
        /// </summary>
        public void NotifyIsExtensionButtonVisible()
        {
            NotifyPropertyChanged("IsExtensionButtonVisible");
        }

        #endregion
    }
}
