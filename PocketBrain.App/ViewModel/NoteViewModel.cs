using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using PhoneKit.Framework.Core.Graphics;
using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.Storage;
using PhoneKit.Framework.Core.Tile;
using PhoneKit.Framework.Tile;
using PocketBrain.App.Controls;
using PocketBrain.App.Model;
using System;
using System.IO;
using System.Threading.Tasks;
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
        /// The random number generator to generate unique file names.
        /// </summary>
        private Random _random = new Random();

        /// <summary>
        /// The wide tile path of the rendered image.
        /// </summary>
        private readonly string _tileWidePath;

        /// <summary>
        /// The normal tile path of the rendered image.
        /// </summary>
        private readonly string _tileNormalPath;

        /// <summary>
        /// The delete command.
        /// </summary>
        private DelegateCommand _deleteCommand;

        /// <summary>
        /// The removes the attachement command.
        /// </summary>
        private DelegateCommand _removeAttachementCommand;

        /// <summary>
        /// The add attachement command.
        /// </summary>
        private DelegateCommand _addAttachementCommand;

        /// <summary>
        /// The pin to start command for a single note.
        /// </summary>
        private DelegateCommand _pinToStartCommand;

        /// <summary>
        /// The unpin from start command for a single note.
        /// </summary>
        private DelegateCommand _unpinFromStartCommand;
 
        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty note.
        /// </summary>
        public NoteViewModel()
            : this(new Note())
        {
        }

        /// <summary>
        /// Creates a note.
        /// </summary>
        /// <param name="container">The container of the note.</param>
        /// <param name="note">The note.</param>
        public NoteViewModel(Note note)
        {
            _note = note;

            _deleteCommand = new DelegateCommand(() =>
                {
                    RemoveAttachement();
                    UnpinTile();

                    // delte the item in the list if it was saved before
                    if (NoteListViewModel.Instance.Notes.Contains(this))
                        NoteListViewModel.Instance.Notes.Remove(this);
                });

            _removeAttachementCommand = new DelegateCommand(() =>
                {
                    if (_note.HasAttachement)
                    {
                        RemoveAttachement();
                    }
                },
                () => {
                    return _note.HasAttachement;
                });

            _addAttachementCommand = new DelegateCommand(() =>
                {
                    var task = new PhotoChooserTask();
                    task.ShowCamera = true;
                    task.Completed += (se, pr) =>
                    {
                        if (pr.Error != null || pr.TaskResult != TaskResult.OK)
                            return;

                        // save a copy in local storage
                        string filePath = GetUniqueLocalFilePathOfFile(pr.OriginalFileName);
                        if (StorageHelper.SaveFileFromStream(filePath, pr.ChosenPhoto))
                        {
                            SetAttachement(filePath);
                        }
                    };
                    task.Show();
                },
                () =>
                {
                    return !_note.HasAttachement;
                });

            _pinToStartCommand = new DelegateCommand(() =>
                {
                    PinOrUpdateTile();
                    UpdateCanExecuteChanged();
                    NotifyPropertyChanged("CanPinToStart");
                },
                () =>
                {
                    return CanPinToStart;
                });

            _unpinFromStartCommand = new DelegateCommand(() =>
                {
                    UnpinTile();
                    UpdateCanExecuteChanged();
                    NotifyPropertyChanged("CanPinToStart");
                },
                () =>
                {
                    return !CanPinToStart;
                });

            // init the paths for the rendred tile images
            _tileWidePath = LiveTileHelper.SHARED_SHELL_CONTENT_PATH + string.Format("livetile_wide_{0}.jpeg", Id);
            _tileNormalPath = LiveTileHelper.SHARED_SHELL_CONTENT_PATH + string.Format("livetile_normal_{0}.jpeg", Id);
        }

        #endregion

        /// <summary>
        /// Gets the unique local file name for the given file to copy.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The unique file name in isolated storage.</returns>
        public string GetUniqueLocalFilePathOfFile(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            return string.Format(LiveTileHelper.SHARED_SHELL_CONTENT_PATH + "attachements/{0:000000}_{1}", _random.Next(0, 1000000), fileInfo.Name);
        }

        /// <summary>
        /// Sets the new attachement image and notifies the UI and command manager.
        /// </summary>
        /// <param name="filePath">The file path of the attachement.</param>
        private void SetAttachement(string filePath)
        {
            _note.AttachedImagePath = filePath;
            NotifyPropertyChanged("HasAttachement");

            UpdateCanExecuteChanged();
        }

        /// <summary>
        /// Remvoes the attached image and notifies the UI and command manager.
        /// </summary>
        private void RemoveAttachement()
        {
            _note.RemoveAttachement();
            NotifyPropertyChanged("HasAttachement");

            UpdateCanExecuteChanged();
        }

        /// <summary>
        /// Updates the command manager to test the canExecute predicates.
        /// </summary>
        private void UpdateCanExecuteChanged()
        {
            _removeAttachementCommand.RaiseCanExecuteChanged();
            _addAttachementCommand.RaiseCanExecuteChanged();
            _pinToStartCommand.RaiseCanExecuteChanged();
            _unpinFromStartCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Pins or updates a single note to the start page.
        /// </summary>
        private void PinOrUpdateTile()
        {
            var noteWideGfx = GraphicsHelper.Create(new NoteWideTile(DisplayedTitle, Content));
            var noteWideUri = StorageHelper.SaveJpeg(_tileWidePath, noteWideGfx);
            var noteNormalGfx = GraphicsHelper.Create(new NoteNormalTile(DisplayedTitle, Content));
            var noteNormalUri = StorageHelper.SaveJpeg(_tileNormalPath, noteNormalGfx);

            if (HasAttachement)
            {
                var imageUri = new Uri(StorageHelper.ISTORAGE_SCHEME + AttachedImagePath, UriKind.Absolute);

                var tile = new FlipTileData
                {
                    BackBackgroundImage = imageUri,
                    WideBackBackgroundImage = imageUri,
                    SmallBackgroundImage = imageUri,
                    WideBackgroundImage = noteWideUri,
                    BackgroundImage = noteNormalUri,
                };

                LiveTilePinningHelper.PinOrUpdateTile(NavigationUri, tile);
            }
            else
            {
                var imageUri = new Uri("/Assets/Tiles/FlipCycleTileSmall.png", UriKind.Relative);

                var tile = new FlipTileData
                {
                    SmallBackgroundImage = imageUri,
                    WideBackgroundImage = noteWideUri,
                    BackgroundImage = noteNormalUri
                };

                LiveTilePinningHelper.PinOrUpdateTile(NavigationUri, tile);
            }
            NotifyPropertyChanged("CanPinToStart");
        }

        /// <summary>
        /// Updates the secondary live tile, if one exists.
        /// </summary>
        public void UpdateTile()
        {
            // update tile image
            if (LiveTileHelper.TileExists(NavigationUri))
                PinOrUpdateTile();
        }

        /// <summary>
        /// Unpins the secondary tile from start if one exists.
        /// </summary>
        public void UnpinTile()
        {
            ClearTileImages();
            LiveTileHelper.RemoveTile(NavigationUri);
        }

        /// <summary>
        /// Clears all the generated tile images when these exist.
        /// </summary>
        private void ClearTileImages()
        {
            // delete file in background task
            Task.Run(() =>
            {
                StorageHelper.DeleteFile(_tileWidePath);
                StorageHelper.DeleteFile(_tileWidePath);
            });
        }

        #region Properties

        /// <summary>
        /// Gets the note ID.
        /// </summary>
        /// <remarks>
        /// Public set for JSON deserialization.
        /// </remarks>
        public string Id
        {
            get
            {
                return _note.Id;
            }
            set
            {
                _note.Id = value;
            }
        }

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
                    NotifyPropertyChanged("DisplayedTitle");
                }
            }
        }

        /// <summary>
        /// Gets the displayed title.
        /// </summary>
        public string DisplayedTitle
        {
            get
            {
                return string.IsNullOrEmpty(_note.Title) ? "Untitled" : _note.Title;
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
                    NotifyPropertyChanged("HasAttachement");
                }
            }
        }

        /// <summary>
        /// Gets whether the note has an attachement.
        /// </summary>
        public bool HasAttachement
        {
            get
            {
                return _note.HasAttachement;
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
        /// Gets the navigation URI.
        /// </summary>
        public Uri NavigationUri
        {
            get
            {
                return new Uri("/NotePage.xaml?id=" + _note.Id, UriKind.Relative);
            }
        }

        /// <summary>
        /// Indicates whether the tile can be pinned to start or whether the note is already pinned
        /// </summary>
        public bool CanPinToStart
        {
            get
            {
                return !LiveTileHelper.TileExists(NavigationUri);
            }
        }

        /// <summary>
        /// Gets whether the note is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(Content) || HasAttachement;
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

        /// <summary>
        /// Gets the remove attachement command.
        /// </summary>
        public ICommand RemoveAttachementCommand
        {
            get
            {
                return _removeAttachementCommand;
            }
        }

        /// <summary>
        /// Gets the remove attachement command.
        /// </summary>
        public ICommand AddAttachementCommand
        {
            get
            {
                return _addAttachementCommand;
            }
        }

        /// <summary>
        /// Gets the pin to start command.
        /// </summary>
        public ICommand PinToStartCommand
        {
            get
            {
                return _pinToStartCommand;
            }
        }

        /// <summary>
        /// Gets the unpin from start command.
        /// </summary>
        public ICommand UnpinFromStartCommand
        {
            get
            {
                return _unpinFromStartCommand;
            }
        }

        #endregion
    }
}
