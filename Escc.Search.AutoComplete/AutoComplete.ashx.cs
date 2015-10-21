using System;
using System.Globalization;
using System.Web;
using EsccWebTeam.Data.Web;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using Exceptionless;
using Escc.Search.AutoComplete.AzureTableStorage;

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
                var keywords = new AzureTableStorageKeywordSource().ReadSearchSuggestions(context.Request.QueryString["term"].ToLower(CultureInfo.CurrentCulture));
                WriteResultAsJson(context, keywords);
            }
        }

        private void ProcessCorsHeaders(HttpContext context)
        {
           Cors.AllowCrossOriginRequest(context.Request, context.Response, new ConfigurationCorsAllowedOriginsProvider().CorsAllowedOrigins());
        }

      

        /// <summary>
        /// Outputs the search terms as a JSON response
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="keywords">The keywords.</param>
        private void WriteResultAsJson(HttpContext context, IEnumerable<string> keywords)
        {
            context.Response.ContentType = "text/javascript";
            context.Response.Write("[");

            var indexedKeywords = new List<string>(keywords);

            int len = indexedKeywords.Count;
            for (int i = 0; i < len; i++)
            {

                // preferred term needs only a value property
                context.Response.Write("{ \"label\": \"" + indexedKeywords[i].ToString() + "\", \"value\": \"" + indexedKeywords[i].ToString() + "\" }");

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