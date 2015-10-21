using System.Collections.Generic;

namespace Escc.Search.AutoComplete
{
    /// <summary>
    /// A place from which keywords can be read
    /// </summary>
    public interface IKeywordSource
    {
        /// <summary>
        /// Reads the search suggestions.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <returns></returns>
        IEnumerable<string> ReadSearchSuggestions(string searchTerm);
    }
}