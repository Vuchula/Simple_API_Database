using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static Simple_API_Database.Models.EF_Models;
using Simple_API_Database.Models;
// ADD THESE DIRECTIVES
using Simple_API_Database.DataAccess;
using Newtonsoft.Json;
using System.Net.Http;

namespace Simple_API_Database.Controllers
{
    public class HomeController : Controller
    {
        /*
            These lines are needed to use the Database context,
            define the connection to the API, and use the
            HttpClient to request data from the API
        */
        public ApplicationDbContext dbContext;
        //Base URL for the IEXTrading API. Method specific URLs are appended to this base URL.
        string BASE_URL = "https://api.iextrading.com/1.0/";
        HttpClient httpClient;

        /*
             These lines create a Constructor for the HomeController.
             Then, the Database context is defined in a variable.
             Then, an instance of the HttpClient is created.

        */

        public HomeController(ApplicationDbContext context)
        {
            dbContext = context;

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new
                System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }









        /*
            Calls the IEX reference API to get the list of symbols.
            Returns a list of the companies whose information is available. 
        */
        public List<Company> GetSymbols()
        {
            string IEXTrading_API_PATH = BASE_URL + "ref-data/symbols";
            string companyList = "";
            List<Company> companies = null;

            // connect to the IEXTrading API and retrieve information
            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // read the Json objects in the API response
            if (response.IsSuccessStatusCode)
            {
                companyList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            // now, parse the Json strings as C# objects
            if (!companyList.Equals(""))
            {
                // https://stackoverflow.com/a/46280739
                companies = JsonConvert.DeserializeObject<List<Company>>(companyList);
                companies = companies.GetRange(1814, 20);
            }

            return companies;
        }

        public IActionResult Index()
        {
            //Set ViewBag variable first
            ViewBag.dbSuccessComp = 0;
            List<Company> companies = GetSymbols();

            //Save companies in TempData, so they do not have to be retrieved again
            TempData["Companies"] = JsonConvert.SerializeObject(companies);

            return View(companies);
        }

        /*
            The Symbols action calls the GetSymbols method that returns a list of Companies.
            This list of Companies is passed to the Symbols View.
        */
        public IActionResult Symbols()
        {
            //Set ViewBag variable first
            ViewBag.dbSuccessComp = 0;
            List<Company> companies = GetSymbols();

            //Save companies in TempData, so they do not have to be retrieved again
            TempData["Companies"] = JsonConvert.SerializeObject(companies);

            return View(companies);
        }

        /*
            Save the available symbols in the database
        */
        public IActionResult PopulateSymbols()
        {
            // Retrieve the companies that were saved in the symbols method
            List<Company> companies = JsonConvert.DeserializeObject<List<Company>>(TempData["Companies"].ToString());

            foreach (Company company in companies)
            {
                //Database will give PK constraint violation error when trying to insert record with existing PK.
                //So add company only if it doesnt exist, check existence using symbol (PK)
                if (dbContext.Companies.Where(c => c.symbol.Equals(company.symbol)).Count() == 0)
                {
                    dbContext.Companies.Add(company);
                }
            }

            dbContext.SaveChanges();
            ViewBag.dbSuccessComp = 1;
            return View("Index", companies);
        }







        //--------------------------------------------------------------------------------
        // Links to other views and failsafe
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult About()
        {
            return View();
        }






        //---------------------------------------------------------------------------------------
        // News API Endpoint
        public List<News> Getdatetime()
        {
            string IEXTrading_API_PATH = BASE_URL + "stock/market/news/last/5";
            string newsList = "";
            List<News> news_all = null;

            // connect to the IEXTrading API and retrieve information
            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // read the Json objects in the API response
            if (response.IsSuccessStatusCode)
            {
                newsList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            // now, parse the Json strings as C# objects
            if (!newsList.Equals(""))
            {
                // https://stackoverflow.com/a/46280739
                news_all = JsonConvert.DeserializeObject<List<News>>(newsList);
                news_all = news_all.GetRange(0, 5);
            }
            return news_all;
        }

        public IActionResult News()
        {
            //Set ViewBag variable first
            ViewBag.dbSuccessNews = 0;
            List<News> news_all = Getdatetime();

            //Save news_all in TempData, so they do not have to be retrieved again
            TempData["News_all"] = JsonConvert.SerializeObject(news_all);
            return View(news_all);
        }

        /*
            The Datetime action calls the Getdatetime method that returns a list of News_all.
            This list of News_all is passed to the Datetime View.
        */

        public IActionResult Datetime()
        {
            //Set ViewBag variable first
            ViewBag.dbSuccessComp = 0;
            List<News> news_all = Getdatetime();

            //Save news_all in TempData, so they do not have to be retrieved again
            TempData["News_all"] = JsonConvert.SerializeObject(news_all);
            return View(news_all);
        }

        /*
            Save the available symbols in the database
        */

        public IActionResult PopulateDatetime()
        {
            // Retrieve the news_all that were saved in the symbols method
            List<News> news_all = JsonConvert.DeserializeObject<List<News>>(TempData["News_all"].ToString());
            foreach (News news in news_all)
            {
                //Database will give PK constraint violation error when trying to insert record with existing PK.
                //So add news only if it doesnt exist, check existence using symbol (PK)
                if (dbContext.News_all.Where(c => c.datetime.Equals(news.datetime)).Count() == 0)
                {
                    dbContext.News_all.Add(news);
                }
            }

            dbContext.SaveChanges();
            ViewBag.dbSuccessNews = 1;
            return View("News", news_all);
        }









        //---------------------------------------------------------------------------------------

        // Quote API Endpoint

        public List<Quotes> GetQuoteSymbol()

        {

            string IEXTrading_API_PATH = BASE_URL + "/stock/market/previous";

            string quoteList = "";

            List<Quotes> Quotes = null;



            // connect to the IEXTrading API and retrieve information

            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);

            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();



            // read the Json objects in the API response

            if (response.IsSuccessStatusCode)

            {

                quoteList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            }



            // now, parse the Json strings as C# objects

            if (!quoteList.Equals(""))

            {

                // https://stackoverflow.com/a/46280739

                Quotes = JsonConvert.DeserializeObject<List<Quotes>>(quoteList);

                Quotes = Quotes.GetRange(1814, 20);

            }

            return Quotes;

        }



        public IActionResult Quote()

        {

            //Set ViewBag variable first

            ViewBag.dbSuccessQuote = 0;

            List<Quotes> Quotes = GetQuoteSymbol();



            //Save Quotes in TempData, so they do not have to be retrieved again

            TempData["Quotes"] = JsonConvert.SerializeObject(Quotes);

            return View(Quotes);

        }



        /*

            The Datetime action calls the GetSymbols method that returns a list of Quotes.

            This list of Quotes is passed to the Datetime View.

        */



        public IActionResult QuoteSymbols()

        {

            //Set ViewBag variable first

            ViewBag.dbSuccessQuote = 0;

            List<Quotes> Quotes = GetQuoteSymbol();



            //Save Quotes in TempData, so they do not have to be retrieved again

            TempData["Quotes"] = JsonConvert.SerializeObject(Quotes);

            return View(Quotes);

        }



        /*

            Save the available symbols in the database

        */



        public IActionResult PopulateQuotes()

        {

            // Retrieve the Quotes that were saved in the symbols method

            List<Quotes> Quotes = JsonConvert.DeserializeObject<List<Quotes>>(TempData["Quotes"].ToString());

            foreach (Quotes quote in Quotes)

            {

                //Database will give PK constraint violation error when trying to insert record with existing PK.

                //So add quote only if it doesnt exist, check existence using symbol (PK)

                if (dbContext.Quote.Where(c => c.symbol.Equals(quote.symbol)).Count() == 0)

                {

                    dbContext.Quote.Add(quote);

                }

            }



            dbContext.SaveChanges();

            ViewBag.dbSuccessQuote = 1;

            return View("Quotes", Quotes);

        }


    }
}