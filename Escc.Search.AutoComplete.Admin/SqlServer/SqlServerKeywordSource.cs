using Dapper;
using Escc.GoogleAnalytics.Admin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escc.Search.AutoComplete.Admin.SqlServer
{
    /// <summary>
    /// Read all keywords from a SQL server database
    /// </summary>
    public class SqlServerKeywordSource : IKeywordSource
    {
        /// <summary>
        /// Reads the keywords.
        /// </summary>
        /// <returns></returns>
        public List<KeywordResult> ReadKeywords()
        {
            var list = new List<KeywordResult>();

            using (var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["AutoSuggestReader"].ConnectionString))
            {
                var results = cn.Query("SELECT Keyword, PageViews, FeedDate FROM InSearchKeywords ORDER BY Keyword");
                foreach (var result in results)
                {
                    list.Add(new KeywordResult()
                    {
                        Keyword = result.Keyword,
                        PageViews = result.PageViews,
                        FeedDate = result.FeedDate
                    });
                }
            }

            return list;
        }
    }
}
