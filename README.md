# Escc.Search.AutoComplete

When you enter a query in the search box on [www.eastsussex.gov.uk](https://www.eastsussex.gov.uk), it suggests some things you might be looking for. These suggestions are based on the search terms of other users, recorded in Google Analytics. 

The dataset is read using the Google Analytics API, cleaned up and saved in a database, from where it is published as JSON ready for the autocomplete feature to use. The JSON format is that used by the JQuery UI autocomplete feature. 

## Development setup steps

1. From an Administrator command prompt, run `app-setup-dev.cmd` to set up a site in IIS.
2. Build the solution
