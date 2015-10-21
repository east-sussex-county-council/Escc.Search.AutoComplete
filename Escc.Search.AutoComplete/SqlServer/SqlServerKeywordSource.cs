using Exceptionless;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Escc.Search.AutoComplete.SqlServer
{
    /// <summary>
    /// Reads keywords from a SQL Server database
    /// </summary>
    public class SqlServerKeywordSource : IKeywordSource
    {
        /// <summary>
        /// Gets the search keywords from a database
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        public IEnumerable<string> ReadSearchSuggestions(string searchTerm)
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