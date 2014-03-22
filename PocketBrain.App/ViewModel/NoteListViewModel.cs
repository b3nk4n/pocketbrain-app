﻿using PhoneKit.Framework.Core.MVVM;
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
            Notes.Add(note);
            Notes = new ObservableCollection<NoteViewModel>(Notes.OrderByDescending(i => i.DateCreated));
        }

        /// <summary>
        /// Loads the notes data.
        /// </summary>
        protected override void Load()
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

            // select the template to render
            if (_notes.Count == 0)
            {
                if (Settings.LockScreenBackgroundImagePath.Value == null)
                {
                    lockUri = new Uri("/Assets/LockScreenPlaceholder.png", UriKind.Relative);
                    isLocal = false;
                }
                else
                {
                    lockUri = new Uri("/" + Settings.LockScreenBackgroundImagePath.Value, UriKind.Relative);
                    isLocal = true;
                }
            }
            else
            {
                if (_notes.Count == 1)
                {
                    lockGfx = GraphicsHelper.Create(new NoteLockScreen(_notes[0].DisplayedTitle, _notes[0].Content, Settings.LockScreenBackgroundImagePath.Value));
                }

                else if (_notes.Count == 2)
                {
                    lockGfx = GraphicsHelper.Create(
                        new NoteLockScreenDual(
                            _notes[0].DisplayedTitle, _notes[0].Content,
                            _notes[1].DisplayedTitle, _notes[1].Content,
                            Settings.LockScreenBackgroundImagePath.Value));
                }
                else
                {
                    lockGfx = GraphicsHelper.Create(
                        new NoteLockScreenQuad(
                            _notes[0].DisplayedTitle, _notes[0].Content,
                            _notes[1].DisplayedTitle, _notes[1].Content,
                            _notes[2].DisplayedTitle, _notes[2].Content,
                            (_notes.Count == 3) ? string.Empty : _notes[3].DisplayedTitle, (_notes.Count == 3) ? string.Empty : _notes[3].Content,
                            Settings.LockScreenBackgroundImagePath.Value));
                }

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
