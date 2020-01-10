using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PRS_ExtractKeyPhrases
{
    class Program
    {

  
        static void Main(string[] args)
        {
            var prsData = GetCSVData();
            var updatedData = ScrapeySracpeFace(prsData).Result;
            SaveCSVData(updatedData);
            Console.ReadLine();
        }

        protected static IEnumerable<PRSData> GetCSVData()
        {
            using var reader = new StreamReader(@"C:\temp\small-data.csv");
            using var csv = new CsvReader(reader);
            return csv.GetRecords<PRSData>();
        }

        protected static void SaveCSVData(IEnumerable<PRSData> prsData)
        {
            using var writer = new StreamWriter(@"c:\temp\output.csv");
            using var csv = new CsvWriter(writer);
            csv.WriteRecords(prsData);
        }
        protected static async Task<IEnumerable<PRSData>> ScrapeySracpeFace(IEnumerable<PRSData> prsData)
        {
            int counter = 1;
            foreach (var record in prsData.ToList())
            {
                string siteUrl = record.Url;


                CancellationTokenSource cancelationToken = new CancellationTokenSource();
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage request = await httpClient.GetAsync(siteUrl);
                cancelationToken.Token.ThrowIfCancellationRequested();

                Stream response = await request.Content.ReadAsStreamAsync();
                cancelationToken.Token.ThrowIfCancellationRequested();

                HtmlParser parser = new HtmlParser();
                IHtmlDocument document = parser.ParseDocument(response);

                record.ArtlcieCopy = GetScrapeResults(document);

                counter++;
                Console.WriteLine("procecessed: " + counter);

            }


            return prsData.ToList();


        }

        private static string GetScrapeResults(IHtmlDocument document)
        {
            var article = document.All.Where(x => x.ClassName == "article-content").FirstOrDefault().GetElementsByTagName("p");
            StringBuilder pageCopy = new StringBuilder();
            foreach (var element in article)
            {
                pageCopy.Append(element.Html().Replace(Environment.NewLine, " "));
            }

            return pageCopy.ToString();

        }
    }
}
