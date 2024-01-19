# Monitor G5 PartConfigration web adapter
Library for making complex configuration structures in ERP Monitor G5 easy to work with in a web context

Library also eliminate dependency for internal guid references that changes in Monitor G5

## Transform G5 configuraiton to Web
```C#
AdapterLibrary.MonitorAPI adapter = new AdapterLibrary.MonitorAPI();

string partConfigState = postRequestTo(monitorAPI + "Common/PartConfigurations/Get"); 

var partNumbers = client.postAsync(apiUrl + "Inventory/Parts?$select=Id,PartNumber");

string resultAsJsonString = adapter.configurationToWeb(partConfigState, partNumbers);

```
*additional partNumbers argument is required until PartConfigurationState rows contain the PartNumber information (feature request for expandable PartConfigurationState)

## Example output
```json
{
  "partNumber": "M-240",
  "values": {
    "width": 100,
    "depth": 100
  },
  "texts": {
    "marking": "PG2400A"
  },
  "selections": {
    "thickness": ["T1_5"],
  }
}
```