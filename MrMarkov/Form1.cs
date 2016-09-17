using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Boilerpipe.Net.Extractors;

namespace MrMarkov
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        private List<string> unvisited;
        private List<string> visited;
        private Dictionary<string, List<string>> corpus;
        private object locker1, locker2;
        private Random rnd;
        private bool run;

        private void CrawlRandomUrl()
        {
            string str;
            lock (locker1)
            {
                if (unvisited.Count == 0)
                {
                    run = false;
                    return;
                }
                var i = rnd.Next(unvisited.Count);
                str = unvisited[i];
                unvisited.Remove(str);
                visited.Add(str);
            }
            foreach (LinkItem i in LinkFinder.Extract(str))
            {
                lock (locker1)
                {
                    if (i.Href != null && !visited.Contains(i.Href) && !unvisited.Contains(i.Href))
                    {
                        unvisited.Add(i.Href);
                    }
                }
                /*MessageBox.Show(text);*/
            }
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                WebClient wc = new WebClient();
                wc.Headers[HttpRequestHeader.UserAgent] = "MrMarkov 1.0";
                var doc = wc.DownloadString(str);
                var txt = CommonExtractors.ArticleExtractor.GetText(doc);
                // document.LoadHtml(doc);
                var text = txt.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                // var text = TextExtractor.ExtractViewableTextCleaned(document.DocumentNode).Split(' ');
                var order = (int)numericUpDown1.Value;
                for (int i = 0; i <= text.Length - 2*order; i++)
                {
                    if (true || i != text.Length - order)
                    {
                        var word = "";
                        var follow = "";
                        for (var j = 0; j < order; j++)
                        {
                            word += WebUtility.HtmlDecode(text[i + j]).Replace("\n", "").Replace("\r", "").Replace(" ", "") + " ";
                            
                            follow += WebUtility.HtmlDecode(text[i + j + 1]).Replace("\n", "").Replace("\r", "").Replace(" ", "") + " ";
                            
                        }
                        if (word == "" || follow == "") continue;
                        if (corpus.ContainsKey(word))
                        {
                            corpus[word].Add(follow);
                        }
                        else
                        {
                            corpus[word] = new List<string> {follow};
                        }
                        if (i == text.Length - 2*order && !corpus.ContainsKey(follow))
                        {
                            var el = corpus.ElementAt(rnd.Next(corpus.Count));
                            var key = el.Key;
                            corpus[follow] = new List<string> {key};
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            label2.Invoke(() => label2.Text = $"Unvisited: {unvisited.Count}, Visited: {visited.Count}\n" +
            $"Corpus: {corpus.Count} words");
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            run = false;
            button1.Enabled = true;
            button2.Enabled = false;
            numericUpDown1.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                var el = corpus.ElementAt(rnd.Next(corpus.Count));
                var key = el.Key;
                var s = key.Split(' ')[0].FirstCharToUpper();
                for (int i = 0; i < 100; i++)
                {
                    var inn = corpus[key];
                    var t = inn[rnd.Next(inn.Count - 1)];
                    if (!string.IsNullOrEmpty(t))
                    {
                        key = t;
                    }
                    s += " " + key.Split(' ')[0];
                }
                MessageBox.Show(s);
            }
            catch
            {
                MessageBox.Show("Error");
            }
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            numericUpDown1.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = true;
            unvisited = new List<string>();
            visited = new List<string>();
            locker1 = new object();
            locker2 = new object();
            corpus = new Dictionary<string, List<string>>();
            rnd = new Random();
            run = true;
            unvisited.Add(textBox1.Text);
            CrawlRandomUrl();
            for (int i = 0; i < 4; i++)
            {
                Thread t = new Thread(() =>
                {
                    while (run)
                    {
                        CrawlRandomUrl();
                    }
                });
                t.Start();
            }
        }
    }
}
