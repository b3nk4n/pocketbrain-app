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
    /// The note lock screen template.
    /// </summary>
    public partial class NoteLockScreen : UserControl
    {
        /// <summary>
        /// Creates a NoteLockScreen instance.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="text">The content text.</param>
        public NoteLockScreen(string title, string text)
        {
            InitializeComponent();

            this.Title.Text = title;
            this.Text.Text = text;
        }
    }
}
