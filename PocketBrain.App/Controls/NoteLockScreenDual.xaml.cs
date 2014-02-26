using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PocketBrain.App.Controls
{
    /// <summary>
    /// The note gual lock screen template.
    /// </summary>
    public partial class NoteLockScreenDual : UserControl
    {
        /// <summary>
        /// Creates a NoteLockScreenDual instance.
        /// </summary>
        /// <param name="title1">The title of note1.</param>
        /// <param name="text1">The content text of note1.</param>
        /// <param name="title2">The title of note2.</param>
        /// <param name="text2">The content text of note2.</param>
        public NoteLockScreenDual(string title1, string text1, string title2, string text2)
        {
            InitializeComponent();

            this.Title1.Text = title1;
            this.Text1.Text = text1;
            this.Title2.Text = title2;
            this.Text2.Text = text2;
        }
    }
}
