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
using Microsoft.Phone.Shell;
using PocketBrain.App.Resources;
using PhoneKit.Framework.Core.Tile;
using PocketBrain.App.Controls;
using PhoneKit.Framework.Core.Graphics;
using System.Windows.Media.Imaging;
using PhoneKit.Framework.Core.LockScreen;
using System.Windows.Media;

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
        /// Indicates whether the data has been loaded.
        /// </summary>
        private bool _isDataLoaded;

        /// <summary>
        /// The current note.
        /// </summary>
        private NoteViewModel _currentNote;

        /// <summary>
        /// The persistent name of the next lockscreen image to toggle from A to B,
        /// which is required for lockscreen image update.
        /// </summary>
        private StoredObject<string> _nextLockScreenExtension = new StoredObject<string>("nextLockScreenExtension", "A");

        /// <summary>
        /// The lockscreen command.
        /// </summary>
        private DelegateCommand _lockScreenCommand;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a note list view model instance.
        /// </summary>
        public NoteListViewModel()
        {
            Load();

            _lockScreenCommand = new DelegateCommand(async () =>
            {
                if (await LockScreenHelper.VerifyAccessAsync())
                {
                    NotifyPropertyChanged("HasLockScreenAccess");
                }
            }, () =>
            {
                return !HasLockScreenAccess;
            });
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

        /// <summary>
        /// Updates the primary tile.
        /// </summary>
        public void UpdatePrimaryTile()
        {
            var count = _notes.Count;

            var tileData = new IconicTileData
            {
                Count = count,
                Title = AppResources.ApplicationTitle,
            };

            tileData.WideContent1 = (count > 0) ? _notes[0].DisplayedTitle : string.Empty;
            tileData.WideContent2 = (count > 1) ? _notes[1].DisplayedTitle : string.Empty;
            tileData.WideContent3 = (count > 2) ? _notes[2].DisplayedTitle : string.Empty;

            LiveTileHelper.UpdateDefaultTile(tileData);
        }

        /// <summary>
        /// Updates the lockscreen image, if the application has access to it.
        /// </summary>
        public void UpdateLockScreen()
        {
            if (!LockScreenHelper.HasAccess())
                return;

            WriteableBitmap lockGfx;
            Uri lockUri;
            bool isLocal;

            // select the template to render
            if (_notes.Count == 0)
            {
                lockUri = new Uri("/Assets/LockScreenPlaceholder.png", UriKind.Relative);
                isLocal = false;
            }
            else
            {
                if (_notes.Count == 1)
                    lockGfx = GraphicsHelper.Create(new NoteLockScreen(_notes[0].DisplayedTitle, _notes[0].Content));
                else
                    lockGfx = GraphicsHelper.Create(new NoteLockScreenDual(_notes[0].DisplayedTitle, _notes[0].Content, _notes[1].DisplayedTitle, _notes[1].Content));

                // render lock image
                var nextExtension = _nextLockScreenExtension.Value;
                lockUri = StorageHelper.SaveJpeg(string.Format("/locknote_{0}.jpg", nextExtension), lockGfx);
                isLocal = true;
                _nextLockScreenExtension.Value = (nextExtension == "A") ? "B" : "A";
            }

            // set lockscreen image
            LockScreenHelper.SetLockScreenImage(lockUri, isLocal);
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
        /// Gets the current active note.
        /// </summary>
        public NoteViewModel CurrentNote
        {
            get
            {
                return _currentNote;
            }
            set
            {
                _currentNote = value;
            }
        }

        /// <summary>
        /// Gets the lock screen command.
        /// </summary>
        public ICommand LockScreenCommand
        {
            get
            {
                return _lockScreenCommand;
            }
        }

        /// <summary>
        /// Gets whether the application has lockscreen access.
        /// </summary>
        public bool HasLockScreenAccess
        {
            get
            {
                return LockScreenHelper.HasAccess();
            }
        }

        #endregion
    }
}
