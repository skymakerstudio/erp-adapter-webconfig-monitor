# Monitor G5 PartConfigration web adapter
Library for making complex configuration structures in ERP Monitor G5 easy to work with in a web context

Library also eliminate dependency for internal guid references that changes in Monitor G5

## Example input / output to use on web for configuration
```json
{
  "partNumber": "M-240",
  "valid": true,
  "values": {
    "width": 100,
    "depth": 100
  },
  "texts": {
    "marking": "PG2400A"
  },
  "selections": {
    "thickness": [ 
      { "selection": "T1_5", "quantity": 1 },
    ],
  }
}
```

## Transform G5 configuraiton to Web
```C#
AdapterLibrary.MonitorAPI adapter = new AdapterLibrary.MonitorAPI();

string partConfigState = postRequestTo(monitorAPI + "Common/PartConfigurations/Get"); 

var partNumbers = client.postAsync(apiUrl + "Inventory/Parts?$select=Id,PartNumber");

string resultAsJsonString = adapter.configurationToWeb(partConfigState, partNumbers);

```

## Transform Web to G5 configuration
```C#
AdapterLibrary.MonitorAPI adapter = new AdapterLibrary.MonitorAPI();

const serielizedJson = """{"partNumber":"M-240","values":{"width":123,"depth":456},"texts":{"marking":"PG2400A"},"selections":{"thickness":["T1_5"]}}""";

string partConfigState = postRequestTo(monitorAPI + "Common/PartConfigurations/Get"); 

var partNumbers = client.postAsync(apiUrl + "Inventory/Parts?$select=Id,PartNumber");

string resultAsJsonString = adapter.configurationToWeb(partConfigState, partNumbers);

```


## FAQ

### Why is a list of all partNumbers needed?
additional partNumbers argument is required until PartConfigurationState rows contain the PartNumber information (feature request for expandable PartConfigurationState)

### Why cant we use the entire PartConfigurationState
PartConfigurationState contains too much information, and not all should be possible to modify on the web client side
