using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using Exceptionless;

namespace Escc.GoogleAnalytics
{
    /// <summary>
    /// Query search terms recorded by Google Analytics
    /// </summary>
    [WebService(Namespace = "http://www.eastsussex.gov.uk/Escc.GoogleAnalytics")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class Service : System.Web.Services.WebService
    {
        /// <summary>
        /// Gets the search keywords from a database, which is a cached copy of the data from Google Analytics
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        [WebMethod(Description = "Gets the search keywords from a database, which is a cached copy of the data from Google Analytics")]
        public string[] GoogleAnalyticsSearchSuggestions(string searchTerm)
        {
            SqlConnection cn = null;
            try
            {
                try
                {
                    cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Escc.Search.AutoComplete.Reader"].ConnectionString);
                }
                catch (NullReferenceException ex)
                {
                    ex.Data.Add("Connection string missing from web.config", "Escc.Search.AutoComplete.Reader");
                    ex.ToExceptionless().Submit();
                    throw;
                }

                var parameter = new SqlParameter("@SearchTerm", SqlDbType.VarChar, 500);
                parameter.Value = searchTerm;

                DataSet matchedKeywords = new DataSet();
                List<string> keywords = new List<string>();

                try
                {
                    var command = cn.CreateCommand();
                    command.CommandText = "usp_InSearchKeywords_Select_BySearchTerm";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(parameter);
                    var adapter = new SqlDataAdapter(command);
                    adapter.Fill(matchedKeywords);
                    
                    foreach (DataRow row in matchedKeywords.Tables[0].Rows)
                    {
                        keywords.Add(row["Keyword"].ToString());
                    }

                    var keywordsArray = new string[keywords.Count];
                    keywords.CopyTo(keywordsArray);
                    return keywordsArray;
                }
                catch (SqlException ex)
                {
                    ex.ToExceptionless().Submit();
                    throw;
                }
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                throw;
            }
            finally
            {
                if (cn != null) cn.Dispose();
            }
        }
    }
}
