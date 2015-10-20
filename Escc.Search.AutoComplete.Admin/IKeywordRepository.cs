using System.Collections.Generic;

namespace Escc.GoogleAnalytics.Admin
{
    public interface IKeywordRepository
    {
        void SaveKeywords(List<KeywordResult> keywords);
    }
}