namespace AdapterLibrary;

using System.Numerics;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
using System.Runtime.Versioning;

public record VariableValue( 
  int Type, // String: 0, Numeric: 1, Boolean: 2, Date: 3
  string StringValue,
  bool BooleanValue,
  decimal NumericValue,
  DateTime? DateTimeValue
);

public record VariableState(
  string Id,
  string Name,
  int VariableType, // String: 0, Numeric: 1, Boolean: 2, Date: 3
  VariableValue Value
);

public record SectionState (
  string Id,
  VariableState[] Variables

);

public record PartConfigurationState(
  string SessionId,
  bool IsValid,
  string PartId,
  int Quantity,
  SectionState[] Sections
  // "ExpiresAt": "0001-01-01T00:00:00+00:00",
  // "CustomerId": 0,
  // "Comment": null,
  // "PartConfigurationTemplateSnapshotId": 0,
  // "PartConfigurationTemplateId": 0,
  // "PartConfigurationTemplateVersion": 0,
  // "PartConfigurationId": 0,
  // "AlternativePreparationCode": null,
  // "DiscountPercentage": 0.0,
  // "LockedDiscount": false,
  // "LockedUnitPrice": false,
  // "PriceFormulaFactor": 0.0,
  
  // "StandardPrice": null,
  // "UnitPrice": null,
  // "UnitPriceInCompanyCurrency": null,
  // "WeightPerUnit": null,
  
);



public class WebValidationResult
 {

  public required string Id;

  public required string Description;

  public required string[] ErrorMessages;

}

public class WebSelectionRowValue {

  public required string id;
  public required string partDescription;
  public required string partId;

  public required string partNumber;

  public required int quantity;

  public required WebValidationResult[] validationResults;

}

public class WebSelectionGroupState
{

  public required string id;
  public required string code;
  public required string description;

  public required WebSelectionRowValue[] values;

}

public record WebVariableState(
  string id, 
  string name,
  // public required string description;

  decimal value

  // public required WebValidationResult[] validationResults;
);

public record WebConfigurationState(

    string partNumber,
    string partId,
    Dictionary<string, decimal>[] variables
    // public required string partConfigurationId;
    // public required string configurationSessionId;
    // public required int quantity;

    // public required Dictionary<string, WebVariableState>[] Variables;
    // public required Dictionary<string, WebSelectionGroupState>[] SelectionGroups;
);




public class MonitorAPI
{

  public string configurationToWeb (string partConfigurationStateJSON) {

    PartConfigurationState? partConfigurationState = JsonSerializer.Deserialize<PartConfigurationState>(partConfigurationStateJSON, 
      new JsonSerializerOptions(JsonSerializerDefaults.General)
    );

    var variables = new Dictionary<string, decimal>();
        // {
        //   { "Width", new WebVariableState("707434600696463128", "Width", 100) },
        // };
    if (partConfigurationState != null) {
      for (int i = 0; i < partConfigurationState.Sections.Length; i++) 
      {

        var section = partConfigurationState.Sections[i];
        if (section != null) {
          for (int j = 0; j < section.Variables.Length; j++) {
            var sectionVariable = section.Variables[j];
            if (sectionVariable != null) {
              if ((sectionVariable.VariableType == 1) & (sectionVariable.Value.Type == 1)) {
                // var value = (sectionVariable.Value.NumericValue != null) ? sectionVariable.Value.NumericValue : 0;
                variables.Add(sectionVariable.Name, sectionVariable.Value.NumericValue);
              } else {
              }
            }
          }
        }
      }
    }

    var partNumber = "M-240"; // fetch from list of part number map

    var state = new 
    {
        partId = (partConfigurationState != null) ? partConfigurationState.PartId : "",
        partNumber = partNumber,
        // partConfigurationId = "",
        // configurationSessionId = "",
        // quantity = 1,
        variables,
        // SelectionGroups = [],
        // Variables = [],
    };

    string json = JsonSerializer.Serialize(state);

    return json;
  }

  public string webToConfigurationInstructions () {
    return "";
  }
}
