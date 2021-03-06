using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using PhoneKit.Framework.Core.Graphics;
using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.Themeing;
using PhoneKit.Framework.Core.Storage;
using PhoneKit.Framework.Core.Tile;
using PhoneKit.Framework.Tile;
using PhoneKit.Framework.Voice;
using PocketBrain.App.Controls;
using PocketBrain.App.Model;
using PocketBrain.App.Resources;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xna.Framework.Media;
using PocketBrain.App.Helpers;
using System.Windows.Media.Imaging;
using PhoneKit.Framework.Core.LockScreen;

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
        /// The delete command (moves it to the archive page).
        /// </summary>
        private DelegateCommand _deleteCommand;

        /// <summary>
        /// Clears the note (from archive page incl. delete the attachment)
        /// </summary>
        private DelegateCommand _deleteFromArchiveCommand;

        /// <summary>
        /// The restore from archive command.
        /// </summary>
        private DelegateCommand _restoreCommand;

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

        /// <summary>
        /// The sharing command.
        /// </summary>
        private DelegateCommand _shareCommand;

        /// <summary>
        /// The append text command.
        /// </summary>
        private DelegateCommand _speakPrependTextCommand;

        /// <summary>
        /// The append text command.
        /// </summary>
        private DelegateCommand _speakReplaceTextCommand;

        /// <summary>
        /// The append text command.
        /// </summary>
        private DelegateCommand _speakAppendTextCommand;

        /// <summary>
        /// The hide note command.
        /// </summary>
        private DelegateCommand _hideCommand;

        /// <summary>
        /// The show note command.
        /// </summary>
        private DelegateCommand _showCommand;

        /// <summary>
        /// The photo chooser task.
        /// </summary>
        /// <remarks>Must be defined at class level to work properly in tombstoning.</remarks>
        private PhotoChooserTask _photoTask = new PhotoChooserTask();
 
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
                    UnpinTile();

                    // delete the item in the list if it was saved before
                    if (NoteListViewModel.Instance.Notes.Contains(this))
                    {
                        ArchiveListViewModel.Instance.AddNoteToArchive(this);
                        NoteListViewModel.Instance.RemoveNote(this);
                    }

                    // clear the current note
                    NoteListViewModel.Instance.CurrentNote = null;

                    NoteListViewModel.Instance.NotifyIsExtensionButtonVisible();

                    if (NoteListViewModel.Instance.Notes.Count == 1)
                    {
                        MainPage.ScrollToTopOnNextNavigationTo = true;
                    }
                });

            _deleteFromArchiveCommand = new DelegateCommand(() =>
            {
                UnpinTile();

                // delete the item in the list if it was saved before
                if (ArchiveListViewModel.Instance.Notes.Contains(this))
                {
                    ArchiveListViewModel.Instance.ClearNote(this);
                }

                ArchiveListViewModel.Instance.NotifyIsExtensionButtonVisible();
            });

            _restoreCommand = new DelegateCommand(() =>
                {
                    NoteListViewModel.Instance.Restore(this);
                    ArchiveListViewModel.Instance.RemoveNote(this);
                    ArchiveListViewModel.Instance.NotifyIsExtensionButtonVisible();
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
                    try
                    {
                        _photoTask.Show();
                    }
                    catch(Exception)
                    {
                        // supress multiple Show() call error.
                    }
                },
                () =>
                {
                    return !_note.HasAttachement;
                });

            _pinToStartCommand = new DelegateCommand(() =>
                {
                    PinOrUpdateTile();
                    UpdateCanExecuteChanged();
                },
                () =>
                {
                    return CanPinToStart;
                });

            _unpinFromStartCommand = new DelegateCommand(() =>
                {
                    UnpinTile();
                    UpdateCanExecuteChanged();
                },
                () =>
                {
                    return !CanPinToStart;
                });

            _shareCommand = new DelegateCommand(() =>
                {
                    ShareStatusTask shareTask = new ShareStatusTask();

                    if (string.IsNullOrWhiteSpace(Title))
                    {
                        shareTask.Status = string.Format("{0}", Content);
                    }
                    else
                    {
                        shareTask.Status = string.Format("{0}\r\r{1}", Title, Content);
                    }
                   
                    shareTask.Show();
                });

            _speakPrependTextCommand = new DelegateCommand(async () =>
                {
                    try
                    {
                        //if (Speech.Instance.HasRecognizerUIError)
                        //    return;

                        // enforce to instantiate the recoginizer UI
                        var recognizer = Speech.Instance.RecognizerUI;

                        if (Speech.Instance.HasRecognizerUI)
                        {
                            recognizer.Settings.ReadoutEnabled = false;
                            recognizer.Settings.ShowConfirmation = false;
                            var result = await recognizer.RecognizeWithUIAsync();

                            if (result.ResultStatus == Windows.Phone.Speech.Recognition.SpeechRecognitionUIStatus.Succeeded)
                            {
                                string text = result.RecognitionResult.Text;

                                if (!string.IsNullOrEmpty(text))
                                {
                                    Content = string.Format("{0}\r{1}", AntiProfanityFilter(text), Content);
                                }
                            }
                        }
                        else
                        {
                            // show no rec UI warning message
                            MessageBox.Show(AppResources.MessageBoxNoRecUI, AppResources.MessageBoxWarningTitle, MessageBoxButton.OK);
                        }
                    }
                    catch (Exception) { }
                });

            _speakReplaceTextCommand = new DelegateCommand(async () =>
                {
                    try
                    {
                        //if (Speech.Instance.HasRecognizerUIError)
                        //    return;

                        // enforce to instantiate the recoginizer UI
                        var recognizer = Speech.Instance.RecognizerUI;

                        if (Speech.Instance.HasRecognizerUI)
                        {
                            recognizer.Settings.ReadoutEnabled = false;
                            recognizer.Settings.ShowConfirmation = false;
                            var result = await recognizer.RecognizeWithUIAsync();

                            if (result.ResultStatus == Windows.Phone.Speech.Recognition.SpeechRecognitionUIStatus.Succeeded)
                            {
                                string text = result.RecognitionResult.Text;

                                if (!string.IsNullOrEmpty(text))
                                {
                                    Content = AntiProfanityFilter(text);
                                }
                            }
                        }
                        else
                        {
                            // show no rec UI warning message
                            MessageBox.Show(AppResources.MessageBoxNoRecUI, AppResources.MessageBoxWarningTitle, MessageBoxButton.OK);
                        }
                    }
                    catch (Exception) { }
                });

            _speakAppendTextCommand = new DelegateCommand(async () =>
                {
                    try
                    {
                        //if (Speech.Instance.HasRecognizerUIError)
                        //    return;

                        // enforce to instantiate the recoginizer UI
                        var recognizer = Speech.Instance.RecognizerUI;

                        if (Speech.Instance.HasRecognizerUI)
                        {
                            recognizer.Settings.ReadoutEnabled = false;
                            recognizer.Settings.ShowConfirmation = false;
                            var result = await recognizer.RecognizeWithUIAsync();

                            if (result.ResultStatus == Windows.Phone.Speech.Recognition.SpeechRecognitionUIStatus.Succeeded)
                            {
                                string text = result.RecognitionResult.Text;

                                if (!string.IsNullOrEmpty(text))
                                {
                                    Content += "\r" + AntiProfanityFilter(text);
                                }
                            }
                        }
                        else
                        {
                            // show no rec UI warning message
                            MessageBox.Show(AppResources.MessageBoxNoRecUI, AppResources.MessageBoxWarningTitle, MessageBoxButton.OK);
                        }
                    }
                    catch (Exception) { }
                });

            _showCommand = new DelegateCommand(() =>
                {
                    IsHidden = false;
                    UpdateCanExecuteChanged();
                },
                () =>
                {
                    return IsHidden;
                });

            _hideCommand = new DelegateCommand(() =>
                {
                    IsHidden = true;
                    UpdateCanExecuteChanged();
                },
                () =>
                {
                    return !IsHidden;
                });

            // init photo chooser task
            _photoTask.ShowCamera = true;
            _photoTask.Completed += (se, pr) =>
                {
                    if (pr.Error != null || pr.TaskResult != TaskResult.OK)
                        return;
                    // save a copy in local storage
                    FileInfo fileInfo = new FileInfo(pr.OriginalFileName);
                    string filePath = GetUniqueLocalFilePathOfFile(pr.OriginalFileName);

                    var image = StaticMediaLibrary.GetImageFromFileName(fileInfo.Name);
                    if (image != null && StorageHelper.SaveJpeg(filePath, image.PreviewImage as WriteableBitmap) != null)
                    {
                        SetAttachement(filePath);
                    }
                };      
        }

        #endregion

        /// <summary>
        /// Sets an attachement image from the library.
        /// </summary>
        /// <param name="medialLibIndex">The library image.</param>
        public void SetAttachementFromMediaLibraryIndex(int medialLibIndex)
        {
            var mediaLibrary = new MediaLibrary();

            // verify index in range
            if (medialLibIndex >= 0 && medialLibIndex < mediaLibrary.Pictures.Count)
            {
                var pic = mediaLibrary.Pictures[medialLibIndex];
                string filePath = GetUniqueLocalFilePathOfFile(pic.Name);
                if (StorageHelper.SaveFileFromStream(filePath, pic.GetImage()))
                {
                    SetAttachement(filePath);
                }
            }
        }

        /// <summary>
        /// Anti-Profanity-Filter.
        /// </summary>
        /// <param name="text">The text to un-filter.</param>
        /// <returns>The pure text.</returns>
        private string AntiProfanityFilter(string text)
        {
            return text.Replace("<profanity>", string.Empty)
                .Replace("</profanity>", string.Empty);
        }

        /// <summary>
        /// Gets the unique local file name for the given file to copy.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The unique file name in isolated storage.</returns>
        public string GetUniqueLocalFilePathOfFile(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            return string.Format(LiveTileHelper.SHARED_SHELL_CONTENT_PATH + "attachements/{0}_{1}", Id, fileInfo.Name);
        }

        /// <summary>
        /// Remvoes the attached image and notifies the UI and command manager.
        /// </summary>
        public void RemoveAttachement()
        {
            _note.RemoveAttachement();
            NotifyPropertyChanged("HasAttachement");

            UpdateCanExecuteChanged();
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
        /// Updates the command manager to test the canExecute predicates.
        /// </summary>
        private void UpdateCanExecuteChanged()
        {
            _removeAttachementCommand.RaiseCanExecuteChanged();
            _addAttachementCommand.RaiseCanExecuteChanged();
            _pinToStartCommand.RaiseCanExecuteChanged();
            _unpinFromStartCommand.RaiseCanExecuteChanged();
            _showCommand.RaiseCanExecuteChanged();
            _hideCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Pins or updates a single note to the start page.
        /// </summary>
        private void PinOrUpdateTile()
        {
            var noteWideGfx = GraphicsHelper.Create(new NoteWideTile(Title, Content));
            var noteWideUri = StorageHelper.SavePng(TileWidePath, noteWideGfx);
            GraphicsHelper.CleanUpMemory(noteWideGfx);

            var noteNormalGfx = GraphicsHelper.Create(new NoteNormalTile(Title, Content));
            var noteNormalUri = StorageHelper.SavePng(TileNormalPath, noteNormalGfx);
            GraphicsHelper.CleanUpMemory(noteNormalGfx);

            if (HasAttachement)
            {
                var imageUri = new Uri(StorageHelper.ISTORAGE_SCHEME + AttachedImagePath, UriKind.Absolute);

                FlipTileData tile = new FlipTileData();

                if (IsValid)
                {
                    tile = new FlipTileData()
                    {
                        BackBackgroundImage = imageUri,
                        WideBackBackgroundImage = imageUri,
                        SmallBackgroundImage = imageUri,
                        WideBackgroundImage = noteWideUri,
                        BackgroundImage = noteNormalUri,
                        BackTitle = AppResources.ApplicationTitle
                    };
                }
                else // without text, just an image
                {
                    tile = new FlipTileData()
                    {
                        BackgroundImage = imageUri,
                        WideBackgroundImage = imageUri,
                        SmallBackgroundImage = imageUri,
                        Title = AppResources.ApplicationTitle
                    };
                }

                LiveTilePinningHelper.PinOrUpdateTile(NavigationUri, tile);
            }
            else
            {
                var tile = new FlipTileData
                {
                    SmallBackgroundImage = noteNormalUri,
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
            NotifyPropertyChanged("CanPinToStart");
        }

        /// <summary>
        /// Clears all the generated tile images when these exist.
        /// </summary>
        private void ClearTileImages()
        {
            // delete file in background task
            Task.Run(() =>
            {
                StorageHelper.DeleteFile(TileNormalPath);
                StorageHelper.DeleteFile(TileWidePath);
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
                }
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
            set
            {
                _note.DateCreated = value;
            }
        }

        /// <summary>
        /// Gets the deletion date.
        /// </summary>
        public DateTime DateDeleted
        {
            get
            {
                return _note.DateDeleted;
            }
            set
            {
                if (_note.DateDeleted != value)
                {
                    _note.DateDeleted = value;
                    NotifyPropertyChanged("DateDeleted");
                }             
            }
        }

        /// <summary>
        /// Gets whether the note is visible.
        /// </summary>
        public bool IsHidden
        {
            get
            {
                return _note.IsHidden;
            }
            set
            {
                if (_note.IsHidden != value)
                {
                    _note.IsHidden = value;
                    NotifyPropertyChanged("IsHidden");
                    NotifyPropertyChanged("IsNotHidden");
                    NotifyPropertyChanged("IsHiddenAndActive");
                    NotifyPropertyChanged("IsNotHiddenAndActive");
                } 
            }
        }

        public bool IsNotHidden
        {
            get
            {
                return !_note.IsHidden;
            }
        }

        public bool IsHiddenAndActive
        {
            get
            {
                return _note.IsHidden && LockScreenHelper.HasAccess();
            }
        }

        public bool IsNotHiddenAndActive
        {
            get
            {
                return !_note.IsHidden && LockScreenHelper.HasAccess();
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
        /// Gets the path of the wide tile.
        /// </summary>
        public string TileWidePath
        {
            get
            {
                return LiveTileHelper.SHARED_SHELL_CONTENT_PATH + string.Format("livetile_wide_{0}.jpeg", Id);
            }
        }

        /// <summary>
        /// Gets the path of the normal tile.
        /// </summary>
        public string TileNormalPath
        {
            get
            {
                return LiveTileHelper.SHARED_SHELL_CONTENT_PATH + string.Format("livetile_normal_{0}.jpeg", Id);
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
        /// Gets whether the note is valid by checking text, title and attachement.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(Content) || HasAttachement;
            }
        }

        /// <summary>
        /// Gets whether the note is valid by checking text and title.
        /// </summary>
        public bool IsValidTextAndVisible
        {
            get
            {
                return (!string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(Content)) && !IsHidden;
            }
        }

        //public bool IsSpeechSupported
        //{
        //    get
        //    {
        //        return !Speech.Instance.HasRecognizerUIError;
        //    }
        //}

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
        /// Gets the delete from archive note command.
        /// </summary>
        public ICommand DeleteFromArchiveCommand
        {
            get
            {
                return _deleteFromArchiveCommand;
            }
        }

        /// <summary>
        /// Gets restore note command.
        /// </summary>
        public ICommand RestoreCommand
        {
            get
            {
                return _restoreCommand;
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

        /// <summary>
        /// Gets the share command.
        /// </summary>
        public ICommand ShareCommand
        {
            get
            {
                return _shareCommand;
            }
        }

        /// <summary>
        /// Gets speak prepend text command.
        /// </summary>
        public ICommand SpeakPrependTextCommand
        {
            get
            {
                return _speakPrependTextCommand;
            }
        }

        /// <summary>
        /// Gets speak append text command.
        /// </summary>
        public ICommand SpeakReplaceTextCommand
        {
            get
            {
                return _speakReplaceTextCommand;
            }
        }

        /// <summary>
        /// Gets speak append text command.
        /// </summary>
        public ICommand SpeakAppendTextCommand
        {
            get
            {
                return _speakAppendTextCommand;
            }
        }

        /// <summary>
        /// Gets the show note command.
        /// </summary>
        public ICommand ShowCommand
        {
            get
            {
                return _showCommand;
            }
        }

        /// <summary>
        /// Gets the hide note command.
        /// </summary>
        public ICommand HideCommand
        {
            get
            {
                return _hideCommand;
            }
        }

        /// <summary>
        /// Gets whether the date is visible in the list.
        /// </summary>
        public bool IsDateVisible
        {
            get
            {
                return Settings.ShowCreationDateOnList.Value == "1";
            }
        }

        #endregion
    }
}
