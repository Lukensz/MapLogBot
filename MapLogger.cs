using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace MapLogBot
{
    class MapLogger
    {
        private const string Api = "http://zeta.pokemon-vortex.com/includes/feed.php";
        private static readonly HttpClient Client = new HttpClient();
        private string _lastValue;
        private string _previousValue;
        private const string LogFile = "MapLogs.txt";

        protected async void Request()
        {
            try
            {
                var response = await Client.GetStringAsync(Api);
                var parsedContent = GetPlainTextFromHtml(response);
                if (parsedContent.Contains("CloudFlare"))
                {
                    return;
                }
                if (parsedContent != _lastValue && parsedContent != _previousValue)
                {
                    _previousValue = _lastValue;
                    _lastValue = parsedContent;
                    var time = DateTime.Now;
                    var username = parsedContent.Split(' ')[0];
                    var poke = response.Split(new [] { " caught a:</strong></td></tr><tr><td><strong>" }, StringSplitOptions.None)[1].Split(new [] { " <" }, StringSplitOptions.None)[0];
                    var level = parsedContent.Split(' ').Last();
                    Console.WriteLine("[" + time + "]" + " " + username + " - " + poke + " - Level " + level);
                    using (StreamWriter w = File.AppendText(LogFile))
                    {
                        w.WriteLine("[" + time + "]" + " " + username + " - " + poke + " - Level " + level);
                    }
                }
            }
            catch (HttpRequestException) { }
        }

        private string GetPlainTextFromHtml(string htmlString)
        {
            string htmlTagPattern = "<.*?>";
            var regexCss = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            htmlString = regexCss.Replace(htmlString, string.Empty);
            htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
            htmlString = Regex.Replace(htmlString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
            htmlString = htmlString.Replace("&nbsp;", string.Empty);

            return htmlString;
        }

        static void Main(string[] args)
        {
            MapLogger logger = new MapLogger();
            while (true)
            {
                logger.Request();
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
