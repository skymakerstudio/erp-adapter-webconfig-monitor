# Monitor G5 ERP - Web adapter for PartConfiguration

This library simplifies working with complex data structures for the Monitor G5 API in a web context. It also eliminates
dependencies on internal GUID references that tend to change with version in the G5 API.

To use this library, include the AdapterLibrary in your project.

Use either with partial update of webToConfigurationInstructions if **full sales configurator in Monitor** or full update of webToConfigurationInstructions if its a **minimal product configurator in monitor**. See FAQ below for more details on this.

# Net 6 and 8 
The maste is written in C# 8.

There is also a branch with C# 6 available [net6](https://github.com/skymakerstudio/erp-adapter-webconfig-monitor/tree/net6)


## Example of input/output data structure for the simplified web API

```json
{
  "partNumber": "ConfigurablePlate1",
  "values": {
    "width": 100,
    "depth": 100
  },
  "texts": {
    "marking": "PG2400A"
  },
  "booleans": {
    "compact": true
  },
  "selections": {
    "thickness": [
      {
        "selection": "T1_5",
        "quantity": 1.0
      }
    ]
  }
}
```

Wireframe of corresponding fields in a configurable article in Monitor G5 (that can be filled in automatically with this adapter)
<img width="1029" height="597" alt="Konfigurator-Wireframe---erp-adapter-example" src="https://github.com/user-attachments/assets/453491fe-6714-40c5-b986-98c0a069950d" />


## Example of how to transform G5 configuration to simplified web API request (pseudo code)

```C#
AdapterLibrary.MonitorAPI adapter = new AdapterLibrary.MonitorAPI();

string partConfigState = PostRequestTo(monitorApiUrl + "Common/PartConfigurations/Get", args);

var partNumbers = GetRequestTo(monitorApiUrl + "Inventory/Parts?$select=Id,PartNumber", args);

string resultAsJsonString = adapter.configurationToWeb(partConfigState, partNumbers);
```

## Example of instructions to Common/PartConfigurations/Update (see [Monitor API Docs](https://api.monitor.se/api/Monitor.API.Common.Commands.PartConfigurations.UpdatePartConfiguration.html))

```json
{
  "SessionId": "b6ee6341-92ed-483a-85f5-73180bc04c42",
  "Instructions": [
    {
      "Type": 0,
      "Variable": {
        "VariableId": "707434600696463128",
        "Value": {
          "Type": 1,
          "StringValue": null,
          "BooleanValue": null,
          "NumericValue": 100,
          "DateTimeValue": null
        }
      }
    },
    {
      "Type": 0,
      "Variable": {
        "VariableId": "707434668342198034",
        "Value": {
          "Type": 1,
          "NumericValue": 100
        }
      }
    },
    {
      "Type": 1,
      "SelectionGroupRow": {
        "SelectionGroupRowId": "993518285080342908",
        "Selected": false
      }
    },
    {
      "Type": 1,
      "SelectionGroupRow": {
        "SelectionGroupRowId": "993518285080342909",
        "Selected": true,
        "Quantity": 1
      }
    }
  ]
}
```

## Example of transforming simplified web API request to G5 PartConfiguration update (pseudo code)

```C#
AdapterLibrary.MonitorAPI adapter = new AdapterLibrary.MonitorAPI();

const partialJson = """{"partNumber":"ConfigurablePlate1","values":{"width":123,"depth":456},"texts":{"marking":"PG2400A"},"selections":{"thickness":["T1_5"]}}""";

string partConfigState = PostRequestTo(monitorApiUrl + "Common/PartConfigurations/Get");

var partNumbers = GetRequestTo(monitorApiUrl + "Inventory/Parts?$select=Id,PartNumber");

string instructionsAsJson = adapter.webToConfigurationInstructions(partialJson, sessionId, partConfigStateResponse, partNumbersResponse);

string partConfigStateAfterUpdate = PostRequestTo(monitorApiUrl + "/Common/PartConfigurations/Update", instructionsAsJson)
```

## Get G5 configurator definition to web for synchronization (pseudo code) ([see Monitor API docs](https://api.monitor.se/api/Monitor.API.Common.Commands.PartConfigurations.GetPartConfiguration.html))

```C#
AdapterLibrary.MonitorAPI adapter = new AdapterLibrary.MonitorAPI();

string partConfigState = PostRequestTo(monitorApiUrl + "Common/PartConfigurations/Get", args);

// Description included for use with SelectionGroup row labels
var partNumbersWithDescriptions = GetRequestTo(monitorApiUrl + "Inventory/Parts?$select=Id,PartNumber,Description", args);

string augmentedPartConfigurationState = adapter.getConfiguratorDefinition(partConfigState, partNumbersWithDescriptions);
```

## How to test using VSCode

1. Install [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
2. Follow [Gettings Started](https://code.visualstudio.com/docs/csharp/get-started)
3. Run the Testing -> Run "Debug Tests" tab. Or in a terminal, run:
    ```sh
    dotnet build
    dotnet test
    ```

## FAQ

### Why is a list of all partNumbers needed?

An additional `partNumbers` argument is required until `PartConfigurationState` rows contain the `PartNumber`
information. If one day `SelectionGroupRow` can be expanded directly in `PartConfigurationState` with `PartNumber`, this
argument can be removed.

### Why do we need the `PartConfigurationState` for generating the return instructions?

A fresh `PartConfigurationState`, together with a part number list, works as a definition for the latest GUIDs.

### How should I use `getConfiguratorDefinition`?

Instead of sending lots of data every time an update is done, the important data for structure can be handled via the
definition. While actual inputs and outputs only contain data that should change.

### Should I return every value and selection group or just what has been changed?

Since Monitor G5 is computing and validating everything side and returns the result you have two alternatives when you set up the ERP adapter:

1. **Full sales configurator in monitor.**
Do a partial update of webToConfigurationInstructions(partialJson) and react to the results. Use this pattern when you have all the rules in Monitor G5 and need a result every time something is changed by the user. User updates input X-> webToConfigurationInstructions(X) -> result from monitor -> Update UI + Visualization

2. **Minimal configurator in Monitor.** Do a full update to webToConfigurationInstructions and expect the configuration to be completed. Use this pattern when you have some or all rules outside Monitor G5 and just need to "fill in" the fields in a Monitor G5 to enable manufacturing preparations.

