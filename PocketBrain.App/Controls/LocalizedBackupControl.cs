using PhoneKit.Framework.Controls;
using PocketBrain.App.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketBrain.App.Controls
{
    public class LocalizedBackupControl : BackupControlBase
    {
        public LocalizedBackupControl()
        {
            DataContext = new BackupControlViewModel();
        }

        /// <summary>
        /// Localizes the user controls contents and texts.
        /// </summary>
        protected override void LocalizeContent()
        {
            CreateBackupHeaderText = AppResources.CreateBackupHeaderText;
            RestoreBackupHeaderText = AppResources.RestoreBackupHeaderText;
            NameOfBackupHintText = AppResources.NameOfBackupHintText;
            BackupInfoText = AppResources.BackupInfoText;
            AttentionTitle = AppResources.AttentionTitle;
            RestoreInfoText = AppResources.RestoreInfoText;
            CommonBackupWarningText = AppResources.CommonBackupWarningText;
            LoginText = AppResources.Login;
        }
    }
}
