using Escc.Net;
using Google.GData.Analytics;
using Google.GData.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Escc.GoogleAnalytics.Admin
{
    public class GoogleAnalyticsKeywordSource : IKeywordSource
    {
        private readonly IProxyProvider _proxyProvider;

        public GoogleAnalyticsKeywordSource(IProxyProvider proxyProvider)
        {
            _proxyProvider = proxyProvider;
        }

        public List<KeywordResult> ReadKeywords()
        {
            string profileId = ConfigurationManager.AppSettings["GoogleAnalyticsProfileId"];
            DataFeed dataFeed = UseGoogleAnalyticsApiToReturnInSearchKeywordDataFeed(profileId);
            return GoogleDataFeedToRemoveDuplicatesOrBadWords(dataFeed);
        }

        private DataFeed UseGoogleAnalyticsApiToReturnInSearchKeywordDataFeed(string profileId)
        {

            var dataQuery = new DataQuery("https://www.google.com/analytics/feeds/data")
            {
                Ids = profileId,
                Dimensions = "ga:searchKeyword",
                Metrics = "ga:pageviews",
                Sort = "-ga:pageviews",
                GAStartDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd"),
                GAEndDate = DateTime.Now.ToString("yyyy-MM-dd"),
                NumberToRetrieve = 10000
            };

            return ReturnGoogleAnalyticsDataFeed(dataQuery);


        }

        private DataFeed ReturnGoogleAnalyticsDataFeed(DataQuery dataQuery)
        {
            string userName = ConfigurationManager.AppSettings["GoogleUsername"];
            string passWord = ConfigurationManager.AppSettings["GooglePassword"];

            var service = new AnalyticsService(ConfigurationManager.AppSettings["ApplicationName"]);

            GDataRequestFactory f = new GDataRequestFactory(ConfigurationManager.AppSettings["UserAgent"]);
            if (_proxyProvider != null)
            {
                var proxy = _proxyProvider.CreateProxy();
                f.Proxy = proxy;
            }

            service.RequestFactory = f;

            service.setUserCredentials(userName, passWord);

            return service.Query(dataQuery);

        }

        private static List<KeywordResult> GoogleDataFeedToRemoveDuplicatesOrBadWords(DataFeed dataFeed)
        {
            // Rules to perform on transformation
            // 1. Lowercase all keywords
            // 2. Remove duplicates
            // 3. Add the views of the duplicates to the existing keyword so as to keep its true position
            // 4. Remove keywords that are on the blacklist e.g. urls
            // 5. Order by page views in descending order i.e. most popular first



            List<KeywordResult> keywords = new List<KeywordResult>();




            foreach (DataEntry item in dataFeed.Entries)
            {

                int matchIndex = -1;
                for (int i = 0; i < keywords.Count; i++)
                {

                    if (keywords[i].Keyword == item.Dimensions[0].Value.ToLower())
                    {
                        matchIndex = i;
                    }
                }

                if (matchIndex > -1)
                {
                    // Match keyword and get page views
                    int matchedKeywordPageViewCount = keywords[matchIndex].PageViews;

                    // Increment page views to include duplicate page views
                    matchedKeywordPageViewCount += Convert.ToInt32(item.Metrics[0].Value);

                    // Update page views for keyword
                    keywords[matchIndex].PageViews = matchedKeywordPageViewCount;
                }
                else
                {

                    // Still need to apply rule 4
                    string checkedKeyword = RemoveBlacklistedKeywords(item.Dimensions[0].Value.ToLower());

                    if (checkedKeyword.Length > 0)
                    {

                        keywords.Add(new KeywordResult() { Keyword = checkedKeyword, PageViews = Convert.ToInt32(item.Metrics[0].Value) });


                    }
                }



            }


            // Rule 5 reorder based on pageviews in descending order
            return keywords.OrderByDescending(x => x.PageViews).ToList();
        }


        private static string RemoveBlacklistedKeywords(string keyword)
        {
            //if (System.Text.RegularExpressions.Regex.Match(keyword,"")
            //{
            //    return string.Empty;
            //}


            if ((keyword.Contains("www") || keyword.Contains("http") || keyword.Contains(".gov") || keyword.Contains(".uk") || keyword.Contains(".com") || keyword.Contains("@") || keyword.Contains("google")))
            {
                return string.Empty;
            }
            else
            {
                return keyword;
            }
        }
    }
}
