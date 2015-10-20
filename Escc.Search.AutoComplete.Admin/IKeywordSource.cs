using System.Collections.Generic;

namespace Escc.GoogleAnalytics.Admin
{
    public interface IKeywordSource
    {
        List<KeywordResult> ReadKeywords();
    }
}