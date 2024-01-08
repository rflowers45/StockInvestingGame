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
        public int dayNum = 0;
        public string balanceResult = " ";



        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        
        //Retrieves stock data in a clump and returns the data
        public IActionResult OnPostGetStocks(string value)
        {
            try
            {
                //*********API CALL*************
                var symbol = value; //Setting the ticker symbol to what the user has entered
                var apiKey = "";
                var dailyPrices = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol={symbol}&outputsize=full&apikey={apiKey}&datatype=csv"
                    .GetStringFromUrl().FromCsv<List<StockData>>();
                //*********API CALL*************

                //This fills the object with values
                dailyPrices.PrintDump();

                int lastIndex = dailyPrices.Count() - 1; //This is used to get the total amount of indices in the list. For random generation
                var testDate = GetRandomDate(lastIndex); //Getting a random indices for closing stock price
                string dateString = dailyPrices[testDate].Timestamp.ToString();
                
                //This grabs the objects day
                var dayPrice = dailyPrices[testDate].Close;//This gets the price
                price = dayPrice; //Setting the global price to = the day price
                string displayResults = ("Getting data for " + value + "<br> Date: " + dateString + "<br> Closing price: $" + dayPrice);
                //SESSION VARIABLES
                HttpContext.Session.SetString("balance", "10000");
                HttpContext.Session.SetString("price", price.ToString());
                HttpContext.Session.SetInt32("shares", 0);
                HttpContext.Session.SetInt32("currentDay", testDate);
                HttpContext.Session.SetInt32("dayCounter", 1);
                HttpContext.Session.SetString("ticker", value);

                return new JsonResult(displayResults);

            }
            catch (Exception)
            {
                return new JsonResult("Ticker symbol not found or random date is not a trading day. Try again.");

            }

        }

        //This will handle the logic for purchasing a stock. The "value" parameter represents the amount of shares passed in
        public IActionResult OnPostBuyStocks(int value)
        {
            try
            {
                //*********API CALL*************
                var symbol = HttpContext.Session.GetString("ticker"); //Setting the ticker symbol to what the user has entered
                var apiKey = "YQ12ME2NUXQ29XG8"; //I got this key by registering my email. You all might wanna do the same or use mine?
                var dailyPrices = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol={symbol}&outputsize=full&apikey={apiKey}&datatype=csv"
                    .GetStringFromUrl().FromCsv<List<StockData>>();
                //*********API CALL*************

                var vDateIndex = HttpContext.Session.GetInt32("currentDay"); //getting date index
                int iDateIndex = vDateIndex.Value;
                string dateString = dailyPrices[iDateIndex].Timestamp.ToString();
                var dayPrice = dailyPrices[iDateIndex].Close;//This gets the price
                price = dayPrice;
                HttpContext.Session.SetString("price", price.ToString());
                string sSessionPrice = HttpContext.Session.GetString("price"); //Getting session price
                string sSessionBalance = HttpContext.Session.GetString("balance"); //Getting session balance
               
                var vDayCounter = HttpContext.Session.GetInt32("dayCounter");
                int iDayCounter = vDayCounter.Value;
                iDayCounter++;
                balance = Convert.ToDecimal(sSessionBalance);
                price = Convert.ToDecimal(sSessionPrice);
                string endGame = endGameScenario();
                while (iDayCounter < 8)
                {
                    decimal totalBuyingPrice = price * value;
                    decimal total = balance - totalBuyingPrice;
                

                    if (total < 0)
                    {
                        return new JsonResult("You do not have any money to purchase that amount!");
                    }
                    else
                    {
                        var iSessionShares = HttpContext.Session.GetInt32("shares"); //Getting sessions shares
                        int shares = iSessionShares.Value; //Have to convert nullable int to int
                        int iAddedShares = shares + value;

                        iDateIndex++;
                        HttpContext.Session.SetInt32("currentDay", iDateIndex);
                        HttpContext.Session.SetInt32("dayCounter", iDayCounter);
                        HttpContext.Session.SetInt32("shares", iAddedShares); //Setting session shares held
                        HttpContext.Session.SetString("balance", total.ToString()); //Setting session balance
                        return new JsonResult(value + " share(s) purchased. <br> Date: " + dateString +  "<br> Current Balance: $" + total + "<br> Shares Held: " + iAddedShares + "<br> Current Day: " + iDayCounter + "<br> New Price:  $" + price);

                    }
                    
                }
                return new JsonResult(endGame);
            }
            catch (Exception)
            {
                return new JsonResult("There was a problem purchasing that amount!");

            }
        }
        //This will handle the logic for selling stocks
        public IActionResult OnPostSellStocks(int value)
        {
            try
            {
                //*********API CALL*************
                var symbol = HttpContext.Session.GetString("ticker"); //Setting the ticker symbol to what the user has entered
                var apiKey = "YQ12ME2NUXQ29XG8"; //I got this key by registering my email. You all might wanna do the same or use mine?
                var dailyPrices = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol={symbol}&outputsize=full&apikey={apiKey}&datatype=csv"
                    .GetStringFromUrl().FromCsv<List<StockData>>();
                //*********API CALL*************

                var vDateIndex = HttpContext.Session.GetInt32("currentDay"); //getting date index
                int iDateIndex = vDateIndex.Value;
                string dateString = dailyPrices[iDateIndex].Timestamp.ToString();
                var dayPrice = dailyPrices[iDateIndex].Close;//This gets the price
                price = dayPrice;
                HttpContext.Session.SetString("price", price.ToString());
                string sSessionPrice = HttpContext.Session.GetString("price"); //Getting session price
                string sSessionBalance = HttpContext.Session.GetString("balance"); //Getting session balance
                var vDayCounter = HttpContext.Session.GetInt32("dayCounter");
                int iDayCounter = vDayCounter.Value;
                iDayCounter++;
                var iSessionShares = HttpContext.Session.GetInt32("shares"); //Getting sessions shares
                int ownedShares = iSessionShares.Value; //Have to convert nullable-int to int
                balance = Convert.ToDecimal(sSessionBalance);
                price = Convert.ToDecimal(sSessionPrice);
                string endGame = endGameScenario();
                while (iDayCounter < 8)
                {
                    decimal totalSellingPrice = price * value;
                    decimal total = balance + totalSellingPrice;


                    //Checking if the amount of shares being sold are greater than the amount of shares owned
                    if (value > ownedShares)
                    {
                        return new JsonResult("You do not have enough shares to sell!");
                    }
                    else
                    {
                        int iSubtractedShares = ownedShares - value;


                        iDateIndex++;
                        HttpContext.Session.SetInt32("currentDay", iDateIndex);
                        HttpContext.Session.SetInt32("dayCounter", iDayCounter);
                        HttpContext.Session.SetInt32("shares", iSubtractedShares); //Setting session shares held
                        HttpContext.Session.SetString("balance", total.ToString()); //Setting session balance
                        return new JsonResult(value + " share(s) sold. <br> Date: " + dateString +  "<br> Current Balance: $" + total + "<br> Shares Held: " + iSubtractedShares + "<br> Current Day: " + iDayCounter + "<br> New Price:  $" + price);

                    }
                }

                return new JsonResult(endGame);//endGameScenario());
            }
            catch (Exception)
            {
                return new JsonResult("There was a problem selling these stocks!");

            }
        }

        //This will handle the logic for the user selecting "Hold"
        public IActionResult OnPostHold()
        {
            try
            {
                //*********API CALL*************
                var symbol = HttpContext.Session.GetString("ticker"); //Setting the ticker symbol to what the user has entered
                var apiKey = "YQ12ME2NUXQ29XG8"; //I got this key by registering my email. You all might wanna do the same or use mine?
                var dailyPrices = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol={symbol}&outputsize=full&apikey={apiKey}&datatype=csv"
                    .GetStringFromUrl().FromCsv<List<StockData>>();
                //*********API CALL*************

                var vDateIndex = HttpContext.Session.GetInt32("currentDay"); //getting date index
                int iDateIndex = vDateIndex.Value;
                string dateString = dailyPrices[iDateIndex].Timestamp.ToString();
                var dayPrice = dailyPrices[iDateIndex].Close;//This gets the price
                price = dayPrice;
                HttpContext.Session.SetString("price", price.ToString());
                string sSessionPrice = HttpContext.Session.GetString("price"); //Getting session price
                string sSessionBalance = HttpContext.Session.GetString("balance"); //Getting session balance
               
                var vDayCounter = HttpContext.Session.GetInt32("dayCounter");
                int iDayCounter = vDayCounter.Value;
                iDayCounter++;
                var iSessionShares = HttpContext.Session.GetInt32("shares"); //Getting sessions shares
                int ownedShares = iSessionShares.Value; //Have to convert nullable-int to int
                balance = Convert.ToDecimal(sSessionBalance);
                price = Convert.ToDecimal(sSessionPrice);
                string endGame = endGameScenario();
                while (iDayCounter < 8)
                {
                    iDateIndex++;
                    HttpContext.Session.SetInt32("currentDay", iDateIndex);
                    HttpContext.Session.SetInt32("dayCounter", iDayCounter);
                    HttpContext.Session.SetInt32("shares", ownedShares); //Setting session shares held
                    HttpContext.Session.SetString("balance", balance.ToString()); //Setting session balance

                    return new JsonResult("Date: " + dateString + "<br> Current Balance: $" + balance + "<br> Shares Held: " + ownedShares + "<br> Current Day: " + iDayCounter + "<br> New Price:  $" + price);
                }
                return new JsonResult(endGame);


            }
            catch (Exception)
            {
                return new JsonResult(endGameScenario());

            }

        }

        //This will handle the logic for the user selecting "Quit"
        public IActionResult OnPostQuit()
        {
            try
            {
                string sSessionPrice = HttpContext.Session.GetString("price"); //Getting session price
                string sSessionBalance = HttpContext.Session.GetString("balance"); //Getting session balance
               
                var iSessionShares = HttpContext.Session.GetInt32("shares"); //Getting sessions shares
                int ownedShares = iSessionShares.Value; //Have to convert nullable-int to int
                balance = Convert.ToDecimal(sSessionBalance);
                price = Convert.ToDecimal(sSessionPrice);
                decimal totalSellingPrice = price * ownedShares;
                decimal total = balance + totalSellingPrice;
                //iDateIndex++;
                //HttpContext.Session.SetInt32("currentDay", iDateIndex);
                HttpContext.Session.SetInt32("shares", ownedShares); //Setting session shares held
                HttpContext.Session.SetString("balance", total.ToString()); //Setting session balance
                string endGame = endGameScenario();
                return new JsonResult("");
             
            }
            catch (Exception)
            {
                return new JsonResult("");

            }

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

        //Stores prices and balance
        public class AccountData
        {
            public decimal price { get; set; }
            public decimal balance { get; set; }
        }

        //********************HELPER FUNCTIONS********************

        //This function generates a random date for the stock API
        private int GetRandomDate(int indicesCount)
        {
            var random = new Random();
            var range = indicesCount - 60;
            var newDate = random.Next(range);

            return newDate;
        }

        //This function shows gains and losses and displays results when game is finished
       public string endGameScenario()
        {
            string sSessionPrice = HttpContext.Session.GetString("price"); //Getting session price
            string sSessionBalance = HttpContext.Session.GetString("balance"); //Getting session balance
            var iSessionShares = HttpContext.Session.GetInt32("shares"); //Getting sessions shares
            int ownedShares = iSessionShares.Value; //Have to convert nullable-int to int
            balance = Convert.ToDecimal(sSessionBalance);
            price = Convert.ToDecimal(sSessionPrice);
            decimal totalSellingPrice = price * ownedShares;
            decimal total = balance + totalSellingPrice;
            //string balanceResult = " ";
            decimal difference = total - 10000;
            if(difference > 0)
            {
                balanceResult = " You Gained: $";
            } else
            {
                balanceResult = " You lost: $";
            }

            HttpContext.Session.SetInt32("shares", ownedShares); //Setting session shares held
            HttpContext.Session.SetString("balance", total.ToString()); //Setting session balance

            //return new JsonResult(endGame + " Final Balance: $" + total + "");
            return "Thank you for playing! <br>" + "Final Balance: $" + total + "<br>" + balanceResult + Math.Abs(difference);
        }
       
    }
}
