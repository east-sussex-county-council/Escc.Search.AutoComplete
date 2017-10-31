using Escc.GoogleAnalytics.Admin;
using Exceptionless;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escc.Search.AutoComplete.Admin.AzureTableStorage
{
    /// <summary>
    /// Saves keywords to Azure table storage
    /// </summary>
    public class AzureTableStorageKeywordRepository : IKeywordRepository
    {
        /// <summary>
        /// Saves the keywords.
        /// </summary>
        /// <param name="keywords">The keywords.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SaveKeywords(List<KeywordResult> keywords)
        {
            // Get the storage account connection
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["Escc.Search.AutoComplete.AzureStorage"].ConnectionString);

            // Create the table if it doesn't exist.
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("autocomplete");
            table.CreateIfNotExistsAsync().Wait();

            // Delete all the existing content
            DeleteAllExistingEntities(table);

            // Add the replacement content
            AddNewEntities(keywords, table);
        }

        private static void AddNewEntities(List<KeywordResult> keywords, CloudTable table)
        {
            foreach (var keyword in keywords)
            {
                // Azure tables use an index clustered first by partition key then by row key.
                //
                // When we look up a keyword we will want matching search terms ordered by page views.
                // To get that we need to have results ordered by page views, then filter the list to only search terms that match, 
                // which means the partition key has to be based on page views and the row key based on search terms.
                //
                // The partition key is a string, so convert the number of page views to a string and pad with leading 0s so that
                // the alpha sort gives the same result as a numeric sort. However this still sorts low numbers of page views ahead
                // of high, so we need to change low numbers to high ones and vice versa to get the right sort order. Subtracting 1000000
                // makes the numbers of page views negative (assuming they're under 1000000), and multiplying by -1 removes the minus sign,
                // giving us the sort order we want.
                //
                // The row key has to be a sanitised version of the keyword because a key can't contain common search term characters such 
                // as / and ?, so save the keyword separately as-typed so that it can be presented back to users.
                var entity = new KeywordEntity()
                {
                    PartitionKey = ((keyword.PageViews - 1000000) * -1).ToString("D8"),
                    RowKey = ToAzureKeyString(keyword.Keyword),
                    Keyword = keyword.Keyword,
                    FeedDate = keyword.FeedDate.ToShortDateString()
                };

                // Create the TableOperation object that inserts the entity.
                var insertOperation = TableOperation.InsertOrReplace(entity);

                // Execute the insert operation.
                try
                {
                    table.Execute(insertOperation);
                }
                catch (StorageException ex)
                {
                    if (ex.Message.Contains("(400) Bad Request"))
                    {
                        Console.WriteLine(keyword.Keyword + " returned " + ex.RequestInformation.ExtendedErrorInformation.ErrorMessage);
                        ex.ToExceptionless().Submit();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private static void DeleteAllExistingEntities(CloudTable table)
        {
            var rangeQuery = new TableQuery<TableEntity>();
            foreach (TableEntity entity in table.ExecuteQuery(rangeQuery))
            {
                try
                {
                    table.Execute(TableOperation.Delete(entity));
                }
                catch (StorageException ex)
                {
                    if (ex.Message.Contains("(400) Bad Request"))
                    {
                        Console.WriteLine(entity.RowKey + " returned " + ex.RequestInformation.ExtendedErrorInformation.ErrorMessage);
                        ex.ToExceptionless().Submit();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        // From http://stackoverflow.com/questions/14859405/azure-table-storage-returns-400-bad-request
        public static string ToAzureKeyString(string str)
        {
            var sb = new StringBuilder();
            foreach (var c in str
                .Where(c => c != '/'
                            && c != '\\'
                            && c != '#'
                            && c != '/'
                            && c != '?'
                            && !char.IsControl(c)))
                sb.Append(c);
            return sb.ToString();
        }
    }
}
