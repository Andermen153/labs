using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace L4
{
    class Program
    {
        static void PrintMails()
        {
            WebClient client = new WebClient();
            string page = client.DownloadString(new Uri("https://www.susu.ru/"));

            File.WriteAllText(@"page.txt", page);
            
            //поиск и печать адресов
            var Mails = (from href in Regex.Matches(page, @"([-\w]+\[dot\])*[-\w]+\[at\][-\w]+(\[dot\][-\w]+)+").Cast<Match>()
                         let url = href.Value.Replace("[at]", "@").Replace("[dot]", ".")
                         select new
                         {
                             Mail = url
                         }
                ).ToList();
            foreach (var mail in Mails)
                Console.WriteLine(String.Concat(Enumerable.Repeat("--|", 0)) + ">>>" + mail.Mail);
        }

        public class WebScnner : IDisposable
        {
            private readonly HashSet<Uri> _procLinks = new HashSet<Uri>();
            private readonly WebClient _webClient = new WebClient();

            private readonly HashSet<string> _ignoreFiles = new HashSet<string> { ".ico", ".xml" };

            private void OnTargetFound(Uri page, Uri[] links)
            {
                TargetFound?.Invoke(page, links);
            }
            private void Process(string domain, Uri page, int count)
            {
                if (count <= 0) return;

                if (_procLinks.Contains(page)) return;
                _procLinks.Add(page);

                string html = _webClient.DownloadString(page);

                var hrefs = (from href in Regex.Matches(html, @"href=""[\/\w-\.:]+""").Cast<Match>()
                             let url = href.Value.Replace("href=", "").Trim('"')
                             let loc = url.StartsWith("/")
                             select new
                             {
                                 Ref = new Uri(loc ? $"{domain}{url}" : url),
                                 IsLockal = loc || url.StartsWith(domain)
                             }
                             ).ToList();

                var externals = (from href in hrefs
                                 where !href.IsLockal
                                 select href.Ref).ToArray();

                if (externals.Length > 0) OnTargetFound(page, externals);

                var lockals = (from href in hrefs
                               where href.IsLockal
                               select href.Ref).ToList();

                foreach (var href in lockals)
                {
                    string fileEx = Path.GetExtension(href.LocalPath).ToLower();
                    if (_ignoreFiles.Contains(fileEx)) continue;

                    Process(domain, href, --count);
                }
            }

            public event Action<Uri, Uri[]> TargetFound;

            public void Scan(Uri startPage, int pageCount)
            {
                _procLinks.Clear();

                string domain = $"{startPage.Scheme}://{startPage.Host}";
                Process(domain, startPage, pageCount);
            }

            public void Dispose()
            {
                _webClient.Dispose();
            }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            using (WebScnner scanner = new WebScnner())
            {
                scanner.TargetFound += (page, links) =>
                {
                    Console.WriteLine($"\nPage:\n\t{page}\nLinks:");

                    foreach (var link in links)
                        Console.WriteLine($"\t{link}");
                };

                scanner.Scan(new Uri("https://www.susu.ru/"), 10);
                Console.WriteLine("Done.");
                
            }

            Console.WriteLine($"\nMails:");
            PrintMails();
            Console.ReadKey(true);
        }
    }
}


