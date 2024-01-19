# Monitor G5 Web adapter for PartConfigration
Library for making complex configuration structures in ERP Monitor G5 easy to work with in a web context

Library also eliminate dependency for internal guid references that changes in Monitor G5

## Example input / output to use on web on product configuration
```json
{
  "partNumber": "M-240",
  "valid": true,
  "values": {
    "width": 100,
    "depth": 100,
  },
  "texts": {
    "marking": "PG2400A"
  },
  "booleans": {
    "compact": true,
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

string partConfigState = postRequestTo(monitorAPI + "Common/PartConfigurations/Get", args); 

var partNumbers = postRequestTo(apiUrl + "Inventory/Parts?$select=Id,PartNumber", args);

string resultAsJsonString = adapter.configurationToWeb(partConfigState, partNumbers);

```

## Example of instructions to Common/PartConfigurations/Update
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
            },
        },
        {
            "Type": 0,
            "Variable": {
                "VariableId": "707434668342198034",
                "Value": {
                    "Type": 1,
                    "NumericValue": 100,
                }
            },
        },
        {
            "Type": 1,
            "SelectionGroupRow": {
                "SelectionGroupRowId": "993518285080342908",
                "Selected": false,
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

## Transform Web to G5 PartConfiguration update
```C#
AdapterLibrary.MonitorAPI adapter = new AdapterLibrary.MonitorAPI();

const serielizedJson = """{"partNumber":"M-240","values":{"width":123,"depth":456},"texts":{"marking":"PG2400A"},"selections":{"thickness":["T1_5"]}}""";

string partConfigState = postRequestTo(monitorAPI + "Common/PartConfigurations/Get"); 

var partNumbers = postRequestTo(apiUrl + "Inventory/Parts?$select=Id,PartNumber");

string instructionsAsJson = adapter.webToConfigurationInstructions(serielizedJson, sessionId, partConfigStateResponse, partNumbersResponse);

string partConfigState = postRequestTo(apiUrl + "/Common/PartConfigurations/Update", instructionsAsJson)

```

## How to use

Include the AdapterLibrary in your project

## How to test - VSCODE

MSTests defined in Test folder

Install C# Devkit and run the Testing -> Run "Debug Tests" tab. Or in terminal:

```sh
dotnet test
```


## FAQ

### Why is a list of all partNumbers needed?
additional partNumbers argument is required until PartConfigurationState rows contain the PartNumber information (if one day SelectionGroupRow can be expanded direct in PartConfigurationState with PartNumber this can be removed)

### Why cant we use the entire PartConfigurationState direct?
PartConfigurationState contains too much information, and not all should be possible to modify on the web client side

### Why do we need the PartConfigurationState for generating the return instructions
A fresh PartConfigurationState, together with part number list, works as a definition for the latest guids.


