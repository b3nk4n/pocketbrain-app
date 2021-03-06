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
using PhoneKit.Framework.Tile;
using PhoneKit.Framework.Core.Themeing;
using PhoneKit.Framework.InAppPurchase;

namespace PocketBrain.App.ViewModel
{
    /// <summary>
    /// Represents note list view model.
    /// </summary>
    public class NoteListViewModel : ExpandableListViewModelBase
    {
        #region Members

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static NoteListViewModel _instance;

        /// <summary>
        /// The current note.
        /// </summary>
        private NoteViewModel _currentNote;

        /// <summary>
        /// Indicates whether the data has been loaded.
        /// </summary>
        private static bool _isDataLoaded;

        /// <summary>
        /// The persistent name of the next lockscreen image to toggle from A to B,
        /// which is required for lockscreen image update.
        /// </summary>
        private StoredObject<string> _nextLockScreenExtension = new StoredObject<string>("nextLockScreenExtension", "A");

        /// <summary>
        /// The lockscreen command.
        /// </summary>
        private DelegateCommand _lockScreenCommand;

        /// <summary>
        /// The pin to start command for a single note.
        /// </summary>
        private DelegateCommand _pinAddNoteToStartCommand;

        /// <summary>
        /// The unpin from start command for a single note.
        /// </summary>
        private DelegateCommand _unpinAddNoteFromStartCommand;

        /// <summary>
        /// The add note URI.
        /// </summary>
        private readonly Uri ADD_NOTE_URI = new Uri("/NotePage.xaml", UriKind.Relative);

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a note list view model instance.
        /// </summary>
        public NoteListViewModel()
            : base("notes")
        {
            Load();

            _lockScreenCommand = new DelegateCommand(async () =>
            {
                try // jsut to make sure, because idk if the verify access method could fail
                {
                    if (await LockScreenHelper.VerifyAccessAsync())
                    {
                        NotifyPropertyChanged("HasLockScreenAccess");
                    }
                }
                catch (Exception) { }
            }, () =>
            {
                return !HasLockScreenAccess;
            });

            _pinAddNoteToStartCommand = new DelegateCommand(() =>
            {
                PinOrUpdateAddNoteTile();
                UpdateCanExecuteChanged();
            },
                () =>
                {
                    return CanAddNotePinToStart;
                });

            _unpinAddNoteFromStartCommand = new DelegateCommand(() =>
            {
                UnpinAddNoteTile();
                UpdateCanExecuteChanged();
            },
                () =>
                {
                    return !CanAddNotePinToStart;
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
        /// Restores a note at the right position by checking the creation date.
        /// </summary>
        /// <param name="note">The note to restore.</param>
        public void Restore(NoteViewModel note)
        {
            InsertNote(note);
        }

        public override void Load(bool forceReload = false)
        {
            if (_isDataLoaded && !forceReload)
                return;

            var loadedNotes = StorageHelper.LoadSerializedFile<ObservableCollection<NoteViewModel>>("notes.data");

            if (loadedNotes != null)
                Notes = loadedNotes;

            _isDataLoaded = true;
        }

        /// <summary>
        /// Saves the notes data.
        /// </summary>
        public override bool Save()
        {
            var res = StorageHelper.SaveAsSerializedFile<ObservableCollection<NoteViewModel>>("notes.data", _notes);
            return res;
        }

        /// <summary>
        /// Updates the command manager to test the canExecute predicates.
        /// </summary>
        private void UpdateCanExecuteChanged()
        {
            _pinAddNoteToStartCommand.RaiseCanExecuteChanged();
            _unpinAddNoteFromStartCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Updates the primary tile.
        /// </summary>
        public void UpdatePrimaryTile()
        {
            int count = 0;
            
            if (Settings.ShowNoteCountOnLiveTile.Value == "1")
                count = _notes.Count; // the real total count here

            var tileData = new IconicTileData
            {
                Count = count,
                Title = AppResources.ApplicationTitle,
            };

            // filter
            IList<NoteViewModel> tileList = new List<NoteViewModel>(8);
            foreach (var note in _notes)
            {
                if (note.IsValidTextAndVisible)
                    tileList.Add(note);
            }

            string[] contentLines = {"", "", ""};
            int currentIndex = 0;

            for (int i = 0; i < tileList.Count; ++i)
            {
                if (currentIndex >= contentLines.Length)
                    break;

                contentLines[currentIndex++] = (string.IsNullOrWhiteSpace(tileList[i].Title)) ? tileList[i].Content : tileList[i].Title;
            }

            tileData.WideContent1 = contentLines[0];
            tileData.WideContent2 = contentLines[1];
            tileData.WideContent3 = contentLines[2];

            LiveTileHelper.UpdateDefaultTile(tileData);
        }

        /// <summary>
        /// Pins or updates a single note to the start page.
        /// </summary>
        private void PinOrUpdateAddNoteTile()
        {
            var tile = new StandardTileData
            {
                BackgroundImage = new Uri("/Assets/AddNoteIcon.png", UriKind.Relative)
            };

            LiveTilePinningHelper.PinOrUpdateTile(ADD_NOTE_URI, tile);

            NotifyPropertyChanged("CanAddNotePinToStart");
        }

        /// <summary>
        /// Unpins the secondary tile from start if one exists.
        /// </summary>
        public void UnpinAddNoteTile()
        {
            LiveTileHelper.RemoveTile(ADD_NOTE_URI);
            NotifyPropertyChanged("CanAddNotePinToStart");
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

            // filter
            IList<NoteViewModel> lockList = new List<NoteViewModel>(8);
            foreach (var note in _notes)
            {
                if (note.IsValidTextAndVisible)
                    lockList.Add(note);
            }

            int displayItemsCount = Math.Min(lockList.Count, int.Parse(Settings.MaximumLockItems.Value));

            // select the template to render
            if (displayItemsCount == 0 && Settings.LockScreenBackgroundImagePath.Value == null)
            {
                lockUri = new Uri("/Assets/LockScreenPlaceholder.png", UriKind.Relative);
                isLocal = false;
            }
            else
            {
                // ugly workaround
                if (displayItemsCount == 0)
                {
                    lockGfx = GraphicsHelper.Create(new NoteLockScreen(string.Empty, string.Empty, Settings.LockScreenBackgroundImagePath.Value));
                }

                else if (displayItemsCount == 1)
                {
                    lockGfx = GraphicsHelper.Create(new NoteLockScreen(lockList[0].Title, lockList[0].Content, Settings.LockScreenBackgroundImagePath.Value));
                }

                else if (displayItemsCount == 2)
                {
                    lockGfx = GraphicsHelper.Create(
                        new NoteLockScreenDual(
                            lockList[0].Title, lockList[0].Content,
                            lockList[1].Title, lockList[1].Content,
                            Settings.LockScreenBackgroundImagePath.Value));
                }
                else if (displayItemsCount == 3 || displayItemsCount == 4)
                {
                    lockGfx = GraphicsHelper.Create(
                        new NoteLockScreenQuad(
                            lockList[0].Title, lockList[0].Content,
                            lockList[1].Title, lockList[1].Content,
                            lockList[2].Title, lockList[2].Content,
                            (lockList.Count == 3) ? string.Empty : lockList[3].Title, (lockList.Count == 3) ? string.Empty : lockList[3].Content,
                            Settings.LockScreenBackgroundImagePath.Value));
                }
                else
                {
                    lockGfx = GraphicsHelper.Create(
                        new NoteLockScreenSix(
                            lockList[0].Title, lockList[0].Content,
                            lockList[1].Title, lockList[1].Content,
                            lockList[2].Title, lockList[2].Content,
                            lockList[3].Title, lockList[3].Content,
                            lockList[4].Title, lockList[4].Content,
                            (lockList.Count == 5) ? string.Empty : lockList[5].Title, (lockList.Count == 5) ? string.Empty : lockList[5].Content,
                            Settings.LockScreenBackgroundImagePath.Value));
                }

                // render lock image
                var nextExtension = _nextLockScreenExtension.Value;
                lockUri = StorageHelper.SaveJpeg(string.Format("/locknote_{0}.jpg", nextExtension), lockGfx);
                GraphicsHelper.CleanUpMemory(lockGfx);
                isLocal = true;

                // A,B,C. We added C, because it was not working when we saved twice (A->B->A => no lockscreen update)
                _nextLockScreenExtension.Value = (nextExtension == "A") ? "B" : (nextExtension == "B") ? "C": "A";
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
        /// Indicates whether the tile can be pinned to start or whether the note is already pinned
        /// </summary>
        public bool CanAddNotePinToStart
        {
            get
            {
                return !LiveTileHelper.TileExists(ADD_NOTE_URI);
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
        /// Gets the pin to start command.
        /// </summary>
        public ICommand PinAddNoteToStartCommand
        {
            get
            {
                return _pinAddNoteToStartCommand;
            }
        }

        /// <summary>
        /// Gets the unpin from start command.
        /// </summary>
        public ICommand UnpinAddNoteFromStartCommand
        {
            get
            {
                return _unpinAddNoteFromStartCommand;
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

        public void UpdateHasProVersion()
        {
            NotifyPropertyChanged("HasProVersion");
        }

        /// <summary>
        /// Gets whether the application is a pro version.
        /// </summary>
        public bool HasProVersion
        {
            get
            {
#if DEBUG
                return true;
#else
                return InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_KEY);
#endif
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
