using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Configuration;
using Escc.Net;
using Exceptionless;
using Escc.Search.AutoComplete.Admin.AzureTableStorage;
using Escc.Search.AutoComplete.Admin.SqlServer;

namespace Escc.GoogleAnalytics.Admin
{
    public class Program
    {
        static void Main(string[] args)
        {
            TransferKeywords(new SqlServerKeywordSource(), new AzureTableStorageKeywordRepository());
        }

        private static void TransferKeywords(IKeywordSource source, IKeywordRepository destination)
        {
            try
            {
                Exceptionless.ExceptionlessClient.Current.Startup();

                Console.WriteLine("Keyword importing started ...");

                var keywords = source.ReadKeywords();

                Console.WriteLine("Keywords cleaned and ready to import: " + keywords.Count.ToString());

                destination.SaveKeywords(keywords);

                Console.WriteLine("Keywords imported. Press Enter to finish.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press Enter to finish.");
                Console.ReadLine();
            }
        }
    }
}
