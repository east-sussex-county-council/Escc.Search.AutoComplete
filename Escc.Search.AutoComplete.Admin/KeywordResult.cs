using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Escc.GoogleAnalytics.Admin
{
    public class KeywordResult
    {
        public string Keyword { get; set; }
        public int PageViews { get; set; }

        public DateTime FeedDate { get; set; }
    }
}
