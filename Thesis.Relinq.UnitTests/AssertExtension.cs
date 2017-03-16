using Newtonsoft.Json;
using NUnit.Framework;

namespace Thesis.Relinq.UnitTests
{
    public static class AssertExtension
    {
       public static void AreEqualByJson(object expected, object actual)
        {
            string expectedJson = JsonConvert.SerializeObject(expected, Formatting.Indented);
            string actualJson = JsonConvert.SerializeObject(actual, Formatting.Indented);
            Assert.AreEqual(expectedJson, actualJson);
        }
    }
}