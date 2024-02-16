namespace AdapterLibrary;

using System.Text.Json;
using System;
using System.Collections.Generic;

// ---------------------------- Monitor API read records ---------------------------------
public record VariableValue( 
  int Type, // String: 0, Numeric: 1, Boolean: 2, Date: 3
  string? StringValue,
  bool? BooleanValue,
  double? NumericValue,
  DateTime? DateTimeValue
);

public record VariableState(
  string Id,
  string Name,
  int VariableType, // String: 0, Numeric: 1, Boolean: 2, Date: 3
  VariableValue Value
);

public record SelectionGroupState(
  string Code,
  string Id,
  SelectionGroupRowState[] Rows
);

public record SelectionGroupRowState(
  string Id,
  string PartId,
  bool IsSelected,
  int Quantity
)
{
  public string? PartNumber { get; set; } // Not part of standard response
  public string? Description { get; set; } // Not part of standard response
};

public record SectionState (
  string Id,
  VariableState[] Variables,
  SelectionGroupState[] SelectionGroups,
  SectionState[] Sections
);

public record PartConfigurationState(
  string SessionId,
  bool IsValid,
  string PartId,
  int Quantity,
  SectionState[] Sections 
);


// ----------------------------- Monitor API update records ---------------------------------------
public record VariableUpdate(
  string VariableId,
  VariableValue Value
);

public record UpdatePartConfigurationInstruction(
  int Type, // 0 = Variable, 1 = SelectionGroupRow
  VariableUpdate? Variable,
  SelectionGroupRowUpdate? SelectionGroupRow
);

public record SelectionGroupRowUpdate(
  string SelectionGroupRowId,
  string PartId,
  bool? Selected,
  int? Quantity
);


// ------------------------------ Maps & Helper records -------------------------------------------------

public record PartNumberMap (
  string Id,
  string PartNumber,
  string? Description
);

public record RowPartIdMap (
  string Id,
  string PartId
);

public record WebConfigurationIdMap (
  Dictionary<string, string> values,
  Dictionary<string, string> texts,
  Dictionary<string, string> booleans,
  Dictionary<string, string> selectionGroups,
  Dictionary<string, List<string>> selectionRows,
  Dictionary<string, List<RowPartIdMap>> selectionGroupRowIds
);

// --------------------------------- Web configuration API ----------------------------------------
public class WebValidationResult
 {

  public required string Id;

  public required string Description;

  public required string[] ErrorMessages;
}

public record WebSelectionRowItem (
  string selection,
  int quantity
);

public record WebConfigurationState(

    string partNumber,
    Dictionary<string, double> values,
    Dictionary<string, string> texts,
    Dictionary<string, bool> booleans,
    Dictionary<string, WebSelectionRowItem[]> selections
);


// ----------------------------------- Main Adapter class -----------------------------------------
public class MonitorAPI
{

  private static PartNumberMap getPartFromPartId (string partId, List<PartNumberMap>? partNumberList) {
    var partDef = (partNumberList ?? []).Find(item => item.Id == partId);
    if (partDef != null) {
      return partDef;
    } else {
      throw new Exception();
    }
  }

  private static List<VariableState> getAllVariablesFromSections (SectionState[] sections) {
    List<VariableState> variables = new List<VariableState>();
    for (int i = 0; i < sections.Length; i++) {
      var section = sections[i];
      // section.Variables
      for (int j = 0; j < section.Variables.Length; j++) {
        var currVar = section.Variables[j];
        variables.Add(currVar);
      }

      // Recursive fetch of sub sections content
      if (section.Sections.Length > 0) {
        var subVariables = getAllVariablesFromSections(section.Sections);
        foreach (VariableState subVar in subVariables) {
          variables.Add(subVar);
        }
      }
      
    }
    return variables;
  }

  private static List<SelectionGroupState> getAllSelectionGroupsFromSections (SectionState[] sections) {
    List<SelectionGroupState> selectionGroups = new List<SelectionGroupState>();
    for (int i = 0; i < sections.Length; i++) {
      var section = sections[i];
      // section.Variables
      for (int j = 0; j < section.SelectionGroups.Length; j++) {
        var currVar = section.SelectionGroups[j];
        selectionGroups.Add(currVar);
      }

      // Recursive fetch of sub sections content
      if (section.Sections.Length > 0) {
        var subGroups = getAllSelectionGroupsFromSections(section.Sections);
        foreach (SelectionGroupState subSelection in subGroups) {
          selectionGroups.Add(subSelection);
        }
      }
      
    }
    return selectionGroups;
  }

  // Currently StateSelectionRow is missing the PartNumber and Description which has to be added
  private static void appendMissingDataToPartConfigurationStateSection (SectionState[] sections, List<PartNumberMap> partIdList) {
    
    for (int i = 0; i < sections.Length; i++) {
      var section = sections[i];
      // section.Variables
      for (int j = 0; j < section.SelectionGroups.Length; j++) {
        var group = section.SelectionGroups[j];
        foreach (SelectionGroupRowState row in group.Rows) {
          var part = getPartFromPartId(row.PartId, partIdList);
          row.PartNumber = part.PartNumber;
          row.Description = part.Description;
        }
      }

      // Recursive append of missing sub sections content
      if (section.Sections.Length > 0) {
        appendMissingDataToPartConfigurationStateSection(section.Sections, partIdList);
      }
      
    }
  }

  public WebConfigurationIdMap generateCodeToIdMaps (string partConfigurationStateJSON, string partNumberListJSON) { 
    PartConfigurationState? partConfigurationState = JsonSerializer.Deserialize<PartConfigurationState>(partConfigurationStateJSON, 
      new JsonSerializerOptions(JsonSerializerDefaults.General)
    );

    List<PartNumberMap>? partIdList = JsonSerializer.Deserialize<List<PartNumberMap>>(partNumberListJSON, new JsonSerializerOptions(JsonSerializerDefaults.General) ); // todo: Replace when $expand is suppoted for PartNumber on PartConfigurationState (issue submitted to support)

    Dictionary<string, string> valueNameToId = new Dictionary<string, string>();
    Dictionary<string, string> textNameToId = new Dictionary<string, string>();
    Dictionary<string, string> booleanNameToId = new Dictionary<string, string>();
    Dictionary<string, string> selectionGroupCodeToId = new Dictionary<string, string>();
    Dictionary<string, List<string>> selectionRowPartNumberToId = new Dictionary<string, List<string>>();
    Dictionary<string, List<RowPartIdMap>> selectionGroupRowIds = new Dictionary<string, List<RowPartIdMap>>();

    var sections = partConfigurationState?.Sections ?? [];
    var variablesInSections = getAllVariablesFromSections(sections);

    for (int i = 0; i<variablesInSections.Count; i++) {
      var currVar = variablesInSections[i];
      if (currVar.VariableType == 0) { // string
        textNameToId.Add(currVar.Name, currVar.Id);
      } else if (currVar.VariableType == 1) { // number
        valueNameToId.Add(currVar.Name, currVar.Id);
      } else if (currVar.VariableType == 3) { // boolean
        booleanNameToId.Add(currVar.Name, currVar.Id);
      } else {
        // todo: Handle Date and unkown
      }
    }

    var selectionGroupsInSections = getAllSelectionGroupsFromSections(sections);
    for (int i = 0; i<selectionGroupsInSections.Count; i++) {
      var currSection = selectionGroupsInSections[i];
      selectionGroupCodeToId.Add(currSection.Code, currSection.Id);
      List<RowPartIdMap> rowIdList = new List<RowPartIdMap>();
      for (int j = 0; j<currSection.Rows.Length; j++) {
        var row = currSection.Rows[j];
        var partNumber = getPartFromPartId(row.PartId, partIdList).PartNumber;

        if (selectionRowPartNumberToId.ContainsKey(partNumber))
        {
            selectionRowPartNumberToId[partNumber].Add(row.Id);
        }
        else
        {
            selectionRowPartNumberToId[partNumber] = new List<string>() { row.Id };
        }

        rowIdList.Add(new RowPartIdMap(row.Id, row.PartId));
      }
      selectionGroupRowIds.Add(currSection.Code, rowIdList);
    }

    var map = new WebConfigurationIdMap(
      valueNameToId,
      textNameToId,
      booleanNameToId,
      selectionGroupCodeToId,
      selectionRowPartNumberToId,
      selectionGroupRowIds
    );
    return map;
  }

  public string configurationToWeb (string partConfigurationStateJSON, string partNumberListJSON) {

    PartConfigurationState? partConfigurationState = JsonSerializer.Deserialize<PartConfigurationState>(partConfigurationStateJSON, 
      new JsonSerializerOptions(JsonSerializerDefaults.General)
    );

    List<PartNumberMap>? partIdList = JsonSerializer.Deserialize<List<PartNumberMap>>(partNumberListJSON, new JsonSerializerOptions(JsonSerializerDefaults.General) ); // todo: Replace when $expand is suppoted for PartNumber on PartConfigurationState (issue submitted to support)

    
    var values = new Dictionary<string, double>();
    var texts = new Dictionary<string, string>();
    var selections = new Dictionary<string, List<WebSelectionRowItem>>();
        // {
        //   { "Width", new WebVariableState("707434600696463128", "Width", 100) },
        // };
    if (partConfigurationState != null) {
      
      for (int i = 0; i < partConfigurationState.Sections.Length; i++) 
      {

        var section = partConfigurationState.Sections[i];
        if (section != null) {
          for (int j = 0; j < section.Variables.Length; j++) {
            var secVariable = section.Variables[j];
            if (secVariable != null) {
              if ((secVariable.VariableType == 1) & (secVariable.Value.Type == 1) & (secVariable.Value.NumericValue != null)) {
                double value = secVariable.Value.NumericValue ?? 0;
                values.Add(secVariable.Name, value);
              } else {
                // todo: Add String to texts and Date to dates
              }
            }
          }

          for (int j = 0; j < section.SelectionGroups.Length; j++) {
            var selGroup = section.SelectionGroups[j];
            if (selGroup != null) {
              List<WebSelectionRowItem> selectedRows = new List<WebSelectionRowItem>();
              
              for (int k = 0; k < selGroup.Rows.Length; k++) {
                var row = selGroup.Rows[k];
                if (row.IsSelected) {
                  var rowPartNumber = getPartFromPartId(row.PartId, partIdList ?? ([])).PartNumber;
                  var webValue = new WebSelectionRowItem(rowPartNumber, row.Quantity);
                  selectedRows.Add(webValue);
                }
              }

              selections.Add(selGroup.Code, selectedRows);
                // var value = (sectionVariable.Value.NumericValue != null) ? sectionVariable.Value.NumericValue : 0;
              
            }
          }
        }
      }
    }

    var partId = (partConfigurationState != null) ? partConfigurationState.PartId : "";
    var partNumber = getPartFromPartId(partId, partIdList ?? ([])).PartNumber;

    bool valid = (partConfigurationState != null) ? partConfigurationState.IsValid : false;

    var state = new 
    {
        partNumber,
        valid,
        // quantity = 1,
        values,
        texts,
        selections,
    };

    string json = JsonSerializer.Serialize(state,  new 
     System.Text.Json.JsonSerializerOptions()  { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull 
    });

    return json;
  }

  public string webToConfigurationInstructions (string webConfigStateJSON, string sessionId, string partConfigurationStateJSON, string partNumberListJSON) {
    
    WebConfigurationState? webConfigState =  JsonSerializer.Deserialize<WebConfigurationState>(webConfigStateJSON, 
      new JsonSerializerOptions(JsonSerializerDefaults.General)
    );

    var mapCodeToId = generateCodeToIdMaps(partConfigurationStateJSON, partNumberListJSON);

    var instructions = new List<UpdatePartConfigurationInstruction>();

    if (webConfigState != null) {
      if (webConfigState.values != null) {
        foreach (string key in webConfigState.values.Keys) { 
            var numericValue = webConfigState.values[key];
            string variableId = mapCodeToId.values.ContainsKey(key) ? (mapCodeToId.values[key] ?? "") : "";
            instructions.Add(
              new UpdatePartConfigurationInstruction(0, new VariableUpdate(variableId,  new VariableValue(1, null, null, numericValue, null)), null)
            );
        }
      }
      if (webConfigState.texts != null) {
        foreach (string key in webConfigState.texts.Keys) { 
            var textValue = webConfigState.texts[key];
            string variableId = mapCodeToId.texts.ContainsKey(key) ? (mapCodeToId.texts[key] ?? "") : "";
            instructions.Add(
              new UpdatePartConfigurationInstruction(0, new VariableUpdate(variableId,  new VariableValue(0, textValue, null, null, null)), null)
            );
        }
      }
      if (webConfigState.booleans != null) {
        foreach (string key in webConfigState.booleans.Keys) { 
            var booleanValue = webConfigState.booleans[key];
            string variableId = mapCodeToId.booleans.ContainsKey(key) ? (mapCodeToId.booleans[key] ?? "") : "";
            instructions.Add(
              new UpdatePartConfigurationInstruction(0, new VariableUpdate(variableId,  new VariableValue(0, null, booleanValue, null, null)), null)
            );
        }
      }
      if (webConfigState.selections != null) {
        foreach (string key in webConfigState.selections.Keys) { 
            WebSelectionRowItem[] rowSelections = webConfigState.selections[key] ?? [];
            string selectionId = mapCodeToId.selectionGroups.ContainsKey(key) ? (mapCodeToId.selectionGroups[key] ?? "") : "";
          
            var allRows = mapCodeToId.selectionGroupRowIds[key];
            
            List<SelectionGroupRowUpdate> rowUnselectInstructions = new List<SelectionGroupRowUpdate>();

            // First - Add instruction to unselect all rows first
            for (int i = 0; i<allRows.Count; i++) {
              var row = allRows[i];
              rowUnselectInstructions.Add(new SelectionGroupRowUpdate(row.Id, row.PartId, false, null)); 
            }

            // Secondly - change rows that exist in web selection
            List<SelectionGroupRowUpdate> rowUpdateInstructions = new List<SelectionGroupRowUpdate>();
            foreach (SelectionGroupRowUpdate rowUpdateInstruction in rowUnselectInstructions) {
              var rowToSelect = Array.Find(rowSelections, item => {
                List<string> possibleRowIds = mapCodeToId.selectionRows[item.selection];
                return possibleRowIds.Contains(rowUpdateInstruction.SelectionGroupRowId);
              });
              var rowInstruction = rowUpdateInstruction;
              if (rowToSelect != null) {
                rowInstruction = new SelectionGroupRowUpdate(rowUpdateInstruction.SelectionGroupRowId, rowUpdateInstruction.PartId, true, rowToSelect.quantity);
              }
              instructions.Add(new UpdatePartConfigurationInstruction(1, null, rowInstruction));
            }
        }
      }
    }
    
    var configurationUpdate = new { SessionId = sessionId, Instructions = instructions };
    string json = JsonSerializer.Serialize(configurationUpdate,  new 
     System.Text.Json.JsonSerializerOptions()  { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull 
    });
    return json;
  }

  public string getConfiguratorDefinition (string partConfigurationStateJSON, string partNumberListJSON) {

    PartConfigurationState? partConfigurationState = JsonSerializer.Deserialize<PartConfigurationState>(partConfigurationStateJSON, 
      new JsonSerializerOptions(JsonSerializerDefaults.General)
    );

    List<PartNumberMap>? partIdList = JsonSerializer.Deserialize<List<PartNumberMap>>(partNumberListJSON, new JsonSerializerOptions(JsonSerializerDefaults.General) ); // todo: Replace when $expand is suppoted for PartNumber on PartConfigurationState (issue submitted to support)

    if (partConfigurationState != null) {
      appendMissingDataToPartConfigurationStateSection(partConfigurationState.Sections, partIdList ?? []);
    }

    string json = JsonSerializer.Serialize(partConfigurationState,  new 
     System.Text.Json.JsonSerializerOptions()  { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull 
    });

    return json;
  }
}
