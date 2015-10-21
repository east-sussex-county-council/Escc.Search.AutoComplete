using System;
using System.Globalization;
using System.Web;
using EsccWebTeam.Data.Web;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using Exceptionless;

namespace Escc.Search.AutoComplete
{
    /// <summary>
    /// Return keywords as JSON, to be used as search suggestions
    /// </summary>
    public class AutoComplete : IHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            ProcessCorsHeaders(context);

            if (!String.IsNullOrEmpty(context.Request.QueryString["term"]))
            {
                var keywords = ReadSearchSuggestions(context.Request.QueryString["term"].ToLower(CultureInfo.CurrentCulture));
                WriteResultAsJson(context, keywords);
            }
        }

        private void ProcessCorsHeaders(HttpContext context)
        {
           Cors.AllowCrossOriginRequest(context.Request, context.Response, new ConfigurationCorsAllowedOriginsProvider().CorsAllowedOrigins());
        }

        /// <summary>
        /// Gets the search keywords from a database
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        public string[] ReadSearchSuggestions(string searchTerm)
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

        /// <summary>
        /// Outputs the search terms as a JSON response
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="keywords">The keywords.</param>
        private void WriteResultAsJson(HttpContext context, string[] keywords)
        {
            context.Response.ContentType = "text/javascript";
            context.Response.Write("[");

            int len = keywords.Length;
            for (int i = 0; i < len; i++)
            {

                // preferred term needs only a value property
                context.Response.Write("{ \"label\": \"" + keywords[i].ToString() + "\", \"value\": \"" + keywords[i].ToString() + "\" }");

                if (i < len - 1) HttpContext.Current.Response.Write(",");
            }

            context.Response.Write("]");
        }



        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
        /// </returns>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}