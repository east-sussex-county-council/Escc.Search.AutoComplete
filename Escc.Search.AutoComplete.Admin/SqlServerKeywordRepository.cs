using Microsoft.ApplicationBlocks.Data;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using System;
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
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@Keyword", SqlDbType.VarChar);
            parameters[1] = new SqlParameter("@Pageviews", SqlDbType.Int);

            try
            {
                if (keywords.Count > 5000)
                {
                    SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "usp_InSearchKeywords_Delete");

                    for (int i = 0; i < keywords.Count; i++)
                    {

                        parameters[0].Value = keywords[i].Keyword;
                        parameters[1].Value = keywords[i].Pageviews;

                        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "usp_InSearchKeywords_Insert_Keyword", parameters);

                    }
                }


            }
            catch (SqlException ex)
            {
                ExceptionManager.Publish(ex);
            }

        }
    }
}
