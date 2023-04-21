using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace DecklistManagement
{
    public class DecklistDownloader
    {
        public const string DECKLIST_FOLDER_NAME = "Decklists";
        public const string COMBINED_DECKLIST_FOLDER = "Combined";
        public const string PIONEER_DECKLIST_FOLDER = "Pioneer";
        public const string MODERN_DECKLIST_FOLDER = "Modern";

        public const string EXAMPLE_DECK_NAME = "FNM Hero - 51234.txt";
        public static string GetCombinedDecklistName(string archetypeName) => $"Combined{archetypeName}Decklist.txt";

        public const string PIONEER_MTGTOP8_URL = "https://www.mtgtop8.com/format?f=PI";
        public const string MODERN_MTGTOP8_URL = "https://www.mtgtop8.com/format?f=MO";

        public static readonly string[] BASIC_LAND_NAMES = new string[] { "Plains", "Island", "Swamp", "Mountain", "Forest", "Snow-Covered Plains", "Snow-Covered Island", "Snow-Covered Swamp", "Snow-Covered Mountain", "Snow-Covered Forest", "Wastes" };

        public static string GetDecklistFolderPath(string formatFolderName)
        {
            string path = Path.Combine(
                new string[] {
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    DECKLIST_FOLDER_NAME,
                    formatFolderName
                }
            );
            EnsurePathIsValid($"{path}//{EXAMPLE_DECK_NAME}");
            return path;
        }

        /*
         * Write a c# method to check if a path is valid, and if it doesn't, create as many folders as needed along the path to make it a valid path
         */
        public static void EnsurePathIsValid(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                throw new ArgumentException("The path must be an absolute path.");
            }

            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static async void DownloadPioneerDecklists()
        {
            await ScrapeAndDownloadArchetypesAsync(PIONEER_DECKLIST_FOLDER, PIONEER_MTGTOP8_URL);

            /*
            var pioneerDecklistPath = GetDecklistFolderPath(PIONEER_DECKLIST_FOLDER);
            ProcessDecklists(pioneerDecklistPath);
            */

            /*
            var url = "https://www.mtgtop8.com/archetype?a=920&meta=193&f=PI";
            await DownloadDecksAsync(url);
            */

            /*
            var url = "https://www.mtgtop8.com/format?f=PI";
            await ScrapeAndDownloadArchetypesAsync(url);
            */

            /*
            var url = "https://www.mtgtop8.com/mtgo?d=520454&f=Pioneer_Rakdos_Aggro_by_zarbo";
            var savePath = "C:\\Users\\PeterBeckfield\\Downloads\\Pioneer_Rakdos_Aggro_by_zarbo.txt";

            await DownloadFileAsync(url, savePath);
            */
        }

        public static async void DownloadModernDecklists()
        {
            await ScrapeAndDownloadArchetypesAsync(MODERN_DECKLIST_FOLDER, MODERN_MTGTOP8_URL);
        }

        public static async void CombinePioneerDecklists()
        {
            CombineDecklists(PIONEER_DECKLIST_FOLDER, COMBINED_DECKLIST_FOLDER);
        }

        public static async void CombineModernDecklists()
        {
            CombineDecklists(MODERN_DECKLIST_FOLDER, COMBINED_DECKLIST_FOLDER);
        }

        public static async void FindPioneerModernDecklistDifferences()
        {
            FindDecklistDifferences(
                Path.Combine(GetDecklistFolderPath(COMBINED_DECKLIST_FOLDER), GetCombinedDecklistName(PIONEER_DECKLIST_FOLDER)),
                Path.Combine(GetDecklistFolderPath(COMBINED_DECKLIST_FOLDER), GetCombinedDecklistName(MODERN_DECKLIST_FOLDER)),
                Path.Combine(GetDecklistFolderPath(COMBINED_DECKLIST_FOLDER), "asdf.txt")
            );
        }

        public static async void CalculatePioneerCompletionPercentage()
        {
            // TODO push this inside method
            var combinedFileName = Path.Combine(GetDecklistFolderPath(COMBINED_DECKLIST_FOLDER), "fdsa.txt");
            CalculateDeckCompletionPercentage(PIONEER_DECKLIST_FOLDER, combinedFileName);
        }

        public static async void CalculateModernCompletionPercentage()
        {
            // TODO push this inside method
            var combinedFileName = Path.Combine(GetDecklistFolderPath(COMBINED_DECKLIST_FOLDER), "fdsa.txt");
            CalculateDeckCompletionPercentage(MODERN_DECKLIST_FOLDER, combinedFileName);
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

        public static async Task ScrapeAndDownloadArchetypesAsync(string folder, string url)
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

                await DownloadDecksAsync(folder, archetypeUrl, archetypeName);
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
        public static async Task DownloadDecksAsync(string folder, string url, string archetypeName)
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            string cleanArchetypeName = archetypeName.Replace("/", string.Empty);
            cleanArchetypeName = cleanArchetypeName.Replace("\\", string.Empty);

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
                var folderPath = GetDecklistFolderPath(folder);
                var decklistName = $"{cleanArchetypeName} - {deckId}.txt";
                var savePath = Path.Combine(folderPath, decklistName);

                var response = await httpClient.GetAsync(deckUrl, HttpCompletionOption.ResponseHeadersRead);

                response.EnsureSuccessStatusCode();

                var contentStream = await response.Content.ReadAsStreamAsync();
                Console.WriteLine($"{decklistName} {savePath}");
                var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                await contentStream.CopyToAsync(fileStream);

                Console.WriteLine($"Deck {deckId} downloaded to {savePath}.");
                fileStream.Close();
            }
        }

        /*
         * Write a c# method that iterates through all of the text files in the following directory:

            C:\Users\PeterBeckfield\Desktop\Decklists

            and for each text file, first remove any line that says "Sideboard" or is an empty line.  Then for each line that is formatted like so:

            # card names

            store the card names in a global dictionary.

            Once all of the files have been iterated through and all of the card names have been added to the dictionary, save a new file called CombinedDecklist.txt that contains one line per card name
        */

        public static void CombineDecklists(string sourceFolderName, string destinationFolderName)
        {
            Dictionary<string, int> cardCount = new Dictionary<string, int>();

            var sourceFolderPath = GetDecklistFolderPath(sourceFolderName);
            var destinationFolderPath = GetDecklistFolderPath(destinationFolderName);

            var decklistFiles = Directory.GetFiles(sourceFolderPath, "*.txt");

            foreach (var decklistFile in decklistFiles)
            {
                var decklist = File.ReadAllLines(decklistFile);

                for (int i = 0; i < decklist.Length; i++)
                {
                    if (decklist[i] == "Sideboard" || string.IsNullOrWhiteSpace(decklist[i]))
                    {
                        decklist = decklist.Where((source, index) => index != i).ToArray();
                        i--;
                    }
                    else
                    {
                        (int, string) cardInfo = ExtractCardInfo(decklist[i]);
                        int cardAmount = cardInfo.Item1;
                        string cardName = cardInfo.Item2;

                        if (BASIC_LAND_NAMES.Contains(cardName))
                        {
                            continue;
                        }

                        // Depends on what we think is more important, raw card count or instances
                        // For now I think instances
                        if (cardCount.ContainsKey(cardName))
                        {
                            cardCount[cardName]++;
                            //cardCount[cardName]+= cardAmount;
                        }
                        else
                        {
                            cardCount[cardName] = 1;
                            //cardCount[cardName] = cardAmount;
                        }
                    }
                }
            }

            int minimumInstanceThreshold = 16;
            var trimmedCardCount = cardCount.Where(kv => kv.Value >= minimumInstanceThreshold).OrderByDescending(kv => kv.Value);
            //var combinedDecklist = string.Join(Environment.NewLine, trimmedCardCount.Select(kv => $"{kv.Value}x " + kv.Key.Trim()));
            var combinedDecklist = string.Join(Environment.NewLine, trimmedCardCount.Select(kv => kv.Key.Trim()));

            var savePath = Path.Combine(destinationFolderPath, GetCombinedDecklistName(sourceFolderName));
            File.WriteAllText(savePath, combinedDecklist);
            Console.WriteLine($"Combined decklist saved to {savePath}.");
        }

        // Takes all of the cards from the second decklist, and removes any cards that also exist in the first decklist
        public static void FindDecklistDifferences(string firstDecklistName, string secondDecklistName, string outputFileName)
        {
            var combinedFolderPath = GetDecklistFolderPath(COMBINED_DECKLIST_FOLDER);

            var firstDecklistPath = Path.Combine(combinedFolderPath, firstDecklistName);
            var secondDecklistPath = Path.Combine(combinedFolderPath, secondDecklistName);

            var firstDecklist = File.ReadAllLines(firstDecklistPath);
            var secondDecklist = File.ReadAllLines(secondDecklistPath);

            var diffLines = secondDecklist.Except(firstDecklist);

            Console.WriteLine($"First Decklist had {firstDecklist.Count()} cards.");
            Console.WriteLine($"Second Decklist had {secondDecklist.Count()} cards.");
            Console.WriteLine($"Difference has {diffLines.Count()} cards.");
            Console.WriteLine($"First Decklist plus Difference has {firstDecklist.Count() + diffLines.Count()} cards (overlap was {secondDecklist.Count() - diffLines.Count()} cards).");

            File.WriteAllLines(outputFileName, diffLines);
        }

        /*
         * Write a c# method that takes in an input file directory we'll call "Decklists" and an input file path called "Collection".

            Iterate through each file in the Decklists directory.  For each file, a list of cards will be written in the following format:

            # CardName

            The method should sum up the total number of cards in the file, and the total number of card names that exist in the Collection file.  Then output the percentage of cards in the decklist file that exist in the collection file
        */
        public static void CalculateDeckCompletionPercentage(string sourceFolderName, string collectionFilePath)
        {
            var decklistsDirectory = GetDecklistFolderPath(sourceFolderName);

            if (!Directory.Exists(decklistsDirectory))
            {
                Console.WriteLine("Decklists directory not found.");
                return;
            }

            if (!File.Exists(collectionFilePath))
            {
                Console.WriteLine("Collection file not found.");
                return;
            }

            List<int> missingCards = new List<int>();
            List<string> decklistCompletionPercentageLog = new List<string>();

            // Read the Collection file
            HashSet<string> collection = new HashSet<string>(File.ReadAllLines(collectionFilePath));
            foreach(string basicLand in BASIC_LAND_NAMES)
            {
                collection.Add(basicLand);
            }

            // Iterate through each file in the Decklists directory
            foreach (string decklistPath in Directory.GetFiles(decklistsDirectory))
            {
                string[] decklistLines = File.ReadAllLines(decklistPath);
                HashSet<string> missingCardNames = new HashSet<string>();

                int totalCardsInDecklist = 0;
                int cardsInCollection = 0;

                // Iterate through each line in the decklist file
                foreach (string line in decklistLines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.Equals("Sideboard"))
                    {
                        continue;
                    }

                    (int, string) cardInfo = ExtractCardInfo(line);
                    int cardAmount = cardInfo.Item1;
                    string cardName = cardInfo.Item2;

                    totalCardsInDecklist += cardAmount;
                    if (collection.Contains(cardName))
                    {
                        cardsInCollection += cardAmount;
                    }
                    else
                    {
                        missingCardNames.Add(cardName);
                    }
                }

                if (totalCardsInDecklist == 0)
                {
                    Console.WriteLine("No cards found in decklist.");
                }
                else
                {
                    var cardsMissing = totalCardsInDecklist - cardsInCollection;
                    double percentage = (double)cardsMissing / totalCardsInDecklist * 100;
                    //var decklistNumber = ExtractNumbersFromFilePath(decklistPath);
                    var percentageLogString = $"Percentage of cards {Path.GetFileName(decklistPath)} missing: {cardsMissing} ({percentage:0.00}%)";
                    var missingCardString = $"Missing Cards, {string.Join(",", missingCardNames)}";
                    Console.WriteLine($"{percentageLogString}\t\t{missingCardString}");

                    missingCards.Add(cardsMissing);
                    decklistCompletionPercentageLog.Add($"{percentageLogString},{missingCardString}");
                }
            }
            ExportIntegersToCsv(missingCards, $"missing{sourceFolderName}Cards.csv");
            File.WriteAllLines($"missing{sourceFolderName}CardsFromDeckLog.csv", decklistCompletionPercentageLog);
        }

        //Given a string like: C:\Users\peter\Desktop\Decklists\Pioneer\Weenie White - 520945.txt write a c# method to extract the numbers right before .txt
        public static int ExtractNumbersFromFilePath(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var match = Regex.Match(fileName, @"(\d+)$");

            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            else
            {
                throw new ArgumentException("The file path does not contain a number before the .txt extension.");
            }
        }

        // Write a c# method that takes a list of integers and exports them as a file that can be imported to google sheets with a bar graph of the count of each integer
        public static void ExportIntegersToCsv(List<int> integers, string outputFilePath)
        {
            if (integers == null || integers.Count == 0)
            {
                Console.WriteLine("Empty list of integers.");
                return;
            }

            // Count the occurrences of each integer
            Dictionary<int, int> integerCounts = new Dictionary<int, int>();
            foreach (int number in integers)
            {
                if (integerCounts.ContainsKey(number))
                {
                    integerCounts[number]++;
                }
                else
                {
                    integerCounts[number] = 1;
                }
            }

            // Sort the integers
            var sortedIntegers = integerCounts.Keys.ToList();
            sortedIntegers.Sort();

            // Create the CSV data
            List<string> csvLines = new List<string> { "Missing Cards,Deck Count" };
            foreach (int number in sortedIntegers)
            {
                csvLines.Add($"{number},{integerCounts[number]}");
            }

            // Write the CSV data to the output file
            File.WriteAllLines(outputFilePath, csvLines);

            Console.WriteLine($"Exported integers to CSV file: {outputFilePath}");
        }

        public static bool IsDigitOrWhitespace(char c)
        {
            return char.IsDigit(c) || char.IsWhiteSpace(c);
        }

        public static (int, string) ExtractCardInfo(string line)
        {
            var match = Regex.Match(line, @"^(\d+)\s+(.*)$");

            if (match.Success)
            {
                var count = int.Parse(match.Groups[1].Value);
                var cardName = match.Groups[2].Value;
                return (count, cardName);
            }
            else
            {
                return (1, line);
            }
        }
    }
}
