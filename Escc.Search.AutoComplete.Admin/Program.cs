using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Configuration;
using Escc.Net;
using Exceptionless;
using Escc.GoogleAnalytics.Admin.SqlServer;
using Escc.Search.AutoComplete.Admin.SqlServer;
using Escc.Search.AutoComplete.Admin.AzureTableStorage;

namespace Escc.GoogleAnalytics.Admin
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Exceptionless.ExceptionlessClient.Current.Startup();

                Console.WriteLine("Keyword importing started ...");

                var source = new SqlServerKeywordSource();
                var keywords = source.ReadKeywords();

                Console.WriteLine("Keywords cleaned and ready to import: " + keywords.Count.ToString());

                var repo = new AzureTableStorageKeywordRepository();
                repo.SaveKeywords(keywords);

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
