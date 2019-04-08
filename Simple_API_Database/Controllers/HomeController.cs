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

        public List<Sector> Gettype()
        {
            string IEXTrading_API_PATH = BASE_URL + "/stock/market/sector-performance";
            string sectorList = "";
            List<Sector> sector_all = null;

            // connect to the IEXTrading API and retrieve information
            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
            
            // read the Json objects in the API response
            if (response.IsSuccessStatusCode)
            {
                sectorList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            // now, parse the Json strings as C# objects
            if (!sectorList.Equals(""))
            {
                // https://stackoverflow.com/a/46280739
                sector_all = JsonConvert.DeserializeObject<List<Sector>>(sectorList);
                sector_all = sector_all.GetRange(0, 5);
            }
            return sector_all;
        }

        public IActionResult Sector()
        {
            //Set ViewBag variable first
            ViewBag.dbSuccessSector = 0;
            List<Sector> sector_all = Gettype();

            //Save sector_all in TempData, so they do not have to be retrieved again
            TempData["Sector_all"] = JsonConvert.SerializeObject(sector_all);
            return View(sector_all);
        }

        /*
            The Type action calls the Gettype method that returns a list of Sector_all.
            This list of Sector_all is passed to the Type View.
        */

        public IActionResult Type()
        {
            //Set ViewBag variable first
            ViewBag.dbSuccessSec = 0;
            List<Sector> sector_all = Gettype();

            //Save sector_all in TempData, so they do not have to be retrieved again
            TempData["Sector_all"] = JsonConvert.SerializeObject(sector_all);
            return View(sector_all);
        }

        /*
            Save the available symbols in the database
        */

        public IActionResult PopulateType()
        {
            // Retrieve the sector_all that were saved in the symbols method
            List<Sector> sector_all = JsonConvert.DeserializeObject<List<Sector>>(TempData["Sector_all"].ToString());
            foreach (Sector sector in sector_all)
            {
                //Database will give PK constraint violation error when trying to insert record with existing PK.
                //So add sector only if it doesnt exist, check existence using symbol (PK)
                if (dbContext.Sector_all.Where(c => c.type.Equals(sector.type)).Count() == 0)
                { 
                    dbContext.Sector_all.Add(sector);
                }
            }
            dbContext.SaveChanges();
            ViewBag.dbSuccessSector = 1;
            return View("Sector", sector_all);
        }





        //-------------------------------------------------------------------------------------------
        // Losing Stocks API Endpoint

        public List<Loser> GetLosingSymb()

        {

            string IEXTrading_API_PATH = BASE_URL + "/stock/market/list/losers";

            string loserList = "";

            List<Loser> loser_all = null;



            // connect to the IEXTrading API and retrieve information

            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);

            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();



            // read the Json objects in the API response

            if (response.IsSuccessStatusCode)

            {

                loserList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            }



            // now, parse the Json strings as C# objects

            if (!loserList.Equals(""))

            {

                // https://stackoverflow.com/a/46280739

                loser_all = JsonConvert.DeserializeObject<List<Loser>>(loserList);

                loser_all = loser_all.GetRange(0, 5);

            }

            return loser_all;

        }



        public IActionResult loser()

        {

            //Set ViewBag variable first

            ViewBag.dbSuccessloser = 0;

            List<Loser> loser_all = GetLosingSymb();



            //Save loser_all in TempData, so they do not have to be retrieved again

            TempData["loser_all"] = JsonConvert.SerializeObject(loser_all);

            return View(loser_all);

        }



        /*

            The Type action calls the Gettype method that returns a list of loser_all.

            This list of loser_all is passed to the Type View.

        */



        public IActionResult LosingSymb()

        {

            //Set ViewBag variable first

            ViewBag.dbSuccessloser = 0;

            List<Loser> loser_all = GetLosingSymb();



            //Save loser_all in TempData, so they do not have to be retrieved again

            TempData["loser_all"] = JsonConvert.SerializeObject(loser_all);

            return View(loser_all);

        }



        /*

            Save the available symbols in the database

        */



        public IActionResult PopulateLosingSymb()

        {

            // Retrieve the loser_all that were saved in the symbols method

            List<Loser> loser_all = JsonConvert.DeserializeObject<List<Loser>>(TempData["loser_all"].ToString());

            foreach (Loser loser in loser_all)

            {

                //Database will give PK constraint violation error when trying to insert record with existing PK.

                //So add loser only if it doesnt exist, check existence using symbol (PK)

                if (dbContext.Loser_all.Where(c => c.symbol.Equals(loser.symbol)).Count() == 0)

                {

                    dbContext.Loser_all.Add(loser);

                }

            }



            dbContext.SaveChanges();

            ViewBag.dbSuccessloser = 1;

            return View("loser", loser_all);

        }




    }
}