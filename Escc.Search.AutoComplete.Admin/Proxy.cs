using Escc.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Specialized;
using System.Configuration;

namespace Escc.Search.AutoComplete.Admin
{
    class Proxy : IProxyProvider
    {
        public IWebProxy CreateProxy()
        {
            IWebProxy Proxya = WebRequest.GetSystemWebProxy();

            string user = null;
            string password = null;
            string domain = null;

            // Load settings from web.config which allow requests to go out through the ESCC proxy server
            NameValueCollection config = ConfigurationManager.GetSection("Escc.Net/Proxy") as NameValueCollection;

            if (config != null && config["User"] != null)
            {
                user = config["User"];
                password = (config["Password"] != null) ? config["Password"] : String.Empty;
                domain = (config["Domain"] != null) ? config["Domain"] : String.Empty;

                Proxya.Credentials = new NetworkCredential(user, password, domain);
                return Proxya;
            }
            else
            {
                // If no account in web.config, just use current credentials
                Proxya.Credentials = CredentialCache.DefaultCredentials;
                return Proxya;
            }
        }
    }
}
