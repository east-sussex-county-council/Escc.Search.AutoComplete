using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Escc.Search.AutoComplete.AzureTableStorage
{
    /// <summary>
    /// Reads keywords from Azure table storage
    /// </summary>
    public class AzureTableStorageKeywordSource : IKeywordSource
    {
        /// <summary>
        /// Reads the search suggestions.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<string> ReadSearchSuggestions(string searchTerm)
        {
            // Retrieve the storage account from the connection string.
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["Escc.Search.AutoComplete.AzureStorage"].ConnectionString);

            // Create the table query.
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("autocomplete");

            var rangeQuery = new TableQuery<KeywordEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, searchTerm),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, IncrementLastCharacter(searchTerm)))).Take(10);

            // Loop through the results, displaying information about the entity.
            var list = new List<string>();
            foreach (KeywordEntity entity in table.ExecuteQuery(rangeQuery))
            {
                list.Add(entity.Keyword);
            }

            return list;
        }

        private string IncrementLastCharacter(string searchTerm)
        {
            if (String.IsNullOrEmpty(searchTerm)) return searchTerm;

            // Change the last character of the search term to the next character in the ASCII table
            return searchTerm.Substring(0,searchTerm.Length-1) + (char)(searchTerm[searchTerm.Length-1] + 1);
        }
    }
}