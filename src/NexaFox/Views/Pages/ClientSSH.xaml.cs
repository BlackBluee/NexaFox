using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NexaFox.Helpers;
using NexaFox.ViewModels;

namespace NexaFox.Views.Pages
{
    public partial class ClientSSH : UserControl
    {
        private AnsiTextParser _ansiParser;
        private bool _autoScroll = true;

        public ClientSSH()
        {
            InitializeComponent();
            _ansiParser = new AnsiTextParser();

            Loaded += ClientSSH_Loaded;
            Unloaded += ClientSSH_Unloaded;

            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
            PasswordBox.GotFocus += PasswordBox_GotFocus;
            PasswordBox.LostFocus += PasswordBox_LostFocus;

            DataContextChanged += ClientSSH_DataContextChanged;
        }

        private void ClientSSH_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is SSHViewModel oldViewModel)
            {
                oldViewModel.TerminalOutputUpdated -= OnTerminalOutputUpdated;
            }

            if (e.NewValue is SSHViewModel newViewModel)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    OutputBox.Document.Blocks.Clear();
                    newViewModel.TerminalOutputUpdated += OnTerminalOutputUpdated;
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private void OnTerminalOutputUpdated(object sender, string text)
        {
            Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    bool wasAtEnd = IsScrolledToEnd();
                    string cleanedText = text;
                    cleanedText = Regex.Replace(cleanedText, @"\[(?:\?|\d+)[a-zA-Z]", ""); 
                    cleanedText = Regex.Replace(cleanedText, @"\]\d+;.*?(?:\a|\x07)", "");

                    _ansiParser.ParseAnsiText(cleanedText, OutputBox.Document);

                    if (wasAtEnd)
                    {
                        ScrollToEnd();
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        string ultraCleanText = Regex.Replace(text, @"(\x1b|\x07|\a)[\[\]()#;?]*(?:[0-9]{1,4}(?:;[0-9]{0,4})*)?[0-9A-PRZcf-nqry=><]", "");
                        ultraCleanText = Regex.Replace(ultraCleanText, @"\[(?:\?|\d+)[a-zA-Z]", "");
                        ultraCleanText = Regex.Replace(ultraCleanText, @"\]\d+;.*?[\a\x07]", "");

                        OutputBox.AppendText(ultraCleanText);
                        if (_autoScroll)
                        {
                            OutputBox.ScrollToEnd();
                        }
                    }
                    catch
                    {
                    }
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private bool IsScrolledToEnd()
        {
            try
            {
                return OutputBox.VerticalOffset + OutputBox.ViewportHeight >= OutputBox.ExtentHeight - 1;
            }
            catch
            {
                return true; 
            }
        }

        private void ScrollToEnd()
        {
            try
            {
                OutputBox.ScrollToEnd();
            }
            catch
            {
            }
        }

        private void ClientSSH_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePasswordPlaceholder();
        }

        private void ClientSSH_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IDisposable disposable)
                disposable.Dispose();

            if (DataContext is SSHViewModel viewModel)
            {
                viewModel.TerminalOutputUpdated -= OnTerminalOutputUpdated;
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePasswordPlaceholder();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SSHViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }

            UpdatePasswordPlaceholder();
        }

        private void UpdatePasswordPlaceholder()
        {
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    public static class RichTextBoxExtensions
    {
        public static ScrollViewer GetScrollViewer(this RichTextBox rtb)
        {
            if (rtb == null) return null;

            try
            {
                rtb.UpdateLayout();

                if (VisualTreeHelper.GetChildrenCount(rtb) == 0)
                    return null;

                var child = VisualTreeHelper.GetChild(rtb, 0) as DependencyObject;
                if (child == null) return null;

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(child); i++)
                {
                    var scrollViewer = VisualTreeHelper.GetChild(child, i) as ScrollViewer;
                    if (scrollViewer != null) return scrollViewer;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
