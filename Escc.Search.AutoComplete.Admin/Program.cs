using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Configuration;
using Escc.Net;
using Exceptionless;

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

                var source = new GoogleAnalyticsKeywordSource(new ConfigurationProxyProvider());
                var keywords = source.ReadKeywords();

                Console.WriteLine("Keywords cleaned and ready to import" + keywords.Count.ToString());

                var repo = new SqlServerKeywordRepository();
                repo.SaveKeywords(keywords);

                Console.WriteLine("Keywords imported to database");
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
            }

        }

    }
}
