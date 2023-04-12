using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace DecklistManagement
{
    public class DecklistDownloader
    {
        public static async void DownloadPioneerDecklists()
        {
            /*
            var url = "https://www.mtgtop8.com/archetype?a=920&meta=193&f=PI";
            await DownloadDecksAsync(url);
            */

            
            var url = "https://www.mtgtop8.com/format?f=PI";
            await ScrapeAndDownloadArchetypesAsync(url);
            

            /*
            var url = "https://www.mtgtop8.com/mtgo?d=520454&f=Pioneer_Rakdos_Aggro_by_zarbo";
            var savePath = "C:\\Users\\PeterBeckfield\\Downloads\\Pioneer_Rakdos_Aggro_by_zarbo.txt";

            await DownloadFileAsync(url, savePath);
            */
        }

        /* GPT 3.5 Prompt:
         * Write a c# method that downloads the file located at the following URL:
            https://www.mtgtop8.com/mtgo?d=520454&f=Pioneer_Rakdos_Aggro_by_zarbo
            and saves it to disk
        */

        // Had to remove some C# 8.0 stuff and fix file path since it dies if the path provided doesn't exist
        public static async Task DownloadFileAsync(string url, string savePath)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var contentStream = await response.Content.ReadAsStreamAsync();
            var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            await contentStream.CopyToAsync(fileStream);

            fileStream.Close();
        }

        /* GPT 3.5 Prompt:
         * Write a C# method that navigates to the following url:

            https://www.mtgtop8.com/format?f=PI

            And for each of the archetypes listed on the left side of the page, print to the console the following information:

            Archetype Name - URL of Link

            For example:

            Rakdos Aggro - https://www.mtgtop8.com/archetype?a=920&meta=193&f=PI
         * */


        // Update some C#8.0 code, Add HtmlAgilityPack through NuGetManager, fix ambiguous reference
        // Selector was completely broken, needed to inspect the element and baby it:

        // Write a css selector that matches any link that begins like this: a[href='archetype?
        // Convert this css selector to an xpath: a[href^='archetype?']

        public static async Task ScrapeAndDownloadArchetypesAsync(string url)
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var archetypeNodes = doc.DocumentNode.SelectNodes("//a[starts-with(@href, 'archetype?')]");

            foreach (var archetypeNode in archetypeNodes)
            {
                var archetypeName = archetypeNode.InnerText.Trim();
                var archetypeUrl = "https://www.mtgtop8.com/" + archetypeNode.GetAttributeValue("href", "");

                Console.WriteLine($"{archetypeName} - {archetypeUrl}");

                await DownloadDecksAsync(archetypeUrl);
            }
        }

        /*
         * Write a c# method that takes a page like this one:

            https://www.mtgtop8.com/archetype?a=920&meta=193&f=PI

            Iterates through each of the deck links on the right side of the page, for example:

            https://www.mtgtop8.com/event?e=43388&d=520454&f=PI

            and for each of those deck links, take what we will refer to as the DECK_ID (520454 for example), and save the file to disk located at the following url:

            https://www.mtgtop8.com/mtgo?d=DECK_ID&f=DECK_ID.txt
        */

        // Some minor updates, fixed xpath
        public static async Task DownloadDecksAsync(string url)
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var deckNodes = doc.DocumentNode.SelectNodes("//a[starts-with(@href, 'event?')]");

            if (deckNodes == null)
            {
                Console.WriteLine("No decks found on the page.");
                return;
            }

            var deckIds = new List<string>();

            foreach (var deckNode in deckNodes)
            {
                var href = deckNode.GetAttributeValue("href", "");
                var match = Regex.Match(href, @"d=(\d+)");

                if (match.Success)
                {
                    var deckId = match.Groups[1].Value;
                    deckIds.Add(deckId);
                }
            }

            foreach (var deckId in deckIds)
            {
                var deckUrl = $"https://www.mtgtop8.com/mtgo?d={deckId}&f={deckId}.txt";
                var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Decklists\\{deckId}.txt");

                var response = await httpClient.GetAsync(deckUrl, HttpCompletionOption.ResponseHeadersRead);

                response.EnsureSuccessStatusCode();

                var contentStream = await response.Content.ReadAsStreamAsync();
                var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                await contentStream.CopyToAsync(fileStream);

                Console.WriteLine($"Deck {deckId} downloaded to {savePath}.");
                fileStream.Close();
            }
        }
    }
}
