using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var html = new HtmlWeb();
                string url = "https://nha.chotot.com/mua-ban-nha-dat-thanh-pho-thu-dau-mot-binh-duong";
                var document = html.Load(url);
                int count = 0;
                var houses = document.DocumentNode.SelectNodes("//div[@class='list-view']/div/div/ul/div/li");
                crawlData(houses);
                stopwatch.Stop();
                Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);


                //string uri = "https://nha.chotot.com/mua-ban-nha-dat-thanh-pho-thu-dau-mot-binh-duong";
                //HttpClient http = new HttpClient();
                //http.BaseAddress = new Uri(uri);
                //var res = http.GetStringAsync("").Result;
                //Console.WriteLine(res);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void crawlData(HtmlNodeCollection houses)
        {

            int count = 0;
            foreach (var house in houses)
            {
                Thread currentThreat = new Thread(()=> getRealEstate(house));
                currentThreat.Start();
                count++;
                if (count == 100)
                    break;
            }
        }
        public static void getRealEstate (HtmlNode house)
        {
            var node = house.SelectSingleNode(".//a");

            if (node != null)
            {
                string uri = "https://nha.chotot.com" + node.Attributes["href"].Value;

                //html of a house link
                HttpClient http = new HttpClient();
                http.BaseAddress = new Uri(uri);
                var res =  http.GetStringAsync("").Result;

                var title = Regex.Match(res, @"(<h1 class=\""AdDecription_adTitle__2I0VE\"" [\b\D]*>)([\s].*?[\s]*)(?=<\/h1>)", RegexOptions.Singleline).Value.Replace("<h1 class=\"AdDecription_adTitle__2I0VE\" itemProp=\"name\"> <!-- -->", "");
                var acreage = Regex.Match(res, @"(\""AdParam_adParamValue__1ayWO\""\>)(.*?)(?=<\/span>)", RegexOptions.Singleline).Value.Replace("\"AdParam_adParamValue__1ayWO\">", "");
                var price = Regex.Match(res, @"(<span class=\""AdDecription_price__O6z15\"">)[\w\D]*(?=<span class=\""AdDecription_squareMetre__2KYh8\"">)", RegexOptions.Singleline).Value.Replace("<span class=\"AdDecription_price__O6z15\"><span itemProp=\"price\">", "");
            }
        }
    }
}
