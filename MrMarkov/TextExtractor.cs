using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace MrMarkov
{
    // Adapted from https://lotsacode.wordpress.com/2011/02/11/extract-only-viewable-text-from-html-with-c/
    // with thanks
    class TextExtractor
    {
        private static Regex _removeRepeatedWhitespaceRegex = new Regex(@"(\s|\n|\r){2,}", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public static string ExtractViewableTextCleaned(HtmlNode node)
        {
            string textWithLotsOfWhiteSpaces = ExtractViewableText(node);
            return _removeRepeatedWhitespaceRegex.Replace(textWithLotsOfWhiteSpaces, " ");
        }

        public static string ExtractViewableText(HtmlNode node)
        {
            StringBuilder sb = new StringBuilder();
            ExtractViewableTextHelper(sb, node);
            return sb.ToString();
        }

        private static void ExtractViewableTextHelper(StringBuilder sb, HtmlNode node)
        {
            if (node.Name != "script" && node.Name != "style")
            {
                if (node.NodeType == HtmlNodeType.Text)
                {
                    AppendNodeText(sb, node);
                }

                foreach (HtmlNode child in node.ChildNodes)
                {
                    ExtractViewableTextHelper(sb, child);
                }
            }
        }

        private static bool Valid(string s)
        {
            s = s.ToLower();
            string allowed = "abcdefghijklmnopqrstuvwxyz':,.()";
            if (s.Length == 1 && s != "a" && s != "i")
            {
                return false;
            }
            foreach (var c in s)
            {
                if (!allowed.Contains(c)) return false;
            }
            return true;
        }

        private static void AppendNodeText(StringBuilder sb, HtmlNode node)
        {
            string text = ((HtmlTextNode) node).Text;
            
            if (string.IsNullOrWhiteSpace(text) == false && Valid(text))
            {
                sb.Append(text);

                // If the last char isn't a white-space, add a white space
                // otherwise words will be added ontop of each other when they're only separated by
                // tags
                if (text.EndsWith("\t") || text.EndsWith("\n") || text.EndsWith(" ") || text.EndsWith("\r"))
                {
                    // We're good!
                }
                else
                {
                    sb.Append(" ");
                }
            }
        }
    }
}
