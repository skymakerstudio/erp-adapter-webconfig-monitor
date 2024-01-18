namespace AdapterLibrary;

using System.Numerics;
using System.Text.Json;

public class  PartConfigurationState
{
  // public required string SessionId { get; set; }
  // "ExpiresAt": "0001-01-01T00:00:00+00:00",
  // "CustomerId": 0,
  // "Comment": null,
  // "PartConfigurationTemplateSnapshotId": 0,
  // "PartConfigurationTemplateId": 0,
  // "PartConfigurationTemplateVersion": 0,
  // "PartConfigurationId": 0,
  // "IsValid": false,
  // "AlternativePreparationCode": null,
  // "DiscountPercentage": 0.0,
  // "LockedDiscount": false,
  // "LockedUnitPrice": false,
  // "PartId": 0,
  // "PriceFormulaFactor": 0.0,
  // "Quantity": 0.0,
  // "StandardPrice": null,
  // "UnitPrice": null,
  // "UnitPriceInCompanyCurrency": null,
  // "WeightPerUnit": null,
  public required string[] Sections;
}

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

public class WebVariableState
{
  public required string id;
  public required string code;
  public required string description;

  public required string value;

  public required WebValidationResult[] validationResults;
}

public class WebConfigurationState
{
    public required string partNumber;
    public required string partId;

    public required string partConfigurationId;
    public required string configurationSessionId;
    public required int quantity;

    public required Dictionary<string, WebVariableState>[] Variables;
    public required Dictionary<string, WebSelectionGroupState>[] SelectionGroups;
}


public class MonitorAPI
{

  public string configurationToWeb (string partConfigurationStateJSON) {

    PartConfigurationState? partConfigurationState = JsonSerializer.Deserialize<PartConfigurationState>(partConfigurationStateJSON);

    var partNumber = "123";

    var state = new WebConfigurationState
    {
        partId = "123",
        partNumber = partNumber,
        partConfigurationId = "",
        configurationSessionId = "",
        quantity = 1,
        SelectionGroups = [],
        Variables = [],
    };

    string json = JsonSerializer.Serialize(state);

    return json;
  }

  public string webToConfigurationInstructions () {
    return "";
  }
}
