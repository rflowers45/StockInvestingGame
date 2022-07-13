using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using ServiceStack;
using ServiceStack.Text;
using Microsoft.AspNetCore.Http;

namespace StockInvestingGame.Pages
{
    public class IndexModel : PageModel
    {
        public decimal balance = 0; //Stores current balance as global variable
        public decimal price = 0; //Stores the current price of the day
        public int sharesHeld = 0; //Store the amount of shares currently held
        

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        //Retrieves stock data in a clump and returns the data
        //TODO:  possibly chart creation (most likely a separate function), randomizing the date
        public IActionResult OnPostGetStocks(string value)
        {
            try
            {
                var symbol = value; //Setting the ticker symbol to what the user has entered
                var apiKey = "YQ12ME2NUXQ29XG8"; //I got this key by registering my email. You all might wanna do the same or use mine?
                var dailyPrices = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={apiKey}&datatype=csv"
                    .GetStringFromUrl().FromCsv<List<StockData>>();

                //This fills the object with values
                dailyPrices.PrintDump();

                var testDate = GetRandomDate();
                var day = dailyPrices.Where(u => u.Timestamp.Year == testDate.Year && u.Timestamp.Month == testDate.Month && u.Timestamp.Day == testDate.Day); //This grabs the objects day
                var dayPrice = day.Max(u => u.Close);//This gets the price
                price = dayPrice; //Setting the global price to = the day price
                string displayResults = ("Getting data for " + value + "<br> Date: " + testDate + "<br> Closing price: $" + dayPrice);
                HttpContext.Session.SetString("balance", "10000");
                HttpContext.Session.SetString("price", price.ToString());
                HttpContext.Session.SetInt32("shares", 0);

                return new JsonResult(displayResults);

            }
            catch (Exception)
            {
                return new JsonResult("Ticker symbol not found. Check your spelling?");

            }
            
        }
        public IActionResult OnPostGetStocksSlider(int date, string value)
        {
            try
            {
                var symbol = value; //Setting the ticker symbol to what the user has entered
                var apiKey = "YQ12ME2NUXQ29XG8"; //I got this key by registering my email. You all might wanna do the same or use mine?
                var dailyPrices = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={apiKey}&datatype=csv"
                    .GetStringFromUrl().FromCsv<List<StockData>>();

                //This fills the object with values
                dailyPrices.PrintDump();

                DateTime testDate = DateTime.Today.AddDays(-date); //This is a random date I chose for testing
                var day = dailyPrices.Where(u => u.Timestamp.Year == testDate.Year && u.Timestamp.Month == testDate.Month && u.Timestamp.Day == testDate.Day); //This grabs the objects day
                var dayPrice = day.Max(u => u.Close);//This gets the price
                


                return new JsonResult("Getting data for " + symbol + "<br> Date: " + testDate + "<br> Closing price: $" + dayPrice);

            }
            catch (Exception)
            {
                return new JsonResult("Ticker symbol not found. Check your spelling?");

            }

        }

        //This will handle the logic for purchasing a stock. The "value" parameter represents the amount of shares passed in
        public IActionResult OnPostBuyStocks(int value)
        {
            try
            {
                string sSessionPrice = HttpContext.Session.GetString("price"); //Getting session price
                string sSessionBalance = HttpContext.Session.GetString("balance"); //Getting session balance
                balance = Convert.ToDecimal(sSessionBalance);
                price = Convert.ToDecimal(sSessionPrice);

                decimal totalBuyingPrice = price * value;
                decimal total = balance - totalBuyingPrice;
                
                if (total > balance)
                {
                    return new JsonResult("You do not have any money to purchase that amount!");
                }
                else
                {
                    var iSessionShares = HttpContext.Session.GetInt32("shares"); //Getting sessions shares
                    int shares = iSessionShares.Value; //Have to convert nullable int to int
                    iSessionShares = sharesHeld + value;
                    balance = total;
                    HttpContext.Session.SetInt32("shares", shares); //Setting session shares held
                    HttpContext.Session.SetString("balance", total.ToString()); //Setting session balance
                    return new JsonResult(value + "share(s) purchased. <br> Current Balance: $" + balance);

                }

                

            }
            catch (Exception)
            {
                return new JsonResult("There was a problem purchasing that amount!");

            }

        }
        //This will handle the logic for selling stocks
        public IActionResult OnPostSellStocks()
        {
            try
            {


                return new JsonResult("");

            }
            catch (Exception)
            {
                return new JsonResult("");

            }

        }

        //This will handle the logic for the user selecting "Hold"
        public IActionResult OnPostHold()
        {
            try
            {


                return new JsonResult("");

            }
            catch (Exception)
            {
                return new JsonResult("");

            }

        }

        //Not in use currently. Possibly used later on?
        public void OnGet()
        {

        }

        //********************CLASSES********************

        //Stores stock data
        public class StockData
        {
            public DateTime Timestamp { get; set; }
            public decimal Open { get; set; }

            public decimal High { get; set; }
            public decimal Low { get; set; }

            public decimal Close { get; set; }
            public decimal Volume { get; set; }
        }

        //********************HELPER FUNCTIONS********************

        //We can use this funciton to help with randomizing the date. Need to code the logic
        private DateTime GetRandomDate()
        {
            Random rnd = new Random();
            DateTime datetoday = DateTime.Now;

            int rndYear = 2022;//rnd.Next(2000, datetoday.Year);
            int rndMonth = rnd.Next(4, 6);//rnd.Next(1,12);
            int rndDay = rnd.Next(1, 31);//rnd,Next(1,31);

            DateTime generateDate = new DateTime(rndYear, rndMonth, rndDay);
            return generateDate;
        }
    }
}