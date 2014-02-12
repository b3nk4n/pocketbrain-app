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

namespace PocketBrain.App
{
    public partial class NotePage : PhoneApplicationPage
    {
        public NotePage()
        {
            InitializeComponent();

            DeleteNoteButton.Click += (s, e) =>
                {
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                };

            DataContext = NoteListViewModel.Instance;
        }
    }
}