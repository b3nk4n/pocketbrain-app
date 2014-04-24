using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.OS;
using PhoneKit.Framework.Core.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketBrain.App.ViewModel
{
    /// <summary>
    /// The screen overlay view model for fixed screen elements.
    /// </summary>
    public abstract class ExpandableListViewModelBase : ViewModelBase
    {
        /// <summary>
        /// The expand image for the light theme.
        /// </summary>
        private const string EXPAND_LIGHT = "Assets/AppBar/appbar.arrow.expand.png";

        /// <summary>
        /// The expand image for the dark theme.
        /// </summary>
        private const string EXPAND_DARK = "Assets/AppBar/appbar.arrow.expand.dark.png";

        /// <summary>
        /// The collapsed image for the light theme.
        /// </summary>
        private const string COLLAPSED_LIGHT = "Assets/AppBar/appbar.arrow.collapsed.png";

        /// <summary>
        /// The collapsed image for the dark theme.
        /// </summary>
        private const string COLLAPSED_DARK = "Assets/AppBar/appbar.arrow.collapsed.dark.png";

        /// <summary>
        /// The persistent key
        /// </summary>
        private readonly string _key;

        private StoredObject<bool> _isExpanded;

        /// <summary>
        /// The note collection.
        /// </summary>
        protected ObservableCollection<NoteViewModel> _notes = new ObservableCollection<NoteViewModel>();

        /// <summary>
        /// Creates a new ExpandableListViewModelBase instance.
        /// </summary>
        /// <param name="key">The key required for the stored expander state.</param>
        public ExpandableListViewModelBase(string key)
        {
            _key = "expanderState_" + key;
            _isExpanded = new StoredObject<bool>(_key, true);
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        protected abstract void Load();

        /// <summary>
        /// Saves the data.
        /// </summary>
        public abstract bool Save();

        /// <summary>
        /// Gets the notes list.
        /// </summary>
        public ObservableCollection<NoteViewModel> Notes
        {
            protected set
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
        /// Inserts a note at the beginning.
        /// </summary>
        /// <param name="note">The new note to add.</param>
        public void InsertNote(NoteViewModel note)
        {
            _notes.Insert(0, note);
            NotifyPropertyChanged("IsNoteListEmpty");
        }

        /// <summary>
        /// Inserts a note at the beginning.
        /// </summary>
        /// <param name="note">The new note to add.</param>
        public void RemoveNote(NoteViewModel note)
        {
            _notes.Remove(note);
            NotifyPropertyChanged("IsNoteListEmpty");
        }

        /// <summary>
        /// Toggles the expander state.
        /// </summary>
        public void ToggleExpanderState()
        {
            _isExpanded.Value = !_isExpanded.Value;
        }

        /// <summary>
        /// Gets wheter the persistent expander state is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return _isExpanded.Value;
            }
        }

        /// <summary
        /// Gets whether the expansion button is visible.
        /// </summary>
        public bool IsExtensionButtonVisible
        {
            get
            {
                return (Settings.ExpandListsMethod.Value == "1" || Settings.ExpandListsMethod.Value == "2") && _notes.Count > 0;
            }
        }

        /// <summary>
        /// Gets the expand button image path.
        /// </summary>
        public string ExpandImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return EXPAND_DARK;
                else
                    return EXPAND_LIGHT;
            }
        }

        /// <summary>
        /// Gets the collapsed button image path.
        /// </summary>
        public string CollapsedImagePath
        {
            get
            {
                if (PhoneThemeHelper.IsLightThemeActive)
                    return COLLAPSED_DARK;
                else
                    return COLLAPSED_LIGHT;
            }
        }

        /// <summary>
        /// Gets whether the notes list is empty.
        /// </summary>
        public bool IsNoteListEmpty
        {
            get
            {
                return _notes.Count == 0;
            }
        }
    }
}
