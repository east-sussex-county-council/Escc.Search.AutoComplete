# Escc.Search.AutoComplete

When you enter a query in the search box on [www.eastsussex.gov.uk](https://www.eastsussex.gov.uk), it suggests some things you might be looking for. These suggestions are based on the search terms of other users, recorded in Google Analytics. 

The dataset is read using the Google Analytics API, cleaned up and saved in a repository, from where it is published as JSON ready for the autocomplete feature to use. The JSON format is that used by the JQuery UI autocomplete feature. 

The project contains code for two possible repositories, SQL Server and Azure table storage.

## Development setup steps

1. From an Administrator command prompt, run `app-setup-dev.cmd` to set up a site in IIS.
2. Build the solution
3. Update `app.config` and/or `web.config` with connection details for Google Analytics, SQL Server or your Azure storage account, depending which ones you are using.