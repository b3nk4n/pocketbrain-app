using Microsoft.Phone.Tasks;
using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.Storage;
using PocketBrain.App.Model;
using System;
using System.IO;
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
        }

        #endregion

        /// <summary>
        /// Gets the unique local file name for the given file to copy.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The unique file name in isolated storage.</returns>
        private string GetUniqueLocalFilePathOfFile(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            return string.Format("/attachements/{0:000000}_{1}", _random.Next(0, 1000000), fileInfo.Name);
        }

        /// <summary>
        /// Sets the new attachement image and notifies the UI and command manager.
        /// </summary>
        /// <param name="filePath">The file path of the attachement.</param>
        private void SetAttachement(string filePath)
        {
            NoteListViewModel.Instance.SelectedNote.AttachedImagePath = filePath;
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
        }

        #region Properties

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

        #endregion
    }
}
