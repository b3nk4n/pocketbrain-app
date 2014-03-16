using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketBrain.App.ViewModel
{
    public class LockScreenPreviewViewModel
    {
        /// <summary>
        /// Gets the time.
        /// </summary>
        public string Time
        {
            get
            {
                return DateTime.Now.ToShortTimeString();
            }
        }

        /// <summary>
        /// Gets the date.
        /// </summary>
        public string Date
        {
            get
            {
                return string.Format("{0:M}", DateTime.Now);
            }
        }
        /// <summary>
        /// Gets the day.
        /// </summary>
        public string Day
        {
            get
            {
                return DateTime.Now.DayOfWeek.ToString();
            }
        }
    }
}
