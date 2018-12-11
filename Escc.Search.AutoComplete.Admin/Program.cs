using Escc.Search.AutoComplete.Admin;
using Escc.Search.AutoComplete.Admin.AzureTableStorage;
using Escc.Search.AutoComplete.Admin.GoogleAnalytics;
using Exceptionless;
using System;
using System.Configuration;

namespace Escc.GoogleAnalytics.Admin
{
    public class Program
    {
        static Proxy proxy = new Proxy();
        static void Main(string[] args)
        {
            TransferKeywords(new GoogleAnalyticsKeywordSourceApi4WithServiceAccount(), new AzureTableStorageKeywordRepository());
        }

        private static void TransferKeywords(IKeywordSource source, IKeywordRepository destination)
        {
            try
            {
                Exceptionless.ExceptionlessClient.Current.Startup();

                if (!CheckEnvironmentPrecondition())
                {
                    return;
                }

                Console.WriteLine("Keyword importing started ...");

                var keywords = source.ReadKeywords();

                Console.WriteLine("Keywords cleaned and ready to import: " + keywords.Count.ToString());

                if (keywords.Count > 0)
                {
                    destination.SaveKeywords(keywords);
                }

                Console.WriteLine("Keywords imported.");
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static bool CheckEnvironmentPrecondition()
        {
            var precondition = ConfigurationManager.AppSettings["Precondition"];
            if (!string.IsNullOrEmpty(precondition))
            {
                var split = ConfigurationManager.AppSettings["Precondition"].Split('=');
                if (split.Length == 2)
                {
                    var result = (Environment.GetEnvironmentVariable(split[0]).Equals(split[1], StringComparison.OrdinalIgnoreCase));
                    Console.WriteLine("Precondition " + precondition + (result ? " OK." : " failed."));
                    return result;
                }
            }
            return true;
        }
    }
}
