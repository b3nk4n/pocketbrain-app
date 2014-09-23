﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PocketBrain.App.ViewModel;
using Microsoft.Phone.Tasks;
using System.IO;
using PhoneKit.Framework.Core.Storage;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BugSense.Core.Model;
using BugSense;
using System.Diagnostics;
using PhoneKit.Framework.InAppPurchase;

namespace PocketBrain.App
{
    /// <summary>
    /// The settings page.
    /// </summary>
    public partial class SettingsPage : PhoneApplicationPage
    {
        /// <summary>
        /// The photo chooser task.
        /// </summary>
        /// <remarks>Must be defined at class level to work properly in tombstoning.</remarks>
        private PhotoChooserTask _photoTask = new PhotoChooserTask();

        /// <summary>
        /// The persistent name of the next lockscreen image to toggle from A to B,
        /// which is required for lockscreen image update.
        /// </summary>
        private StoredObject<string> _nextLockScreenExtension = new StoredObject<string>("nextLockScreenBackgroundExtension", "A");

        /// <summary>
        /// Creates a SettingsPage instance.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();

            // init photo chooser task
            _photoTask.ShowCamera = true;
            _photoTask.Completed += (se, pr) =>
            {
                if (pr.Error != null || pr.TaskResult != TaskResult.OK)
                    return;

                // save a copy in local storage
                FileInfo fileInfo = new FileInfo(pr.OriginalFileName);

                // render lock image
                var nextExtension = _nextLockScreenExtension.Value;
                string filePath = string.Format("lockScreenBackground{0}{1}", nextExtension, fileInfo.Extension);
                _nextLockScreenExtension.Value = (nextExtension == "A") ? "B" : "A";

                if (StorageHelper.SaveFileFromStream(filePath, pr.ChosenPhoto))
                {
                    // save
                    Settings.LockScreenBackgroundImagePath.Value = filePath;

                    // update preview
                    UpdatePreviewImage();
                }
            };

            SelectBackgroundImageButton.Click += (s, e) =>
                {
                    _photoTask.Show();
                };

            MaxLockItemsPicker.SelectionChanged += (s, e) =>
                {
                    UpdatePreviewImage();
                };

            UpdatePreviewImage();

            DataContext = NoteListViewModel.Instance;
        }

        /// <summary>
        /// Updates the preview image.
        /// </summary>
        private void UpdatePreviewImage()
        {
            string foregroundImagePath;

            // update layout:
            switch(MaxLockItemsPicker.SelectedIndex)
            {
                case 1:
                    foregroundImagePath = "/Assets/dual.png";
                    break;
                case 2:
                    foregroundImagePath = "/Assets/quad.png";
                    break;
                case 3:
                    foregroundImagePath = "/Assets/six.png";
                    break;
                default:
                    foregroundImagePath = "/Assets/single.png";
                    break;
            }
            PreviewImageForeground.Source = new BitmapImage(new Uri(foregroundImagePath, UriKind.Relative));

            // load no image if there is no lockscreen access
            if (!NoteListViewModel.Instance.HasLockScreenAccess)
                return;

            var lockScreenPath = Settings.LockScreenBackgroundImagePath.Value;

            if (lockScreenPath != null)
            {
                BitmapImage img = new BitmapImage();
                try
                {
                    using (var imageStream = StorageHelper.GetFileStream(lockScreenPath))
                    {
                        img.SetSource(imageStream);
                        PreviewImageBackground.Source = img;
                    }
                }
                catch (Exception e)
                {
                    // BUGSENSE : Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.
                    // Occured 2 times (05.06.2014)
                    Debug.WriteLine("Could not set the image source with error: " + e.Message);
                }
            }
        }

        /// <summary>
        /// When the settings page is navigated to.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // load settings
            SelectByTag(CreationDatePicker, Settings.ShowCreationDateOnList.Value);
            SelectByTag(ExpandListPicker, Settings.ExpandListsMethod.Value);
            SelectByTag(MaxLockItemsPicker, Settings.MaximumLockItems.Value);
            SelectByTag(TileNoteCountListPicker, Settings.ShowNoteCountOnLiveTile.Value);
            SelectByTag(AddNoteButtonPicker, Settings.ShowAddNoteButton.Value);
            SelectByTag(LiveTileFontSizePicker, Settings.LiveTileFontSize.Value);
            SelectByTag(LockScreenFontSizePicker, Settings.LockScreenFontSize.Value);
            SelectByTag(KeyboardExtendedAutoCorrectPicker, Settings.KeyboardWordAutocorrection.Value);

            // hack: make sure the backup-button is visible after purchase.
            NoteListViewModel.Instance.UpdateHasProVersion();
        }

        /// <summary>
        /// When the settings page is navigated from.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            int pos = 0;

            // save settings
            try
            {
                Settings.ShowCreationDateOnList.Value = (string)(CreationDatePicker.SelectedItem as ListPickerItem).Tag;
                pos++;
                Settings.ExpandListsMethod.Value = (string)(ExpandListPicker.SelectedItem as ListPickerItem).Tag;
                pos++;
                var valueLockItems = (string)(MaxLockItemsPicker.SelectedItem as ListPickerItem).Tag;
                if (!InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_KEY))
                {
                    if (valueLockItems == "6")
                    {
                        valueLockItems = "4";
                    }
                }
                Settings.MaximumLockItems.Value = valueLockItems;
                pos++;
                Settings.ShowNoteCountOnLiveTile.Value = (string)(TileNoteCountListPicker.SelectedItem as ListPickerItem).Tag;
                pos++;
                Settings.ShowAddNoteButton.Value = (string)(AddNoteButtonPicker.SelectedItem as ListPickerItem).Tag;
                pos++;
                var valueTileSize = (string)(LiveTileFontSizePicker.SelectedItem as ListPickerItem).Tag;
                if (!InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_KEY))
                {
                    if (valueTileSize == "extralarge")
                    {
                        valueTileSize = "large";
                    }
                    else if (valueTileSize == "small")
                    {
                        valueTileSize = "medium";
                    }
                }
                Settings.LiveTileFontSize.Value = valueTileSize;
                pos++;
                var valueLockSize = (string)(LockScreenFontSizePicker.SelectedItem as ListPickerItem).Tag;
                if (!InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_KEY))
                {
                    if (valueLockSize == "extralarge")
                    {
                        valueLockSize = "large";
                    }
                    else if (valueLockSize == "small")
                    {
                        valueLockSize = "medium";
                    }
                }
                Settings.LockScreenFontSize.Value = valueLockSize;
                pos++;
                Settings.KeyboardWordAutocorrection.Value = (string)(KeyboardExtendedAutoCorrectPicker.SelectedItem as ListPickerItem).Tag;
                pos++;
            }
            catch(Exception ex)
            {
                // try catch block because there was an error once in the app that the app crashes inside the app without a known reason.
                // Invoke the LogException method with additional extra data that will be merged with the global extras in the request.
                BugSenseLogResult logResult = BugSenseHandler.Instance.LogException(ex, "SettingsSave", "Saving the settings failed on position: " + pos);
            }
        }

        /// <summary>
        /// Selects a item value by tag value.
        /// </summary>
        /// <param name="picker">The list picker.</param>
        /// <param name="tagToSelect">The tag value of the item to select.</param>
        private void SelectByTag(ListPicker picker, string tagToSelect)
        {
            if (picker == null || tagToSelect == null)
                return;

            foreach (var item in picker.Items)
            {
                var pickerItem = item as ListPickerItem;

                if (pickerItem != null)
                {
                    if ((string)pickerItem.Tag == tagToSelect)
                    {
                        picker.SelectedItem = pickerItem;
                        break;
                    }
                }
            }
        }
    }
}