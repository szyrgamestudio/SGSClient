using HtmlAgilityPack;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using RtfPipe;
using System.Net;
using System.Text;

namespace SGSClient.Helpers;
public static class HtmlToRichTextBlockConverter
{
    #region RichEditBox
    public static string GetHtml(this RichEditBox reb)
    {
        ArgumentNullException.ThrowIfNull(reb);
        reb.Document.GetText(TextGetOptions.FormatRtf, out string rtf);

        if (string.IsNullOrWhiteSpace(rtf))
            return string.Empty;

        string html = Rtf.ToHtml(rtf);
        return html;
    }
    public static void SetHtml(this RichEditBox reb, string html)
    {
        ArgumentNullException.ThrowIfNull(reb);

        if (string.IsNullOrWhiteSpace(html))
        {
            reb.Document.SetText(TextSetOptions.None, "");
            return;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        string rtf = @"{\rtf1\ansi\ansicpg1250\deff0\nouicompat\deflang1045" +
                     @"{\fonttbl{\f0\fnil\fcharset238 Segoe UI Variable;}}" +
                     @"{\colortbl ;\red255\green255\blue255;}" +
                     @"{\*\generator Riched20 3.1.0008}\viewkind4\uc1 " +
                     @"\cf1\f0\fs20 ";

        rtf += ParseNodeCollection(doc.DocumentNode.ChildNodes);
        rtf += "}";

        if (rtf.EndsWith(@"\par }"))
            rtf = rtf.Substring(0, rtf.Length - 6) + "}";

        reb.Document.SetText(TextSetOptions.FormatRtf, rtf);
    }
    private static string ParseNodeCollection(HtmlNodeCollection nodes)
    {
        StringBuilder sb = new();

        foreach (var node in nodes)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                string text = WebUtility.HtmlDecode(node.InnerText)
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Replace(@"\", @"\\")
                    .Replace("{", @"\{")
                    .Replace("}", @"\}");
                sb.Append(text);
            }
            else
            {
                string inner = ParseNodeCollection(node.ChildNodes);

                switch (node.Name.ToLower())
                {
                    case "b":
                    case "strong":
                        sb.Append(@"{\b " + inner + @"\b0}");
                        break;

                    case "i":
                    case "em":
                        sb.Append(@"{\i " + inner + @"\i0}");
                        break;

                    case "u":
                        sb.Append(@"{\ul " + inner + @"\ul0}");
                        break;

                    case "br":
                        sb.Append(@"\line ");
                        break;

                    case "p":
                        sb.Append(@"\pard\cf1\f0 " + inner + @"\par ");
                        break;

                    case "span":
                    case "div":
                        sb.Append(inner);
                        break;

                    case "a":
                        sb.Append(@"{\ul " + inner + @"\ul0}");
                        break;

                    default:
                        sb.Append(inner);
                        break;
                }
            }
        }

        return sb.ToString();
    }
    #endregion

    #region RichTextBlock
    public static void SetHtml(this RichTextBlock rtb, string html)
    {
        ArgumentNullException.ThrowIfNull(rtb);

        rtb.Blocks.Clear();
        if (string.IsNullOrWhiteSpace(html))
            return;

        try
        {
            html = html.Trim();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (var node in doc.DocumentNode.ChildNodes)
            {
                ProcessNode(rtb, node);
            }
        }
        catch
        {
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run { Text = WebUtility.HtmlDecode(html.Trim()) });
            rtb.Blocks.Add(paragraph);
        }
    }
    private static void ProcessNode(RichTextBlock rtb, HtmlNode node)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            var text = WebUtility.HtmlDecode(node.InnerText).Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run { Text = text });
                rtb.Blocks.Add(paragraph);
            }
        }
        else
        {
            switch (node.Name.ToLower())
            {
                case "p":
                    var paragraph = new Paragraph();
                    ParseNodeCollection(paragraph.Inlines, node.ChildNodes);
                    if (paragraph.Inlines.Count > 0)
                        rtb.Blocks.Add(paragraph);
                    break;

                case "div":
                    foreach (var child in node.ChildNodes)
                        ProcessNode(rtb, child);
                    break;

                default:
                    var defaultParagraph = new Paragraph();
                    ParseNodeCollection(defaultParagraph.Inlines, new HtmlNodeCollection(node) { node });
                    if (defaultParagraph.Inlines.Count > 0)
                        rtb.Blocks.Add(defaultParagraph);
                    break;
            }
        }
    }
    private static void ParseNodeCollection(InlineCollection inlines, HtmlNodeCollection nodes)
    {
        foreach (var node in nodes)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                var text = WebUtility.HtmlDecode(node.InnerText);
                if (!string.IsNullOrWhiteSpace(text))
                    inlines.Add(new Run { Text = text });
            }
            else
            {
                switch (node.Name.ToLower())
                {
                    case "b":
                    case "strong":
                        var bold = new Bold();
                        ParseNodeCollection(bold.Inlines, node.ChildNodes);
                        inlines.Add(bold);
                        break;

                    case "i":
                    case "em":
                        var italic = new Italic();
                        ParseNodeCollection(italic.Inlines, node.ChildNodes);
                        inlines.Add(italic);
                        break;

                    case "u":
                        var underline = new Underline();
                        ParseNodeCollection(underline.Inlines, node.ChildNodes);
                        inlines.Add(underline);
                        break;

                    case "br":
                        inlines.Add(new Microsoft.UI.Xaml.Documents.LineBreak());
                        break;

                    case "p":
                        // dodaj LineBreak tylko jeśli w środku nie ma innych elementów
                        bool hasContent = node.ChildNodes.Any(n => !string.IsNullOrWhiteSpace(n.InnerText));
                        if (hasContent && inlines.Count > 0)
                            inlines.Add(new Microsoft.UI.Xaml.Documents.LineBreak());

                        ParseNodeCollection(inlines, node.ChildNodes);

                        if (hasContent)
                            inlines.Add(new Microsoft.UI.Xaml.Documents.LineBreak());
                        break;

                    case "span":
                    case "div":
                        // przetwarzamy tylko dzieci, nie dodajemy dodatkowych LineBreak
                        ParseNodeCollection(inlines, node.ChildNodes);
                        break;

                    case "a":
                        var link = new Hyperlink();
                        link.NavigateUri = Uri.TryCreate(node.GetAttributeValue("href", ""), UriKind.Absolute, out var uri) ? uri : null;
                        ParseNodeCollection(link.Inlines, node.ChildNodes);
                        inlines.Add(link);
                        break;

                    default:
                        ParseNodeCollection(inlines, node.ChildNodes);
                        break;
                }
            }
        }
    }

    #endregion
}
