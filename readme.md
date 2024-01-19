# Monitor G5 PartConfigration web adapter
Library for making complex configuration structures in ERP Monitor G5 easy to work with in a web context

## Transform G5 configuraiton to Web
```C#
private static readonly HttpClient client = new HttpClient();
var apiUrl = "https://localhost:8001/sv/001.1/api/v1/";
var args = new Dictionary<string, string>
  {
      { "SessionId", "XXXXX" },
  };

var content = new FormUrlEncodedContent(args);
var configResponse = await client.postAsync(apiUrl + "Common/PartConfigurations/Get", content);
var partConfigState = await configResponse.Content.ReadAsStringAsync(); // PartConfigurationState

var partIdListResponse = client.postAsync(apiUrl + "Inventory/Parts");

var partIdList = await partIdListResponse.Content.ReadAsStringAsync();
string result = adapter.configurationToWeb(partConfigState, partIdList);

```

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