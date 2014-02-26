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
        /// Indicates whether the current note is unsaved.
        /// </summary>
        private bool _isCurrentNoteUnsaved = false;

        /// <summary>
        /// The persistent name of the next lockscreen image to toggle from A to B,
        /// which is required for lockscreen image update.
        /// </summary>
        private StoredObject<string> _nextLockScreenExtension = new StoredObject<string>("nextLockScreenExtension", "A");

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
                WideContent1 = string.Empty,
                WideContent2 = string.Empty,
                WideContent3 = string.Empty,
            };

            if (count > 0)
            {
                tileData.WideContent1 = _notes[0].DisplayedTitle;
                var content = _notes[0].Content;
                if (!string.IsNullOrEmpty(content))
                {
                    var contentFragments = content.Split(new[]{'\n', '\r'});
                    tileData.WideContent2 = contentFragments[0];

                    if (contentFragments.Length > 1)
                        tileData.WideContent3 = contentFragments[2];
                }
                
            }

            LiveTileHelper.UpdateDefaultTile(tileData);
        }

        /// <summary>
        /// Updates the lockscreen image, if the application has access to it.
        /// </summary>
        public void UpdateLockScreen()
        {
            if (!LockScreenHelper.HasAccess() || _notes.Count == 0)
                return;

            WriteableBitmap lockGfx;

            // select the template to render
            if (_notes.Count == 1)
                lockGfx = GraphicsHelper.Create(new NoteLockScreen());
            else
                lockGfx = GraphicsHelper.Create(new NoteLockScreenDual());

            // render lock image
            var nextExtension = _nextLockScreenExtension.Value;
            var lockUri = StorageHelper.SaveJpeg(string.Format("/locknote_{0}.jpg", nextExtension), lockGfx);
            _nextLockScreenExtension.Value = (nextExtension == "A") ? "B" : "A";

            // set lockscreen image
            LockScreenHelper.SetLockScreenImage(lockUri, true);
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
