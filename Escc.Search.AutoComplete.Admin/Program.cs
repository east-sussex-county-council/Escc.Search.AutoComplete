using Escc.Search.AutoComplete.Admin;
using Escc.Search.AutoComplete.Admin.AzureTableStorage;
using Escc.Search.AutoComplete.Admin.GoogleAnalytics;
using Exceptionless;
using System;

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
                Console.WriteLine("Press Enter to finish.");
            }
        }
    }
}
