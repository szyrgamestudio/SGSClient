using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;

namespace SGSClient.Helpers;
public class HtmlToParagraphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string html = value as string ?? string.Empty;

        // prosty parsing — zamiana <br> na \n
        html = Regex.Replace(html, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);

        // usuń zbędne spacje i CRLF
        html = html.Trim();

        // utwórz paragraf
        Paragraph paragraph = new Paragraph();

        // split po \n, by zachować linebreaki
        string[] lines = html.Split('\n');
        foreach (var line in lines)
        {
            // obsługa <b>, <i>, <u> – prosta wersja
            string temp = line;

            // Bold
            temp = Regex.Replace(temp, @"<b>(.*?)</b>", "§b$1§/b", RegexOptions.IgnoreCase);
            // Italic
            temp = Regex.Replace(temp, @"<i>(.*?)</i>", "§i$1§/i", RegexOptions.IgnoreCase);
            // Underline
            temp = Regex.Replace(temp, @"<u>(.*?)</u>", "§u$1§/u", RegexOptions.IgnoreCase);

            while (true)
            {
                Match m = Regex.Match(temp, @"§(b|i|u)(.*?)§/\1", RegexOptions.IgnoreCase);
                if (!m.Success)
                {
                    paragraph.Inlines.Add(new Run { Text = temp });
                    break;
                }

                // tekst przed tagiem
                if (m.Index > 0)
                    paragraph.Inlines.Add(new Run { Text = temp[..m.Index] });

                string tag = m.Groups[1].Value.ToLower();
                string content = m.Groups[2].Value;

                Inline inline;

                switch (tag)
                {
                    case "b":
                        var b = new Bold();
                        b.Inlines.Add(new Run { Text = content });
                        inline = b;
                        break;

                    case "i":
                        var i = new Italic();
                        i.Inlines.Add(new Run { Text = content });
                        inline = i;
                        break;

                    case "u":
                        var u = new Underline();
                        u.Inlines.Add(new Run { Text = content });
                        inline = u;
                        break;

                    default:
                        inline = new Run { Text = content };
                        break;
                }

                paragraph.Inlines.Add(inline);

                temp = temp[(m.Index + m.Length)..];
            }

            paragraph.Inlines.Add(new LineBreak());
        }

        return paragraph;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
