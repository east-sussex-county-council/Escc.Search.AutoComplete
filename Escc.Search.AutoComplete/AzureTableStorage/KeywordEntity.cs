using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Escc.Search.AutoComplete.AzureTableStorage
{
    /// <summary>
    /// A keyword returned from Azure table storage
    /// </summary>
    public class KeywordEntity : TableEntity
    {
        public string Keyword { get; set; }
    }
}