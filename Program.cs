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

namespace UrlHouses
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var a = DateTime.Now.Ticks;

            List<string> listUrl = new List<string>();
            IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(Environment.CurrentDirectory);
            driver.Manage().Window.Minimize();
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);

            try
            {
                driver.Url = "https://nha.chotot.com/mua-ban-bat-dong-san";

                int totalOfPages = 1;
                while (true)
                {
                    driver.Url = String.Format("https://nha.chotot.com/mua-ban-bat-dong-san?page={0}", totalOfPages);
                    bool existPage = driver.PageSource.Contains("NotFound_notFoundWrapper__1Erzd");
                    if (!existPage)
                    {
                        new WebDriverWait(driver, new TimeSpan(0, 0, 1500)).Until(c => c.FindElement(By.ClassName("list-view")));
                        IList<IWebElement> aTags = driver.FindElements(By.XPath("//div[@class='list-view']/div/div/ul/div/li/a"));
                        int ccountHouse = 0;
                        foreach (IWebElement element in aTags)
                        {
                            string urlHouse = element.GetAttribute("href");
                            if (urlHouse != null)
                            {
                                listUrl.Add(urlHouse);
                            }
                            ccountHouse++;
                            if (ccountHouse > 10)
                            {
                                break;
                            }
                        }

                        totalOfPages++;

                        if (totalOfPages > 2)
                        {
                            break;
                        }
                    }
                }
                driver.Close();
                driver = new ChromeDriver();
                driver.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            aaa(listUrl);

            int m = int.Parse(Console.ReadLine());
            Console.WriteLine(m);



        }
        private static async Task aaa(List<string> listUrl)
        {
            var a = DateTime.Now.Ticks;
            int dem = 0;
            List<Task<String>> awaitingTasks = new List<Task<string>>();
            for (int i = 0; i < listUrl.Count(); ++i)
            {
                dem++;
                var task = GetUrl(listUrl[i], dem);

                awaitingTasks.Add(task);

                await Task.WhenAny(awaitingTasks);

            }
            await Task.WhenAll(awaitingTasks);
        }
        private static async Task<string> GetUrl(string url, int dem)
        {
            Console.WriteLine("Running: " + url);
            return await Task.Run(() =>
            {
                getInfor(url, dem);

                return "Done";
            });

        }
        private static void getInfor(string urlHouse, int dem)
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




//var a = DateTime.Now.Ticks;
//
//String[] urls = new string[] { "a", "b" };
//for (int i = 0; i < urls.Length; ++i)
//{
//    var task = GetUrl(urls[i]);

//    awaitingTasks.Add(task);
//    if (awaitingTasks.Count == 2)
//    {
//        await Task.WhenAny(awaitingTasks);


//    }

//}
//await Task.WhenAll(awaitingTasks);
//Console.WriteLine(DateTime.Now.Ticks - a);

//static async Task<string> GetUrl(string url)
//{
//    Console.WriteLine("Running: " + url);
//    return await Task.Run(() =>
//    {
//        long i = (long)2e8;

//        while (i-- > 0)
//        {
//            if (i % 100000000 == 0)
//            {
//                Console.WriteLine(url + " " + i);
//            }
//        }
//        return "a";
//    });

//}