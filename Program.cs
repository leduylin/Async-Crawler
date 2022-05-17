using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

using System.Collections.Generic;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using HtmlAgilityPack;
using System.Net.Http;
using System.Reflection.Metadata;

namespace UrlHouses
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // List<String> urls = getAllURL();
            CrawlDataUsingAgilityPack();
            Console.ReadLine();

        }

        static int countHouse = 0;
        //private static List<String> getAllURL()
        //{
        //    List<string> listUrl = new List<string>();
        //    IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(Environment.CurrentDirectory);
        //    driver.Manage().Window.Minimize();
        //    driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);
        //    int countThread = 0;
        //    int countHouse = 0;
        //    // get houses URLs 
        //    try
        //    {
        //        driver.Url = "https://nha.chotot.com/mua-ban-bat-dong-san";

        //        int totalOfPages = 1;
        //        while (true)
        //        {
        //            driver.Url = String.Format("https://nha.chotot.com/mua-ban-bat-dong-san?page={0}", totalOfPages);
        //            bool existPage = driver.PageSource.Contains("NotFound_notFoundWrapper__1Erzd");
        //            if (!existPage)
        //            {
        //                new WebDriverWait(driver, new TimeSpan(0, 0, 1500)).Until(c => c.FindElement(By.ClassName("list-view")));
        //                IList<IWebElement> aTags = driver.FindElements(By.XPath("//div[@class='list-view']/div/div/ul/div/li/a"));
        //                int ccountHouse = 0;
        //                foreach (IWebElement element in aTags)
        //                {
        //                    string urlHouse = element.GetAttribute("href");
        //                    if (urlHouse != null)
        //                    {
        //                        listUrl.Add(urlHouse);
        //                    }
        //                    ccountHouse++;
        //                    if(countHouse > 50)
        //                    {
        //                        Thread thread = new Thread(()=>CrawlData(listUrl, countThread));
        //                        thread.Start();
        //                        countThread++;

        //                    }

        //                }
        //                totalOfPages++;
        //                if (totalOfPages > 2)
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //        driver.Close();
        //        driver = new ChromeDriver();
        //        driver.Dispose();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }
        //    return listUrl;
        //}

        //Crawl all the houses
        private static async Task CrawlAllData(HtmlNodeCollection houses, int count)
        {
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Start thread: " + count);
            int dem = 0;
            List<Task<String>> awaitingTasks = new List<Task<string>>();
            foreach (var house in houses)
            {
                dem++;
                var node = house.SelectSingleNode(".//a");
                var task = CrawlAHouse(node, count);
                awaitingTasks.Add(task);
                await Task.WhenAny(awaitingTasks);
            }
            await Task.WhenAll(awaitingTasks);
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("End thread: " + count);

        }

        //Crawl each house
        private static async Task<string> CrawlAHouse(HtmlNode node, int thread)
        {
            return await Task.Run(() =>
            {
                // GetInforUsingSelenium(url, dem);
                GetInforEachHouseUsingAgilityPack(node, thread);
                return "Done";
            });

        }

        //Get information each house

        private static void CrawlDataUsingAgilityPack()
        {
            try
            {
                var htmlWeb = new HtmlWeb();
                string url = "https://nha.chotot.com/mua-ban-bat-dong-san-phuong-chanh-nghia-thanh-pho-thu-dau-mot-binh-duong";
                HtmlDocument document = htmlWeb.Load(url);

                int totalOfPages = 1;
                int totalOfHouses = 0;
                int countThread = 0;
                while (true)
                {
                    url = String.Format("https://nha.chotot.com/mua-ban-bat-dong-san?page={0}", totalOfPages);
                    document = htmlWeb.Load(url);

                    bool isFinalPage = document.ToString().Contains("NotFound_notFoundWrapper__1Erzd");

                    if (!isFinalPage)
                    {
                        var houses = document.DocumentNode.SelectNodes("//div[@class='list-view']/div/div/ul/div/li");
                        countThread++;
                        Thread thread = new Thread(() => CrawlAllData(houses, countThread));
                        thread.Start();
                        totalOfPages++;

                    }
                    else
                    {
                        break;
                    }

                    if (totalOfPages > 100)
                    {
                        break;
                    }
                }

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private static void GetInforEachHouseUsingAgilityPack(HtmlNode node, int thread)
        {
            countHouse++;
            if (node != null)
            {
                string uri = "https://nha.chotot.com" + node.Attributes["href"].Value;
                var htmlWeb = new HtmlWeb();
                HtmlDocument document = htmlWeb.Load(uri);

                bool isExisted = document.ToString().Contains("NotFound_notFoundWrapper__1Erzd");
                if (!isExisted)
                {
                    //html of a house link 
                    HttpClient httpHouse = new HttpClient();
                    httpHouse.BaseAddress = new Uri(uri);
                    var res = httpHouse.GetStringAsync("").Result;

                    //get information of house
                    //.Replace("<h1 class=\"AdDecription_adTitle__2I0VE\" itemProp=\"name\"> <!-- -->", "");
                    var title = Regex.Match(res, @"(?<=<h1 class=\""AdDecription_adTitle__2I0VE\"" [\b\D\W]*\-\->)(.*)(?=<\/h1>)", RegexOptions.Singleline).Value;
                    //var address = Regex.Match(res, @"");
                    var acreage = Regex.Match(res, @"(?<=\""AdParam_adParamValue__1ayWO\""\>)(.*?)(?=<\/span>)", RegexOptions.Singleline).Value;
                    var price = Regex.Match(res, @"(?<=<span class=\""AdDecription_price__O6z15\""><span itemProp=\""price\"">)[\w\D]*(?=<span class=\""AdDecription_squareMetre__2KYh8\"">)", RegexOptions.Singleline).Value;
                    var description = Regex.Match(res, @"(?<=<p class=\""AdDecription_adBody__1c8SG\"" [\b\D\W]*tion\"">)(.*)(?=<\/p>)", RegexOptions.Singleline).Value;

                    Console.WriteLine("In thread: " + thread + ", house " + countHouse + " :" + title + "\n" + acreage + "\n" + price + "\n");
                }
            }
        }
        private static void GetInforUsingSelenium(string urlHouse, int dem)
        {
            try
            {
                IWebDriver childDriver = new OpenQA.Selenium.Chrome.ChromeDriver(Environment.CurrentDirectory);
                childDriver.Manage().Window.Minimize();
                childDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(1000);

                try
                {
                    childDriver.Url = urlHouse;
                    dem++;
                    //get house's information
                    var title = childDriver.FindElement(By.CssSelector(".AdDecription_adDecriptionWrapper__36qgN>.AdDecription_adTitle__2I0VE")).Text;
                    var area = childDriver.FindElement(By.CssSelector(".ct-detail .AdDecription_adDecriptionWrapper__36qgN .AdDecription_squareMetre__2KYh8")).Text;
                    var address = childDriver.FindElement(By.CssSelector(".ct-detail .AdParam_address__1pmCR>span")).Text;
                    address = Regex.Match(address, @"[\w\D]*(?=.{10})", RegexOptions.Singleline).Value;
                    var price = childDriver.FindElement(By.CssSelector(".ct-detail .AdDecription_adDecriptionWrapper__36qgN .AdDecription_price__O6z15>span")).Text;
                    price = Regex.Match(price, @"[0-9]*\s.*(?=\-)", RegexOptions.Singleline).Value;
                    var telephone = childDriver.FindElement(By.CssSelector("div.LeadButtonSlide_wrapperLeadButton__2Ytpy")).GetAttribute("innerHTML");
                    telephone = Regex.Match(telephone.ToString(), @"(?<=\""tel\:)[\d]*(?=\"">)", RegexOptions.Singleline).Value;
                    var description = childDriver.FindElement(By.CssSelector(".ct-detail  .AdDecription_adDecriptionWrapper__36qgN .AdDecription_adBody__1c8SG")).Text;

                    //get house's image
                    var btnImg = childDriver.FindElements(By.CssSelector(".AdImage_adImageWrapper__15vl2 .slick-dots>li")).Count;
                    List<string> imgs = new List<string>();
                    while (btnImg > 0)
                    {
                        IWebElement btn = childDriver.FindElement(By.CssSelector(".AdImage_adImageWrapper__15vl2 .slick-slider .AdImage_Next__26U9L"));
                        btn.Click();
                        btnImg--;

                        var imgTags = childDriver.FindElement(By.CssSelector(".ct-detail .AdImage_sliderImage__3REqQ .AdImage_imageWrapper__1yIxE")).GetAttribute("innerHTML");
                        imgTags = Regex.Match(imgTags, @"(?<=src=\"")https:\/\/cdn\.chotot\.com(.*)\.jpg(?=\"")", RegexOptions.Singleline).Value;
                        imgs.Add(imgTags);
                    }

                    Console.WriteLine("\n\n" + title + "\n" + address + "\n" + area + "\n" + price + "\n" + telephone + "\n" + description + "\n\n");

                    childDriver.Close();
                    Thread.Sleep(2000);
                    childDriver.Dispose();

                    Console.WriteLine(dem);

                }
                catch (Exception ec)
                {
                    Console.WriteLine("Child driver " + ec.Message);
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}




