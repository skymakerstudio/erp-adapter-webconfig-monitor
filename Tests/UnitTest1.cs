namespace Tests;
using AdapterLibrary;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
      AdapterLibrary.MonitorAPI adapter = new AdapterLibrary.MonitorAPI();
      string partStateApiResponse1 = "{\"Sections\":[]}";

      string result = adapter.configurationToWeb(partStateApiResponse1);

      string partExpectedState1 = "{\"Variables\":[]}";
      Assert.AreEqual(result, partExpectedState1);
    }
}
