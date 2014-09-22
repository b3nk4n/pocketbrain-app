using PhoneKit.Framework.Controls;
using PocketBrain.App.Resources;
using PocketBrain.App.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PocketBrain.App.Controls
{
    public class BackupControlViewModel : BackupControlViewModelBase
    {
        public BackupControlViewModel()
            : base("000000004C119E36", AppResources.ApplicationTitle)
        {
            
        }

        protected override IDictionary<string, IList<string>> GetBackupDirectoriesAndFiles()
        {
            var pathsAndFiles = new Dictionary<string, IList<string>>();

            // note and archive
            var naList = new List<string>();
            if (NoteListViewModel.Instance.Notes.Count > 0)
            {
                naList.Add("notes.data");
            }
            if (ArchiveListViewModel.Instance.Notes.Count > 0)
            {
                naList.Add("archive.data");
            }
            pathsAndFiles.Add("/", naList);

            // attachements
            var attList = new List<string>();
            foreach (var note in NoteListViewModel.Instance.Notes)
            {
                if (note.HasAttachement)
                {
                    attList.Add(Path.GetFileName(note.AttachedImagePath));
                }
            }
            foreach (var note in ArchiveListViewModel.Instance.Notes)
            {
                if (note.HasAttachement)
                {
                    attList.Add(Path.GetFileName(note.AttachedImagePath));
                }
            }

            pathsAndFiles.Add("Shared/ShellContent/attachements/", attList);
            return pathsAndFiles;
        }

        protected override void BeforeBackup(string backupName)
        {
            base.BeforeBackup(backupName);

            NoteListViewModel.Instance.Save();
            ArchiveListViewModel.Instance.Save();
        }

        protected override void AfterBackup(string backupName, bool success)
        {
            base.AfterBackup(backupName, success);

            if (success)
            {
                MessageBox.Show(string.Format(AppResources.MessageBoxBackupSuccessText, backupName), AppResources.MessageBoxInfoTitle, MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show(string.Format(AppResources.MessageBoxBackupErrorText, backupName), AppResources.MessageBoxWarningTitle, MessageBoxButton.OK);
            }
        }

        protected override void AfterRestore(string backupName, bool success)
        {
            base.AfterRestore(backupName, success);

            if (success)
            {
                // remove tiles, because their reference link would be invalid
                foreach (var note in NoteListViewModel.Instance.Notes)
                {
                    note.UnpinTile();
                }

                // load new data to memory
                NoteListViewModel.Instance.Load(true);
                ArchiveListViewModel.Instance.Load(true);

                MessageBox.Show(string.Format(AppResources.MessageBoxRestoreSuccessText, backupName), AppResources.MessageBoxInfoTitle, MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show(string.Format(AppResources.MessageBoxRestoreErrorText, backupName), AppResources.MessageBoxWarningTitle, MessageBoxButton.OK);
            }
        }
    }
}
