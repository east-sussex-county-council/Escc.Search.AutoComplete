using Escc.GoogleAnalytics.Admin;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Escc.Search.AutoComplete.Admin.GoogleAnalytics
{
    class GoogleAnalyticsKeywordSourceApi4WithServiceAccount : IKeywordSource
    {
        public List<KeywordResult> ReadKeywords()
        {
            var response = new GetReportsResponse();
            var serviceAccountCredentialFilePath = Path.GetFullPath("Secret.json");

            try
            {
                if (string.IsNullOrEmpty(serviceAccountCredentialFilePath))
                    throw new Exception("Path to the service account credentials file is required.");
                if (!File.Exists(serviceAccountCredentialFilePath))
                    throw new Exception("The service account credentials file does not exist at: " + serviceAccountCredentialFilePath);
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["GoogleServiceAccount"]))
                    throw new Exception("ServiceAccountEmail is required.");

                // These are the scopes of permissions you need. It is best to request only what you need and not all of them
                string[] scopes = new string[] { AnalyticsReportingService.Scope.Analytics };             // View your Google Analytics data

                // For Json file
                if (Path.GetExtension(serviceAccountCredentialFilePath).ToLower() == ".json")
                {
                    GoogleCredential credential;
                    using (var stream = new FileStream(serviceAccountCredentialFilePath, FileMode.Open, FileAccess.Read))
                    {
                        credential = GoogleCredential.FromStream(stream)
                             .CreateScoped(scopes);
                    }
                    // Create the  Analytics service.
                    using (var svc = new AnalyticsReportingService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "AnalyticsReporting Service account Authentication"
                    }))
                    {
                        var dateRange = new DateRange
                        {
                            StartDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd"),
                            EndDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        };
                        var pageviews = new Metric
                        {
                            Expression = "ga:pageviews",
                            Alias = "Pageviews"
                        };
                        var searchKeyword = new Dimension { Name = "ga:searchKeyword" };
                        var orderBy = new OrderBy { FieldName = "ga:pageviews" };

                        var reportRequest = new ReportRequest
                        {
                            DateRanges = new List<DateRange> { dateRange },
                            Dimensions = new List<Dimension> { searchKeyword },
                            Metrics = new List<Metric> { pageviews },
                            PageSize = 10000,
                            OrderBys = new List<OrderBy> { orderBy },
                            ViewId = ConfigurationManager.AppSettings["GoogleAnalyticsProfileId"]
                        };
                        var getReportsRequest = new GetReportsRequest
                        {
                            ReportRequests = new List<ReportRequest> { reportRequest }
                        };
                        var batchRequest = svc.Reports.BatchGet(getReportsRequest);
                        response = batchRequest.Execute();
                    }
                }
                else
                {
                    throw new Exception("Unsupported Service account credentials.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Create service account AnalyticsReportingService failed " + ex.Message);
                throw new Exception("CreateServiceAccountAnalyticsReportingFailed ", ex);
            }
        return GoogleDataFeedToRemoveDuplicatesOrBadWords(response);
        }

        static async Task<UserCredential> GetCredential()
        {
            using (var stream = new FileStream("client_secret.json",
                 FileMode.Open, FileAccess.Read))
            {
                string loginEmailAddress = ConfigurationManager.AppSettings["GoogleUsername"];
                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { AnalyticsReportingService.Scope.Analytics },
                    loginEmailAddress, CancellationToken.None,
                    new FileDataStore("GoogleAnalyticsApiConsole"));
            }
        }

        private static List<KeywordResult> GoogleDataFeedToRemoveDuplicatesOrBadWords(GetReportsResponse dataFeed)
        {
            // Rules to perform on transformation
            // 1. Lowercase all keywords
            // 2. Remove duplicates
            // 3. Add the views of the duplicates to the existing keyword so as to keep its true position
            // 4. Remove keywords that are on the blacklist e.g. urls
            // 5. Order by page views in descending order i.e. most popular first
            List<KeywordResult> keywords = new List<KeywordResult>();

            foreach (var item in dataFeed.Reports.First().Data.Rows)
            {
                int matchIndex = -1;
                for (int i = 0; i < keywords.Count; i++)
                {
                    if (keywords[i].Keyword == item.Dimensions.First().ToLower())
                    {
                        matchIndex = i;
                    }
                }
                if (matchIndex > -1)
                {
                    // Match keyword and get page views
                    int matchedKeywordPageViewCount = keywords[matchIndex].PageViews;
                    // Increment page views to include duplicate page views
                    matchedKeywordPageViewCount += Convert.ToInt32(item.Metrics.First().Values.First());
                    // Update page views for keyword
                    keywords[matchIndex].PageViews = matchedKeywordPageViewCount;
                }
                else
                {
                    // Still need to apply rule 4
                    string checkedKeyword = RemoveBlacklistedKeywords(item.Dimensions.First().ToLower());
                    if (checkedKeyword.Length > 0)
                    {
                        keywords.Add(new KeywordResult() { Keyword = checkedKeyword, PageViews = Convert.ToInt32(item.Metrics.First().Values.First()), FeedDate = DateTime.Today });
                    }
                }
            }
            // Rule 5 reorder based on pageviews in descending order
            return keywords.OrderByDescending(x => x.PageViews).ToList();
        }

        private static string RemoveBlacklistedKeywords(string keyword)
        {
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