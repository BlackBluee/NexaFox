using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace NexaFox.Helpers
{
    public class AnsiTextParser
    {
        private static readonly Regex AnsiColorRegex = new Regex(@"\x1b\[((?:\d+;)*\d+)m", RegexOptions.Compiled);
        private static readonly Regex AnsiCursorRegex = new Regex(@"\x1b\[\d*[A-Ha-h]", RegexOptions.Compiled);
        private static readonly Regex AnsiEscapeRegex = new Regex(@"\x1b\[[^m]*m", RegexOptions.Compiled);

        private static readonly Color[] AnsiColors = new Color[]
        {
                Colors.Black,            // 0: Black
                Colors.DarkRed,          // 1: Red
                Colors.DarkGreen,        // 2: Green
                Colors.DarkGoldenrod,    // 3: Yellow
                Colors.DarkBlue,         // 4: Blue
                Colors.DarkMagenta,      // 5: Magenta
                Colors.DarkCyan,         // 6: Cyan
                Colors.LightGray,        // 7: White (light gray)
                Colors.DarkGray,         // 8: Bright Black (dark gray)
                Colors.Red,              // 9: Bright Red
                Colors.Green,            // 10: Bright Green
                Colors.Yellow,           // 11: Bright Yellow
                Colors.Blue,             // 12: Bright Blue
                Colors.Magenta,          // 13: Bright Magenta
                Colors.Cyan,             // 14: Bright Cyan
                Colors.White             // 15: Bright White
        };

        private Brush _currentForeground;
        private Brush _currentBackground;
        private bool _isBold;

        public AnsiTextParser()
        {
            ResetFormatting();
        }

        public void ResetFormatting()
        {
            _currentForeground = Brushes.LightGray;
            _currentBackground = Brushes.Black;
            _isBold = false;
        }

        private static readonly Regex AnsiClearScreenRegex = new Regex(@"\x1b\[2J", RegexOptions.Compiled);
        private static readonly Regex AnsiClearLineRegex = new Regex(@"\x1b\[\d*K", RegexOptions.Compiled);
        private static readonly Regex AnsiHomeRegex = new Regex(@"\x1b\[H", RegexOptions.Compiled);
        private static readonly Regex PromptSequenceRegex = new Regex(@"\]\d+;.*?(?:\a|\x07)", RegexOptions.Compiled);
        private static readonly Regex QuestionMarkSequenceRegex = new Regex(@"\[(?:\?|\d+)[a-zA-Z]", RegexOptions.Compiled);


        public void ParseAnsiText(string text, FlowDocument document)
        {
            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                if (AnsiClearScreenRegex.IsMatch(text))
                {
                    document.Blocks.Clear();
                    text = AnsiClearScreenRegex.Replace(text, string.Empty);
                }

                if (AnsiHomeRegex.IsMatch(text))
                {
                    text = AnsiHomeRegex.Replace(text, string.Empty);
                }

                text = PromptSequenceRegex.Replace(text, string.Empty);
                text = QuestionMarkSequenceRegex.Replace(text, string.Empty);
                text = AnsiClearLineRegex.Replace(text, string.Empty);
                text = AnsiCursorRegex.Replace(text, string.Empty);

                int lastIndex = 0;
                var matches = AnsiColorRegex.Matches(text);

                foreach (Match match in matches)
                {
                    string textBefore = text.Substring(lastIndex, match.Index - lastIndex);
                    if (!string.IsNullOrEmpty(textBefore))
                    {
                        AddText(document, textBefore);
                    }

                    string codeStr = match.Groups[1].Value;
                    InterpretAnsiCode(codeStr);

                    lastIndex = match.Index + match.Length;
                }

                if (lastIndex < text.Length)
                {
                    string remainingText = text.Substring(lastIndex);
                    remainingText = AnsiEscapeRegex.Replace(remainingText, string.Empty);
                    AddText(document, remainingText);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    string cleanText = Regex.Replace(text, @"\x1b\[.*?[A-Za-z]", string.Empty); 
                    cleanText = Regex.Replace(cleanText, @"\[\?\d+[a-zA-Z]", string.Empty); 
                    cleanText = Regex.Replace(cleanText, @"\]\d+;.*?\a", string.Empty); 

                    var para = new Paragraph(new Run(cleanText));
                    document.Blocks.Add(para);
                }
                catch { }
            }
        }

        private void InterpretAnsiCode(string codeStr)
        {
            string[] codes = codeStr.Split(';');

            foreach (string code in codes)
            {
                if (int.TryParse(code, out int codeValue))
                {
                    switch (codeValue)
                    {
                        case 0: // Reset
                            ResetFormatting();
                            break;
                        case 1: // Bold
                            _isBold = true;
                            break;
                        case 22: // Not bold
                            _isBold = false;
                            break;
                        default:
                            if (codeValue >= 30 && codeValue <= 37) // Foreground color
                            {
                                _currentForeground = new SolidColorBrush(AnsiColors[codeValue - 30]);
                            }
                            else if (codeValue >= 40 && codeValue <= 47) // Background color
                            {
                                _currentBackground = new SolidColorBrush(AnsiColors[codeValue - 40]);
                            }
                            else if (codeValue >= 90 && codeValue <= 97) // Bright foreground color
                            {
                                _currentForeground = new SolidColorBrush(AnsiColors[(codeValue - 90) + 8]);
                            }
                            else if (codeValue >= 100 && codeValue <= 107) // Bright background color
                            {
                                _currentBackground = new SolidColorBrush(AnsiColors[(codeValue - 100) + 8]);
                            }
                            break;
                    }
                }
            }
        }

        private void AddText(FlowDocument document, string text)
        {
            string[] lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                var run = new Run(lines[i])
                {
                    Foreground = _currentForeground,
                    Background = _currentBackground,
                    FontWeight = _isBold ? FontWeights.Bold : FontWeights.Normal
                };

                if (i == 0 && document.Blocks.Count > 0 && document.Blocks.LastBlock is Paragraph lastPara)
                {
                    lastPara.Inlines.Add(run);
                }
                else
                {
                    var para = new Paragraph();
                    para.Inlines.Add(run);
                    document.Blocks.Add(para);
                }

                if (i < lines.Length - 1)
                {
                    if (document.Blocks.LastBlock is Paragraph last)
                    {
                        last.Inlines.Add(new LineBreak());
                    }
                }
            }
        }
    }
}
