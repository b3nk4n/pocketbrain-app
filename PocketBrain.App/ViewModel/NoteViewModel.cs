using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using PhoneKit.Framework.Core.Graphics;
using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.OS;
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
        /// The email sharing command.
        /// </summary>
        private DelegateCommand _shareEmailCommand;

        /// <summary>
        /// The message sharing command.
        /// </summary>
        private DelegateCommand _shareMessageCommand;

        /// <summary>
        /// The WhatsApp sharing command.
        /// </summary>
        private DelegateCommand _shareWhatsappCommand;

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
                    _photoTask.Show();
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

            _shareEmailCommand = new DelegateCommand(() =>
                {
                    EmailComposeTask emailTask = new EmailComposeTask();
                    emailTask.Subject = DisplayedTitle;
                    emailTask.Body = Content;
                    emailTask.Show();
                });

            _shareMessageCommand = new DelegateCommand(() =>
                {
                    SmsComposeTask smsTask = new SmsComposeTask();
                    smsTask.Body = string.Format("{0}\r\r{1}", DisplayedTitle, Content);
                    smsTask.Show();
                });

            _shareWhatsappCommand = new DelegateCommand(async () =>
                {
                    if (MessageBox.Show(AppResources.MessageBoxInfoClipboard, AppResources.MessageBoxInfoTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        Clipboard.SetText(string.Format("{0}\r\r{1}", DisplayedTitle, Content));
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("whatsapp:"));
                    }
                });

            _speakPrependTextCommand = new DelegateCommand(async () =>
                {
                    try
                    {
                        if (Speech.Instance.HasRecognizerUI)
                        {
                            Speech.Instance.RecognizerUI.Settings.ReadoutEnabled = false;
                            Speech.Instance.RecognizerUI.Settings.ShowConfirmation = false;
                            var result = await Speech.Instance.RecognizerUI.RecognizeWithUIAsync();

                            if (result.ResultStatus == Windows.Phone.Speech.Recognition.SpeechRecognitionUIStatus.Succeeded)
                            {
                                Content = string.Format("{0}\r{1}", result.RecognitionResult.Text, Content);
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
                        if (Speech.Instance.HasRecognizerUI)
                        {
                            Speech.Instance.RecognizerUI.Settings.ReadoutEnabled = false;
                            Speech.Instance.RecognizerUI.Settings.ShowConfirmation = false;
                            var result = await Speech.Instance.RecognizerUI.RecognizeWithUIAsync();

                            if (result.ResultStatus == Windows.Phone.Speech.Recognition.SpeechRecognitionUIStatus.Succeeded)
                            {
                                Content = result.RecognitionResult.Text;
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
                        if (Speech.Instance.HasRecognizerUI)
                        {
                            Speech.Instance.RecognizerUI.Settings.ReadoutEnabled = false;
                            Speech.Instance.RecognizerUI.Settings.ShowConfirmation = false;
                            var result = await Speech.Instance.RecognizerUI.RecognizeWithUIAsync();

                            if (result.ResultStatus == Windows.Phone.Speech.Recognition.SpeechRecognitionUIStatus.Succeeded)
                            {
                                Content += "\r" + result.RecognitionResult.Text;
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
                    string filePath = GetUniqueLocalFilePathOfFile(pr.OriginalFileName);
                    if (StorageHelper.SaveFileFromStream(filePath, pr.ChosenPhoto))
                    {
                        SetAttachement(filePath);
                    }
                };      
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
            var noteWideUri = StorageHelper.SaveJpeg(TileWidePath, noteWideGfx);
            var noteNormalGfx = GraphicsHelper.Create(new NoteNormalTile(Title, Content));
            var noteNormalUri = StorageHelper.SaveJpeg(TileNormalPath, noteNormalGfx);

            if (HasAttachement)
            {
                var imageUri = new Uri(StorageHelper.ISTORAGE_SCHEME + AttachedImagePath, UriKind.Absolute);

                FlipTileData tile = new FlipTileData();


                if (IsValidTextAndVisible)
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
                return string.IsNullOrEmpty(_note.Title) ? AppResources.DefaultTitle : _note.Title;
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
                } 
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
        /// Gets the share email command.
        /// </summary>
        public ICommand ShareEmailCommand
        {
            get
            {
                return _shareEmailCommand;
            }
        }

        /// <summary>
        /// Gets the share message command.
        /// </summary>
        public ICommand ShareMessageCommand
        {
            get
            {
                return _shareMessageCommand;
            }
        }

        /// <summary>
        /// Gets the share Whatsapp command.
        /// </summary>
        public ICommand ShareWhatsappCommand
        {
            get
            {
                return _shareWhatsappCommand;
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

        #region Themed resources

        /// <summary>
        /// The speak image for the light theme.
        /// </summary>
        private const string SPEAK_LIGHT = "Assets/AppBar/appbar.microphone.png";

        /// <summary>
        /// The speak image for the dark theme.
        /// </summary>
        private const string SPEAK_DARK = "Assets/AppBar/appbar.microphone.dark.png";

        /// <summary>
        /// Gets the speak button image path.
        /// </summary>
        public string SpeakImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return SPEAK_DARK;
                else
                    return SPEAK_LIGHT;
            }
        }

        /// <summary>
        /// The speak prepend image for the light theme.
        /// </summary>
        private const string SPEAK_PREPEND_LIGHT = "Assets/AppBar/appbar.prepend.png";

        /// <summary>
        /// The speak prepend image for the dark theme.
        /// </summary>
        private const string SPEAK_PREPEND_DARK = "Assets/AppBar/appbar.prepend.dark.png";

        /// <summary>
        /// Gets the speak prepend button image path.
        /// </summary>
        public string SpeakPrependImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return SPEAK_PREPEND_DARK;
                else
                    return SPEAK_PREPEND_LIGHT;
            }
        }

        /// <summary>
        /// The speak replace image for the light theme.
        /// </summary>
        private const string SPEAK_REPLACE_LIGHT = "Assets/AppBar/appbar.replace.png";

        /// <summary>
        /// The speak replace image for the dark theme.
        /// </summary>
        private const string SPEAK_REPLACE_DARK = "Assets/AppBar/appbar.replace.dark.png";

        /// <summary>
        /// Gets the speak replace button image path.
        /// </summary>
        public string SpeakReplaceImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return SPEAK_REPLACE_DARK;
                else
                    return SPEAK_REPLACE_LIGHT;
            }
        }

        /// <summary>
        /// The speak append image for the light theme.
        /// </summary>
        private const string SPEAK_APPEND_LIGHT = "Assets/AppBar/appbar.append.png";

        /// <summary>
        /// The speak append image for the dark theme.
        /// </summary>
        private const string SPEAK_APPEND_DARK = "Assets/AppBar/appbar.append.dark.png";

        /// <summary>
        /// Gets the speak append button image path.
        /// </summary>
        public string SpeakAppendImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return SPEAK_APPEND_DARK;
                else
                    return SPEAK_APPEND_LIGHT;
            }
        }

        /// <summary>
        /// The email image for the light theme.
        /// </summary>
        private const string EMAIL_LIGHT = "Assets/AppBar/appbar.email.png";

        /// <summary>
        /// The email image for the dark theme.
        /// </summary>
        private const string EMAIL_DARK = "Assets/AppBar/appbar.email.dark.png";

        /// <summary>
        /// Gets the email button image path.
        /// </summary>
        public string EmailImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return EMAIL_DARK;
                else
                    return EMAIL_LIGHT;
            }
        }

        /// <summary>
        /// The message image for the light theme.
        /// </summary>
        private const string MESSAGE_LIGHT = "Assets/AppBar/appbar.message.png";

        /// <summary>
        /// The message image for the dark theme.
        /// </summary>
        private const string MESSAGE_DARK = "Assets/AppBar/appbar.message.dark.png";

        /// <summary>
        /// Gets the message button image path.
        /// </summary>
        public string MessageImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return MESSAGE_DARK;
                else
                    return MESSAGE_LIGHT;
            }
        }

        /// <summary>
        /// The whatsapp image for the light theme.
        /// </summary>
        private const string WHATSAPP_LIGHT = "Assets/AppBar/appbar.whatsapp.png";

        /// <summary>
        /// The whatsapp image for the dark theme.
        /// </summary>
        private const string WHATSAPP_DARK = "Assets/AppBar/appbar.whatsapp.dark.png";

        /// <summary>
        /// Gets the whatsapp button image path.
        /// </summary>
        public string WhatsappImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return WHATSAPP_DARK;
                else
                    return WHATSAPP_LIGHT;
            }
        }

        /// <summary>
        /// The paperclip image for the light theme.
        /// </summary>
        private const string PAPERCLIP_LIGHT = "Assets/AppBar/appbar.paperclip.png";

        /// <summary>
        /// The paperclip image for the dark theme.
        /// </summary>
        private const string PAPERCLIP_DARK = "Assets/AppBar/appbar.paperclip.dark.png";

        /// <summary>
        /// Gets the paperclip button image path.
        /// </summary>
        public string PaperclipImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return PAPERCLIP_DARK;
                else
                    return PAPERCLIP_LIGHT;
            }
        }

        /// <summary>
        /// The paperclip remove image for the light theme.
        /// </summary>
        private const string PAPERCLIP_REMOVE_LIGHT = "Assets/AppBar/appbar.paperclip.remove.png";

        /// <summary>
        /// The paperclip remove image for the dark theme.
        /// </summary>
        private const string PAPERCLIP_REMOVE_DARK = "Assets/AppBar/appbar.paperclip.remove.dark.png";

        /// <summary>
        /// Gets the paperclip remove button image path.
        /// </summary>
        public string PaperclipRemoveImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return PAPERCLIP_REMOVE_DARK;
                else
                    return PAPERCLIP_REMOVE_LIGHT;
            }
        }

        /// <summary>
        /// The share image for the light theme.
        /// </summary>
        private const string SHARE_LIGHT = "Assets/AppBar/appbar.share.png";

        /// <summary>
        /// The share image for the dark theme.
        /// </summary>
        private const string SHARE_DARK = "Assets/AppBar/appbar.share.dark.png";

        /// <summary>
        /// Gets the share button image path.
        /// </summary>
        public string ShareImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return SHARE_DARK;
                else
                    return SHARE_LIGHT;
            }
        }

        /// <summary>
        /// The share open image for the light theme.
        /// </summary>
        private const string SHARE_OPEN_LIGHT = "Assets/AppBar/appbar.share.open.png";

        /// <summary>
        /// The share open image for the dark theme.
        /// </summary>
        private const string SHARE_OPEN_DARK = "Assets/AppBar/appbar.share.open.dark.png";

        /// <summary>
        /// Gets the share open button image path.
        /// </summary>
        public string ShareOpenImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return SHARE_OPEN_DARK;
                else
                    return SHARE_OPEN_LIGHT;
            }
        }

        /// <summary>
        /// The pin image for the light theme.
        /// </summary>
        private const string PIN_LIGHT = "Assets/AppBar/appbar.pin.png";

        /// <summary>
        /// The pin image for the dark theme.
        /// </summary>
        private const string PIN_DARK = "Assets/AppBar/appbar.pin.dark.png";

        /// <summary>
        /// Gets the pin button image path.
        /// </summary>
        public string PinImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return PIN_DARK;
                else
                    return PIN_LIGHT;
            }
        }

        /// <summary>
        /// The pin remove image for the light theme.
        /// </summary>
        private const string PIN_REMOVE_LIGHT = "Assets/AppBar/appbar.pin.remove.png";

        /// <summary>
        /// The pin remove image for the dark theme.
        /// </summary>
        private const string PIN_REMOVE_DARK = "Assets/AppBar/appbar.pin.remove.dark.png";

        /// <summary>
        /// Gets the paperclip remove button image path.
        /// </summary>
        public string PinRemoveImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return PIN_REMOVE_DARK;
                else
                    return PIN_REMOVE_LIGHT;
            }
        }

        /// <summary>
        /// The eye image for the light theme.
        /// </summary>
        private const string EYE_LIGHT = "Assets/AppBar/appbar.eye.png";

        /// <summary>
        /// The eye image for the dark theme.
        /// </summary>
        private const string EYE_DARK = "Assets/AppBar/appbar.eye.dark.png";

        /// <summary>
        /// Gets the eye button image path.
        /// </summary>
        public string EyeImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return EYE_DARK;
                else
                    return EYE_LIGHT;
            }
        }

        /// <summary>
        /// The eye hide image for the light theme.
        /// </summary>
        private const string EYE_HIDE_LIGHT = "Assets/AppBar/appbar.eye.hide.png";

        /// <summary>
        /// The eye hide image for the dark theme.
        /// </summary>
        private const string EYE_HIDE_DARK = "Assets/AppBar/appbar.eye.hide.dark.png";

        /// <summary>
        /// Gets the eye hide button image path.
        /// </summary>
        public string EyeHideImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return EYE_HIDE_DARK;
                else
                    return EYE_HIDE_LIGHT;
            }
        }

        /// <summary>
        /// The delete image for the light theme.
        /// </summary>
        private const string DELETE_LIGHT = "Assets/AppBar/appbar.delete.png";

        /// <summary>
        /// The delete image for the dark theme.
        /// </summary>
        private const string DELETE_DARK = "Assets/AppBar/appbar.delete.dark.png";

        /// <summary>
        /// Gets the delete button image path.
        /// </summary>
        public string DeleteImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return DELETE_DARK;
                else
                    return DELETE_LIGHT;
            }
        }

        #endregion
    }
}
