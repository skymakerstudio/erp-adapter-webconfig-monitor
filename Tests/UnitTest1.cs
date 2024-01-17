namespace Tests;
using AdapterLibrary;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
      AdapterLibrary.Class1 c = new AdapterLibrary.Class1();
      string result = c.HelloWorld();

      Assert.AreEqual(result, "Hello World");
    }
}
