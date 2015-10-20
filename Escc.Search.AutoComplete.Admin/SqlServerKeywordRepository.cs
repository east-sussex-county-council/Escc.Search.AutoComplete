using Exceptionless;
using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Escc.GoogleAnalytics.Admin
{
    public class SqlServerKeywordRepository : IKeywordRepository
    {
        public void SaveKeywords(List<KeywordResult> keywords)
        {

            SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["AutoSuggestWriter"].ConnectionString);

            if (keywords.Count > 5000)
            {
                cn.Execute("usp_InSearchKeywords_Delete", commandType: CommandType.StoredProcedure);

                for (int i = 0; i < keywords.Count; i++)
                {

                    var parameters = new DynamicParameters();
                    parameters.Add("@Keyword", keywords[i].Keyword, DbType.AnsiString);
                    parameters.Add("@Pageviews", keywords[i].Pageviews, DbType.Int32);

                    cn.Execute("usp_InSearchKeywords_Insert_Keyword", parameters, commandType: CommandType.StoredProcedure);
                }
            }


        }
    }
}
