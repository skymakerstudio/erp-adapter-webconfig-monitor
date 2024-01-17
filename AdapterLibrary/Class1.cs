namespace AdapterLibrary;
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

public class WebConfigurationState
{
    public required string[] Variables { get; set; }
    public required string[] SelectionGroups { get; set; }
    // public bool valid { get; set; }
}


public class MonitorAPI
{

  public string configurationToWeb (string partConfigurationStateJSON) {

    PartConfigurationState? partConfigurationState = JsonSerializer.Deserialize<PartConfigurationState>(partConfigurationStateJSON);

    var state = new WebConfigurationState
    {
        SelectionGroups = ["ABC"],
        Variables = ["CDE"],
    };

    string json = JsonSerializer.Serialize(state);

    return json;
  }

  public string webToConfigurationInstructions () {
    return "";
  }
}
