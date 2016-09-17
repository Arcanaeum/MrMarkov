using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace MrMarkov
{
    // From: https://www.dotnetperls.com/scraping-html
    // with thanks

    public struct LinkItem
    {
        public string Href;
        public string Text;

        public override string ToString()
        {
            return Href;
        }
    }

    static class LinkFinder
    {
        public static List<LinkItem> Extract(string fromUrl)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            WebClient webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.UserAgent] = "MrMarkov 1.0";
            try
            {
                string pageContent = webClient.DownloadString(fromUrl);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(pageContent);

                List<LinkItem> list = new List<LinkItem>();
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    var href = link.Attributes["href"].Value;
                    if (!href.Contains("/"))
                    {
                        href = "/" + href;
                    }
                    if (href == "/")
                    {
                        href = fromUrl;
                    }
                    else if (href.Substring(0, 2) == "//")
                    {
                        if (fromUrl.Substring(0, 5) == "http:")
                        {
                            href = "http:" + href;
                        }
                        else
                        {
                            href = "https:" + href;
                        }
                    }
                    else if (href[0] == '/')
                    {
                        var url = fromUrl;
                        var id = url.IndexOfNth("/", 0, 3);
                        url = url.Substring(0, id);
                        href = url + href;
                    }
                    var li = new LinkItem
                    {
                        Href = href,
                        Text = ""
                    };
                    list.Add(li);
                }
/*
            // 1.
            // Extract all matches in file.
                MatchCollection m1 = Regex.Matches(pageContent, @"(<a.*?>.*?</a>)",
                    RegexOptions.Singleline);

            // 2.
            // Loop over each match.
            foreach (Match m in m1)
            {
                string value = m.Groups[1].Value;
                LinkItem i = new LinkItem();

                // 3.
                // Get href attribute.
                Match m2 = Regex.Match(value, @"href=\""(.*?)\""",
                RegexOptions.Singleline);
                if (m2.Success)
                {
                    // TODO: Clean this ugly crap
                    var href = m2.Groups[1].Value;
                    
                    i.Href = href;
                }

                // 4.
                // Remove inner tags from text.
                string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
                RegexOptions.Singleline);
                i.Text = t;

                list.Add(i);
            }*/

            return list;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " (URL: " + fromUrl + ")");
            }
            return new List<LinkItem>();
        }
    }

}
