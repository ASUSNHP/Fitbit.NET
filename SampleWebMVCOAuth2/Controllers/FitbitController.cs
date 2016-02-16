﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Fitbit.Api;
using System.Configuration;
using Fitbit.Models;
using Fitbit.Api.Portable;
using System.Threading.Tasks;
using Fitbit.Api.Portable.OAuth2;

namespace SampleWebMVC.Controllers
{
    public class FitbitController : Controller
    {
        //
        // GET: /Fitbit/

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /FitbitAuth/
        // Setup - prepare the user redirect to Fitbit.com to prompt them to authorize this app.
        public ActionResult Authorize()
        {
            var appCredentials = new FitbitAppCredentials() {
                    ClientId = ConfigurationManager.AppSettings["FitbitClientId"],
                    ClientSecret = ConfigurationManager.AppSettings["FitbitClientSecret"]
            };
            //make sure you've set these up in Web.Config under <appSettings>:

            Session["AppCredentials"] = appCredentials;

            Fitbit.Api.Portable.Authenticator2 authenticator = new Fitbit.Api.Portable.Authenticator2(appCredentials.ClientId,
                                                                                    appCredentials.ClientSecret,
                                                                                    Request.Url.GetLeftPart(UriPartial.Authority) + "/Fitbit/Callback"
                                                                                    );
            string[] scopes = new string[] {"profile"};
            
            string authUrl = authenticator.GenerateAuthUrl(scopes, null);

            return Redirect(authUrl);
        }

        //Final step. Take this authorization information and use it in the app
        public async Task<ActionResult> Callback()
        {
            FitbitAppCredentials appCredentials = (FitbitAppCredentials)Session["AppCredentials"];

            Fitbit.Api.Portable.Authenticator2 authenticator = new Fitbit.Api.Portable.Authenticator2(appCredentials.ClientId,
                                                                                    appCredentials.ClientSecret,
                                                                                    Request.Url.GetLeftPart(UriPartial.Authority) + "/Fitbit/Callback"
                                                                                    );

            string code = Request.Params["code"];

            OAuth2AccessToken accessToken = await authenticator.ExchangeAuthCodeForAccessTokenAsync(code);

            // For demo, put this in the session managed by ASP.NET

            Session["AccessToken"] = accessToken;
            ViewBag.AccessToken = accessToken;

            return View();

        }

        public async Task<ActionResult> RefreshToken()
        {
            var fitbitClient = GetFitbitClient();

            ViewBag.AccessToken = await fitbitClient.RefreshOAuth2Token();

            return View("Callback");
        }

        /*
        public string TestTimeSeries()
        {
            FitbitClient client = GetFitbitClient();

            var results = client.GetTimeSeries(TimeSeriesResourceType.DistanceTracker, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

            string sOutput = "";
            foreach (var result in results.DataList)
            {
                sOutput += result.DateTime.ToString() + " - " + result.Value.ToString();
            }

            return sOutput;

        }
        
        public ActionResult LastWeekDistance()
        {
            FitbitClient client = GetFitbitClient();

            TimeSeriesDataList results = client.GetTimeSeries(TimeSeriesResourceType.Distance, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

            return View(results);
        }
        */

        public async Task<ActionResult> LastWeekSteps()
        {

            FitbitClient client = GetFitbitClient();

            var response = await client.GetTimeSeriesIntAsync(TimeSeriesResourceType.Steps, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

            return View(response);

        }
        /*
        //example using the direct API call getting all the individual logs
        public ActionResult MonthFat(string id)
        {
            DateTime dateStart = Convert.ToDateTime(id);

            FitbitClient client = GetFitbitClient();

            Fat fat = client.GetFat(dateStart, DateRangePeriod.OneMonth);

            if (fat == null || fat.FatLogs == null) //succeeded but no records
            {
                fat = new Fat();
                fat.FatLogs = new List<FatLog>();
            }
            return View(fat);

        }

        //example using the time series, one per day
        public ActionResult LastYearFat()
        {
            FitbitClient client = GetFitbitClient();

            TimeSeriesDataList fatSeries = client.GetTimeSeries(TimeSeriesResourceType.Fat, DateTime.UtcNow, DateRangePeriod.OneYear);

            return View(fatSeries);

        }

        //example using the direct API call getting all the individual logs
        public ActionResult MonthWeight(string id)
        {
            DateTime dateStart = Convert.ToDateTime(id);

            FitbitClient client = GetFitbitClient();

            Weight weight = client.GetWeight(dateStart, DateRangePeriod.OneMonth);

            if (weight == null || weight.Weights == null) //succeeded but no records
            {
                weight = new Weight();
                weight.Weights = new List<WeightLog>();
            }
            return View(weight);

        }

        //example using the time series, one per day
        public ActionResult LastYearWeight()
        {
            FitbitClient client = GetFitbitClient();

            TimeSeriesDataList weightSeries = client.GetTimeSeries(TimeSeriesResourceType.Weight, DateTime.UtcNow, DateRangePeriod.OneYear);

            return View(weightSeries);

        }

        /// <summary>
        /// This requires the Fitbit staff approval of your app before it can be called
        /// </summary>
        /// <returns></returns>
        public string TestIntraDay()
        {
            FitbitClient client = new FitbitClient(ConfigurationManager.AppSettings["FitbitConsumerKey"],
                ConfigurationManager.AppSettings["FitbitConsumerSecret"],
                Session["FitbitAuthToken"].ToString(),
                Session["FitbitAuthTokenSecret"].ToString());

            IntradayData data = client.GetIntraDayTimeSeries(IntradayResourceType.Steps, new DateTime(2012, 5, 28, 11, 0, 0), new TimeSpan(1, 0, 0));

            string result = "";

            foreach (IntradayDataValues intraData in data.DataSet)
            {
                result += intraData.Time.ToShortTimeString() + " - " + intraData.Value + Environment.NewLine;
            }

            return result;

        }

         */

        /// <summary>
        /// HttpClient and hence FitbitClient are designed to be long-lived for the duration of the session. This method ensures only one client is created for the duration of the session.
        /// More info at: http://stackoverflow.com/questions/22560971/what-is-the-overhead-of-creating-a-new-httpclient-per-call-in-a-webapi-client
        /// </summary>
        /// <returns></returns>
        private FitbitClient GetFitbitClient()
        {
            if (Session["FitbitClient"] == null)
            {
                var oa2Token = (OAuth2AccessToken)Session["AccessToken"];
                var appCredentials = (FitbitAppCredentials)Session["AppCredentials"];

                FitbitClient client = new FitbitClient(appCredentials, oa2Token);

                Session["FitbitClient"] = client;
                return client;
            }
            else
            {
                return (FitbitClient)Session["FitbitClient"];
            }
        }
    }
}