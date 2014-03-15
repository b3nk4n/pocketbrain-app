using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using PocketBrain.App.ViewModel;
using System.Windows.Media.Imaging;
using PhoneKit.Framework.Core.Storage;
using System;
using System.Windows.Input;
using System.Windows.Media;
using PhoneKit.Framework.Storage;
using System.Windows.Controls;
using Microsoft.Xna.Framework;
using System.Windows;
using System.Windows.Data;
using Microsoft.Xna.Framework.GamerServices;

namespace PocketBrain.App
{
    /// <summary>
    /// The page of a single note.
    /// </summary>
    public partial class NotePage : PhoneApplicationPage
    {
        /// <summary>
        /// The last focues element to refocus with the keyboard extensions.
        /// </summary>
        private Control _lastFocusedInputElement;

        /// <summary>
        /// Creates the note page instance.
        /// </summary>
        public NotePage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
                {
                    BindToKeyboardFocus();

                    if (ContentTextBox.Text.Length == 0)
                    {
                        // select the text end of the notes content text field
                        ContentTextBox.Focus();
                        ContentTextBox.Select(ContentTextBox.Text.Length, 0);
                    }
                };

            TitleTextBox.GotFocus += (s, e) =>
                {
                    ResetSpeakExpandButtonsVisibility();

                    ShowKeyboardExtension.Begin();
                };

            TitleTextBox.LostFocus += (s, e) =>
                {
                    _lastFocusedInputElement = s as Control;
                    HideKeyboardExtension.Begin();
                };

            ContentTextBox.GotFocus += (s, e) =>
                {
                    ResetSpeakExpandButtonsVisibility();

                    ShowKeyboardExtension.Begin();
                };

            ContentTextBox.LostFocus += (s, e) =>
                {
                    _lastFocusedInputElement = s as Control;
                    HideKeyboardExtension.Begin();
                };

            DeleteNoteButton.Click += (s, e) =>
                {
                    ClearAttachedImageSource();

                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack(); 
                    else
                        NavigationService.Navigate(new Uri("/MainPage.xaml?clearbackstack=true", UriKind.Relative));
                };

            RemoveAttachementButton.Click += (s, e) =>
                {
                    ClearAttachedImageSource();
                };

            ShareButton.Click += (s, e) =>
                {
                    ShareButton.Visibility = System.Windows.Visibility.Collapsed;
                    SharingContainer.Visibility = System.Windows.Visibility.Visible;
                };

            SpeakButton.Click += (s, e) =>
            {
                var note = DataContext as NoteViewModel;

                if (note != null)
                {
                    if (!string.IsNullOrEmpty(note.Content))
                    {
                        SpeakButton.Visibility = System.Windows.Visibility.Collapsed;
                        SpeakContainer.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        note.SpeakReplaceTextCommand.Execute(null);
                    }
                }
            };
        }

        /// <summary>
        /// Is called when the page is navigated to.
        /// </summary>
        /// <param name="e">The event args</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // reset the exdenting buttons
            ResetSharingExpandButtonsVisibility();
            ResetSpeakExpandButtonsVisibility();

            // load state
            if (PhoneStateHelper.ValueExists("currentNote"))
            {
                NoteListViewModel.Instance.CurrentNote = PhoneStateHelper.LoadValue<NoteViewModel>("currentNote");
                PhoneStateHelper.DeleteValue("currentNote");
            }

            if (NavigationContext.QueryString != null && 
                NavigationContext.QueryString.ContainsKey("id"))
            {
                NoteListViewModel.Instance.CurrentNote = NoteListViewModel.Instance.GetNoteById(NavigationContext.QueryString["id"]);
            }
            else if (NoteListViewModel.Instance.CurrentNote != null && !string.IsNullOrEmpty(NoteListViewModel.Instance.CurrentNote.Id))
            {
                NoteListViewModel.Instance.CurrentNote = NoteListViewModel.Instance.GetNoteById(NoteListViewModel.Instance.CurrentNote.Id);
            }
            else
            {
                NoteListViewModel.Instance.CurrentNote = new NoteViewModel();
            }

            // set the current note as binding context
            DataContext = NoteListViewModel.Instance.CurrentNote;

            UpdateAttachedImageSource(NoteListViewModel.Instance.CurrentNote);
        }

        /// <summary>
        /// Saves the live tile, when the user leaves the notes page.
        /// </summary>
        /// <param name="e">The navigation event args.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // verify it was a BACK button or a WINDOWS button
            if (e.Uri.OriginalString == "app://external/")
            {
                NoteListViewModel.Instance.UpdateLockScreen();
            }

            // ensure there is an active note
            if (NoteListViewModel.Instance.CurrentNote == null)
                return;

            // save state
            if (e.NavigationMode != NavigationMode.Back || e.Uri.OriginalString == "app://external/")
            {
                PhoneStateHelper.SaveValue("currentNote", NoteListViewModel.Instance.CurrentNote);
            }

            // filter navigation to library/camera and delete button event
            if (e.NavigationMode == NavigationMode.New)
            {
                // add the note to the list, if it wasn't already stored before
                if (!NoteListViewModel.Instance.Notes.Contains(NoteListViewModel.Instance.CurrentNote))
                {
                    NoteListViewModel.Instance.Notes.Insert(0, NoteListViewModel.Instance.CurrentNote);
                    MainPage.ScrollToTopOnNextNavigationTo = true;
                }
                return;
            }

            // save the current note
            if (NoteListViewModel.Instance.CurrentNote.IsValid)
            {
                // add the note to the list, if it wasn't already stored before
                if (!NoteListViewModel.Instance.Notes.Contains(NoteListViewModel.Instance.CurrentNote))
                {
                    NoteListViewModel.Instance.Notes.Insert(0, NoteListViewModel.Instance.CurrentNote);
                    MainPage.ScrollToTopOnNextNavigationTo = true;
                }
            }

            NoteListViewModel.Instance.CurrentNote.UpdateTile();
            NoteListViewModel.Instance.CurrentNote = null;
        }

        /// <summary>
        /// Updates the attached image from the models image path.
        /// </summary>
        /// <remarks>
        /// Binding the image URI or path didn't work when the image is located in isolated storage,
        /// so we do it now this way manuelly.
        /// </remarks>
        /// <param name="note">The current note view model.</param>
        private void UpdateAttachedImageSource(NoteViewModel note)
        {
            if (note != null && note.HasAttachement)
            {
                var imagePath = note.AttachedImagePath;

                BitmapImage img = new BitmapImage();
                using (var imageStream = StorageHelper.GetFileStream(imagePath))
                {
                    // in case of a not successfully saved image
                    if (imageStream == null)
                    {
                        AttachementImage.Source = null;
                        AttachementImageContainer.Visibility = System.Windows.Visibility.Collapsed;
                        return;
                    }

                    img.SetSource(imageStream);
                    AttachementImage.Source = img;
                    AttachementImageContainer.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                AttachementImage.Source = null;
                AttachementImageContainer.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Clears the attached image source.
        /// </summary>
        /// <remarks>
        /// This is required because the image can not be deleted when any control has still
        /// any reference to this file.
        /// </remarks>
        private void ClearAttachedImageSource()
        {
            AttachementImage.Source = null;
        }

        /// <summary>
        /// Resets the sharing expanding buttons visibility.
        /// </summary>
        private void ResetSharingExpandButtonsVisibility()
        {
            ShareButton.Visibility = System.Windows.Visibility.Visible;
            SharingContainer.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Resets the speak expanding buttons visibility.
        /// </summary>
        private void ResetSpeakExpandButtonsVisibility()
        {
            SpeakButton.Visibility = System.Windows.Visibility.Visible;
            SpeakContainer.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// The manipulation delta event for pinch zoom of the attachement image.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void AttachementImage_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            if (e.PinchManipulation != null)
            {
                var transform = (CompositeTransform)AttachementImage.RenderTransform;

                // Scale Manipulation
                transform.ScaleX = e.PinchManipulation.CumulativeScale;
                transform.ScaleY = e.PinchManipulation.CumulativeScale;

                // Translate manipulation
                var originalCenter = e.PinchManipulation.Original.Center;
                var newCenter = e.PinchManipulation.Current.Center;
                transform.TranslateX = newCenter.X - originalCenter.X;
                transform.TranslateY = newCenter.Y - originalCenter.Y;

                // end 
                e.Handled = true;
            }
        }

        /// <summary>
        /// Resets the exander button state when any sharing button was clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void SharingButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            ResetSharingExpandButtonsVisibility();
        }

        /// <summary>
        /// Resets the exander button state when any speak button was clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void SpeakButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            ResetSpeakExpandButtonsVisibility();
        }

        #region Keyboard extension

        // Constants
        private const double LandscapeShift = -259d;
        private const double LandscapeShiftWithBar = -328d;
        private const double Epsilon = 0.00000001d;
        private const double PortraitShift = -339d;
        private const double PortraitShiftWithBar = -408d;

        /// <summary>
        /// The translation Y dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslateYProperty = DependencyProperty.Register("TranslateY", typeof(double), typeof(NotePage), new PropertyMetadata(0d, OnRenderYPropertyChanged));

        /// <summary>
        /// The on render changed event to update the margin.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event args.</param>
        private static void OnRenderYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NotePage)d).UpdateTopMargin((double)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the Y translation.
        /// </summary>
        public double TranslateY
        {
            get { return (double)GetValue(TranslateYProperty); }
            set { SetValue(TranslateYProperty, value); }
        }

        /// <summary>
        /// Binds to the keyboard focus.
        /// </summary>
        private void BindToKeyboardFocus()
        {
            PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                var group = frame.RenderTransform as TransformGroup;
                if (group != null)
                {
                    var translate = group.Children[0] as TranslateTransform;
                    var translateYBinding = new Binding("Y");
                    translateYBinding.Source = translate;
                    SetBinding(TranslateYProperty, translateYBinding);
                }
            }
        }

        /// <summary>
        /// Updtes the top margin.
        /// </summary>
        /// <param name="translateY">The y translation.</param>
        private void UpdateTopMargin(double translateY)
        {
            KeyboardExtensionContainer.Margin = new Thickness(0, -translateY, 0, 0);
        }

        /// <summary>
        /// Resets the margin when the textbox is unfocused.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            KeyboardExtensionContainer.Margin = new Thickness();
        }

        /// <summary>
        /// Moves the cursor of the given textbox.
        /// </summary>
        /// <param name="tbx">The textbox.</param>
        /// <param name="offset">The offset to move.</param>
        private void MoveCursor(TextBox tbx, int offset)
        {
            if (tbx == null)
                return;

            if (tbx.SelectionLength > 0)
            {
                // move selection end
                int position = tbx.SelectionStart;
                int newLength = tbx.SelectionLength + offset;

                if (newLength + position > tbx.Text.Length)
                    newLength = tbx.Text.Length - position;
                else if (newLength < 0)
                    newLength = 0;

                tbx.Select(position, newLength);
            }
            else if (tbx.SelectionLength == 0)
            {
                // move cursor
                int position = tbx.SelectionStart;
                int newPosition = (int)MathHelper.Clamp((int)position + offset, (int)0, (int)tbx.Text.Length);

                tbx.Select(newPosition, 0);
            }
        }

        /// <summary>
        /// Moves the cursor to the end of the given textbox and scrolls it into view.
        /// </summary>
        /// <param name="tbx">The textbox.</param>
        private void MoveCursorToEnd(TextBox tbx)
        {
            if (tbx == null)
                return;

            if (tbx.SelectionLength > 0)
            {
                // move selection end
                int position = tbx.SelectionStart;
                int newLength = tbx.Text.Length - tbx.SelectionLength;

                tbx.Select(position, newLength);
            }
            else if (tbx.SelectionLength == 0)
            {
                tbx.Select(tbx.Text.Length, 0);
            }

            // guess textbox heigth and move view position
            if (GuestTextHeightInLines(tbx) > 8)
                Scroller.ScrollToVerticalOffset(Math.Max(0, tbx.ActualHeight - 290));
        }

        /// <summary>
        /// Try to guess the text height in lines.
        /// </summary>
        /// <param name="tbx">The textbox.</param>
        private int GuestTextHeightInLines(TextBox tbx)
        {
            var lines = tbx.Text.Split('\r', '\n');
            int counter = 0;

            foreach (var line in lines)
            {
                counter += 1 + (int)(line.Length / 32);
            }

            return counter;
        }

        /// <summary>
        /// The left button event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void KeyboardExtensionLeftClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_lastFocusedInputElement != null)
                _lastFocusedInputElement.Focus();

            MoveCursor(_lastFocusedInputElement as TextBox, -1);
        }

        /// <summary>
        /// The total right button event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void KeyboardExtensionRightClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_lastFocusedInputElement != null)
                _lastFocusedInputElement.Focus();

            MoveCursor(_lastFocusedInputElement as TextBox, 1);
        }

        /// <summary>
        /// The right button event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void KeyboardExtensionTotalRightClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_lastFocusedInputElement != null)
                _lastFocusedInputElement.Focus();

            MoveCursorToEnd(_lastFocusedInputElement as TextBox);
        }

        /// <summary>
        /// Keeps the input control fouces when the background of the keybord extension was tapped.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event args.</param>
        private void Polygon_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (_lastFocusedInputElement != null)
                _lastFocusedInputElement.Focus();
        }

        #endregion

        
    }
}