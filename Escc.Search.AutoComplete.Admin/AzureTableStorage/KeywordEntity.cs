using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escc.Search.AutoComplete.Admin.AzureTableStorage
{
    /// <summary>
    /// A search keyword represented as an entity in an Azure table
    /// </summary>
    public class KeywordEntity : TableEntity {
        public string Keyword { get; set; }
        public string FeedDate { get; set; }
    }
}
