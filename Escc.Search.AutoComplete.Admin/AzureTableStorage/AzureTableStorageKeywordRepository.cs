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

            foreach (var keyword in keywords)
            {
                // Use a sanitised version of the keyword as the partition key because it's indexed for lookups,
                // but also save the keyword as-typed to present back to users
                var entity = new KeywordEntity()
                {
                    PartitionKey =  ToAzureKeyString(keyword.Keyword),
                    RowKey = keyword.PageViews.ToString("D7"),
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
