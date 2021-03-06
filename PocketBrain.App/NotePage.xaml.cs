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
using PhoneKit.Framework.OS;
using BugSense.Core.Model;
using BugSense;
using PhoneKit.Framework.InAppPurchase;

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

                    OverrideCreationDateMargin();

                    if (TitleTextBox.Text.Length == 0)
                    {
                        // select the text end of the notes content text field
                        try
                        {
                            TitleTextBox.Focus();
                            TitleTextBox.Select(ContentTextBox.Text.Length, 0);
                        }
                        catch (Exception)
                        {
                            // paranoid exception, because there could be the case that there is a BUG in the OS here.
                        }
                    }
                };

            TitleTextBox.KeyUp += (s, e) =>
            {
                // select the content when the enter key was pressed.
                if (e.Key == Key.Enter)
                {
                    // select the text end of the notes content text field
                    try
                    {
                        ContentTextBox.Focus();
                        ContentTextBox.Select(ContentTextBox.Text.Length, 0);
                    }
                    catch (Exception)
                    {
                        // paranoid exception, because there could be the case that there is a BUG in the OS here.
                    }  
                }
            };

            TitleTextBox.GotFocus += (s, e) =>
                {
                    ResetSpeakExpandButtonsVisibility();

                    ShowKeyboard();
                };

            TitleTextBox.LostFocus += (s, e) =>
                {
                    _lastFocusedInputElement = s as Control;
                    HideKeyboardExtension.Begin();
                };

            ContentTextBox.GotFocus += (s, e) =>
                {
                    ResetSpeakExpandButtonsVisibility();

                    ShowKeyboard();
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

            AttachementImage.Tap += async (s, e) =>
                {
                    var note = DataContext as NoteViewModel;

                    if (note != null)
                    {
                        try
                        {
                            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(StorageHelper.APPDATA_LOCAL_SCHEME + note.AttachedImagePath));
                            await Windows.System.Launcher.LaunchFileAsync(file);
                        }
                        catch (Exception ex)
                        {
                            BugSenseLogResult logResult = BugSenseHandler.Instance.LogException(ex, "ShowAttachement", "Tried to show attachement file : " + (note.AttachedImagePath != null ? note.AttachedImagePath : "null"));
                        }
                    }
                };
        }

        /// <summary>
        /// Overrides the creation date container margin depending on the screen resolution.
        /// </summary>
        /// <remarks>
        /// WXGA is default and is not overritten (12,-124,12,0).
        /// </remarks>
        private void OverrideCreationDateMargin()
        {
            var scaleFactor = DisplayHelper.GetResolution();

            if (scaleFactor == ScreenResolution.WXGA || scaleFactor == ScreenResolution.P720)
                CreationDateContainer.Margin = new Thickness(12, -200, 12, 0);
            else if (scaleFactor == ScreenResolution.P1080)
                CreationDateContainer.Margin = new Thickness(12, -308, 12, 0);
        }

        /// <summary>
        /// Is called when the page is navigated to.
        /// </summary>
        /// <param name="e">The event args</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // reset the exdenting buttons
            ResetSpeakExpandButtonsVisibility();

            // read navigation params
            string id = string.Empty;
            string title = string.Empty;
            string text = string.Empty;
            int mediaLibIndex = -1;
            if (NavigationContext.QueryString != null)
            {
                if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_NOTE_ID))
                {
                    id = NavigationContext.QueryString[AppConstants.PARAM_NOTE_ID];
                }
                if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_TITLE))
                {
                    title = NavigationContext.QueryString[AppConstants.PARAM_TITLE];
                }
                if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_TEXT))
                {
                    text = NavigationContext.QueryString[AppConstants.PARAM_TEXT];
                }
                if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_MEDIA_LIB_INDEX))
                {
                    var mediaLibIndexString = NavigationContext.QueryString[AppConstants.PARAM_MEDIA_LIB_INDEX];
                    int.TryParse(mediaLibIndexString, out mediaLibIndex);
                }
            }

            // load state
            if (PhoneStateHelper.ValueExists("currentNote"))
            {
                NoteListViewModel.Instance.CurrentNote = PhoneStateHelper.LoadValue<NoteViewModel>("currentNote");
                PhoneStateHelper.DeleteValue("currentNote");
            }

            if (!string.IsNullOrEmpty(id))
            {
                var note = NoteListViewModel.Instance.GetNoteById(id);

                if (note != null)
                {
                    NoteListViewModel.Instance.CurrentNote = note;
                }
                else
                {
                    // note tile was invalid. Go to main page!
                    NavigationService.Navigate(new Uri("/MainPage.xaml?clearbackstack=true", UriKind.Relative));
                }

            }
            else if (NoteListViewModel.Instance.CurrentNote != null && !string.IsNullOrEmpty(NoteListViewModel.Instance.CurrentNote.Id))
            {
                NoteListViewModel.Instance.CurrentNote = NoteListViewModel.Instance.GetNoteById(NoteListViewModel.Instance.CurrentNote.Id);
            }
            else
            {
                var note = new NoteViewModel();
                note.Title = title;
                note.Content = text;
                if (mediaLibIndex != -1)
                    note.SetAttachementFromMediaLibraryIndex(mediaLibIndex);

                NoteListViewModel.Instance.CurrentNote = note;

            }

            // set content text's input scope
            SetupInputScope();

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
                    NoteListViewModel.Instance.InsertNote(NoteListViewModel.Instance.CurrentNote);
                    MainPage.ScrollToTopOnNextNavigationTo = true;
                }
            }
            // save the current note
            else if (NoteListViewModel.Instance.CurrentNote.IsValid)
            {
                // add the note to the list, if it wasn't already stored before
                if (!NoteListViewModel.Instance.Notes.Contains(NoteListViewModel.Instance.CurrentNote))
                {
                    NoteListViewModel.Instance.InsertNote(NoteListViewModel.Instance.CurrentNote);
                    MainPage.ScrollToTopOnNextNavigationTo = true;
                }
            }

            // verify it was a BACK button or a WINDOWS button
            if (e.Uri.OriginalString == "app://external/")
            {
                NoteListViewModel.Instance.UpdateLockScreen();
            }

            NoteListViewModel.Instance.CurrentNote.UpdateTile();
            NoteListViewModel.Instance.CurrentNote = null;
        }

        /// <summary>
        /// Sets up the input scope of the content textbox.
        /// </summary>
        private void SetupInputScope()
        {
            var scope = new InputScope();
            var scopeName = new InputScopeName();

            if (Settings.KeyboardWordAutocorrection.Value == "0")
                scopeName.NameValue = InputScopeNameValue.Formula;
            else
                scopeName.NameValue = InputScopeNameValue.Text;

            scope.Names.Add(scopeName);
            ContentTextBox.InputScope = scope;
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
                }

                // transform
                var transform = (CompositeTransform)AttachementImage.RenderTransform;

                double imageRatio = (double)img.PixelHeight / img.PixelWidth;

                if (imageRatio != 1)
                {
                    // portrait
                    if (imageRatio > 1)
                    {
                        transform.TranslateY = ((imageRatio - 1) * AttachementImage.Height) / -2;
                    }
                    // landscape
                    else 
                    {
                        transform.TranslateX = ((imageRatio - 1) * AttachementImage.Width) / 2;
                    }
                }

                AttachementImageContainer.Visibility = System.Windows.Visibility.Visible;
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
        /// Resets the speak expanding buttons visibility.
        /// </summary>
        private void ResetSpeakExpandButtonsVisibility()
        {
            SpeakButton.Visibility = System.Windows.Visibility.Visible;
            SpeakContainer.Visibility = System.Windows.Visibility.Collapsed;
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
        /// Shows the keyboard
        /// </summary>
        private void ShowKeyboard()
        {
            if (!InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_KEY))
                return;

            if (DeviceHelper.IsLumia1520 && DisplayHelper.GetResolution() == ScreenResolution.P1080)
            {
                // special animation for lumia1520, because there is a different view behaviour
                ShowKeyboardExtension6inch.Begin();
            }
            else
            {
                if (DisplayHelper.GetScaleFactor() == ScaleFactor.WVGA || DisplayHelper.GetScaleFactor() == ScaleFactor.WXGA)
                    ShowKeyboardExtension.Begin();
                else
                    ShowKeyboardExtension16to9.Begin();
            }
        }

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
                int newLength = tbx.Text.Length - position;

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